using System.Net.Sockets;

namespace DeviceDataSimulatorService.Transport.TCPClient
{
    public class TCPClient
    {
        private TcpClient _tcpClient;

        public string LocalIP { get; private set; }
        public int LocalPort { get; private set; }
        public string RemoteIP { get; private set; }
        public int RemotePort { get; private set; }

        private object _tcpLock = new object();

        public TCPClient()
        {
            lock (_tcpLock)
            {
                _tcpClient = new TcpClient();
            }
        }

        public void Connect(string remoteIP, int remotePort)
        {
            lock (_tcpLock)
            {
                this.RemoteIP = remoteIP;
                this.RemotePort = remotePort;
                if (!_tcpClient.Client.Connected)
                    _tcpClient.Connect(remoteIP, remotePort);
            }
        }

        public int SendData(byte[] buffer, int len)
        {
            lock (_tcpLock)
            {
                if (_tcpClient.Client.Connected)
                {
                    NetworkStream stream = _tcpClient.GetStream();
                    stream.Write(buffer, 0, len);
                }
            }

            return buffer.Length;

        }

        public void Close()
        {
            lock (_tcpLock)
            {
                if (_tcpClient.Client.Connected)
                    if (_tcpClient != null) _tcpClient.Close();
            }
        }
    }
}

