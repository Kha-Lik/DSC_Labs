using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UdpBroadcastClient
{
    public class Client
    {
        private readonly int _port;
        private Socket _listeningSocket;

        public Client(int port)
        {
            _port = port;
        }

        public void Start()
        {
            try
            {
                _listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                var listeningTask = new Task(Listen);
                Console.WriteLine($"Started listening on port {_port}");
                listeningTask.Start();
                while (true)
                {
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Close();
            }
        }
        
        private void Listen()
        {
            try
            {
                IPEndPoint localIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port);
                _listeningSocket.Bind(localIp);
 
                while (true)
                {
                    StringBuilder builder = new StringBuilder();
                    byte[] data = new byte[256]; 
 
                    EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
 
                    do
                    {
                        var bytes = _listeningSocket.ReceiveFrom(data, ref remoteIp);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (_listeningSocket.Available > 0);
                    IPEndPoint remoteFullIp = remoteIp as IPEndPoint;
                    
                    Console.WriteLine($"{remoteFullIp.Address}:{remoteFullIp.Port} - {builder}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Close();
            }
        }

        private void Close()
        {
            if (_listeningSocket == null) return;
            
            _listeningSocket.Shutdown(SocketShutdown.Both);
            _listeningSocket.Close();
            _listeningSocket = null;
        }
    }
}