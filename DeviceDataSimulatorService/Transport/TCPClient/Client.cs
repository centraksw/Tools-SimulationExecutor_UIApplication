namespace DeviceDataSimulatorService.Transport.TCPClient
{
    public class Client
    {
        private static TCPClient _client;
        public static void CreateTcpClient()
        {
            _client = new TCPClient();
        }

        public static void Connect(string ip, int port)
        {
            _client.Connect(ip, port);
        }

        public static void SendData(byte[] buffer)
        {
            int size = _client.SendData(buffer);
        }

        public static void Close()
        {
            if (_client != null) _client.Close();
        }
    }
}
