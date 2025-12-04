using Interfaces;
using System.Net.Sockets;
using System.Windows;

namespace Services
{
    public class TcpConnection :ITcpConnnection
    {
        private TcpClient client = new TcpClient();
        public TcpClient Client { get => client; set => client = value; }
        public NetworkStream WorkStream { get => workStream; set => workStream = value; }

        private NetworkStream workStream;

        public async void StartListening() //持续接收与发送
        {
            while (true)
            {
                await Task.Delay(10000);
            }
        }

        void ITcpConnnection.Connect(string host, int port)
        {
            Client.Connect(host, port); 
            workStream = Client.GetStream();
            Task.Run(() => StartListening());
        }
    }
}
