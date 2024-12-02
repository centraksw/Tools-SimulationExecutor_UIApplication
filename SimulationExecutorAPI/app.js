const express = require('express');
const http = require('http');
const socketIo = require('socket.io');
const redis = require('redis');
const cors = require('cors');

const app = express();

// Create HTTP server
const server = http.createServer(app);
const io = socketIo(server, {
    cors: {
      origin: 'http://localhost:4200', // Allow requests from this origin
      methods: ['GET', 'POST'],
      credentials: true
    }
  });
  
  app.use(cors({
    origin: 'http://localhost:4200',
    methods: ['GET', 'POST'],
    credentials: true
  }));
  
app.use(express.static('public')); 

 
// Define a route for the root URL
app.get('/', (req, res) => {
    res.send('Hello, World!!!');  // Simple response for root URL
});
 
// Set up the Redis client for subscribing to a channel
const redisRes = redis.createClient({
    host: '192.168.0.104',
    port: 6379
});
 
// Connect to the Redis server
redisRes.connect()
    .then(() => {
        console.log('Connected to Redis!');
 
        // Event listener to handle errors
        redisRes.on('error', (err) => {
            console.log('Redis Client Error', err);
        });
 
    })
    .catch((err) => {
        console.error('Redis connection failed:', err);
    });
// Event listener to handle errors
redisRes.on('error', (err) => {
    console.log('Redis Client Error', err);
});
 
// Listen for messages on the Redis channel
redisRes.on('message', (message) => {
    console.log('message>>>', message);
    // Broadcast the message to all connected clients via Socket.IO
    io.emit('redisMessage', message);
});
 
// Subscribe to a channel named 'my_channel'
redisRes.subscribe('TEST', (message, count) => {
    if (message) {
         redisRes.emit('message', message);
    } 
    else {
        console.log('Successfully subscribed to ${count} channel(s)', count);
    }
});

io.on('connection', (socket) => {
    console.log('New client connected');
    socket.on('disconnect', () => {
      console.log('Client disconnected');
    });
});

app.post('/publish', async (req, res) => {
    const { channel, message } = req.body;
    await publisher.connect();
    await publisher.publish(channel, message);
    console.log("Channel: " + channel + "Message: " + message);
    res.send('Message published');
  });
 
// Start the HTTP server
server.listen(3000, function () {
    console.log('Server is running on port 3000');
});

