import { ApplicationRef, inject, Injectable } from '@angular/core';
import { Socket, io } from 'socket.io-client';
import { BehaviorSubject, first, Observable, Observer } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

export class SimulationDataService {
  private socket: Socket;
  private reconnectInterval = 2000;

  // Create a BehaviorSubject to track the socket connection status
  private connectionStatusSubject = new BehaviorSubject<string>('Disconnected.');
  public connectionStatus$ = this.connectionStatusSubject.asObservable();
  http: any;

  constructor() {
    this.socket = io('http://localhost:3000', { autoConnect: false });
    inject(ApplicationRef).isStable.pipe(
      first((isStable) => isStable))
      .subscribe(() => { this.connectSocket(); });
  }
  private connectSocket() {
    this.socket.connect();

    // Handle socket connection error and retry logic

    this.socket.on('connect_error', (error) => {
      this.connectionStatusSubject.next('Reconnecting.....');
      console.log('Socket connection failed: ', error);
      this.retryConnection();
    });

    // Handle successful socket connection

    this.socket.on('connect', () => {
      console.log('Socket connected successfully...');
      this.connectionStatusSubject.next('Connected.');
    });

    // Handle disconnection

    this.socket.on('disconnect', () => {
      console.log('Socket disconnected');
      this.retryConnection();
      this.connectionStatusSubject.next('Disconnected.');
    });
  }

  // Retry connection logic

  private retryConnection() {
    console.log('Retrying connection...');
    setTimeout(() => {
      this.socket.connect(); // Retry connection
    }, this.reconnectInterval);

  }

  observer!: Observer<any>;

  getQuotes(): Observable<any> {

    this.socket.on('redisMessage', (res) => {
      this.observer.next(res);
    });

    return this.createObservable();
  }

  createObservable(): Observable<any> {
    return new Observable<any>(observer => {
      this.observer = observer;
    });
  }

}
