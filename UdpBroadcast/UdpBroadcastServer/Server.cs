using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UdpBroadcastServer
{
    public class Server
    {
        private const string Path = @"./clients.txt";
        private readonly int _port;
        private readonly string _ip;
        private List<IPEndPoint> _clients;
        private Socket _socket;
        
        
        public Server(int port, string ip)
        {
            _port = port;
            _ip = ip;
            
            ParseClients();
        }

        public void Init()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var endpoint = new IPEndPoint(IPAddress.Parse(_ip), _port);
            _socket.Bind(endpoint);
        }

        public void SendMessage(string message)
        {
            foreach (var ipEndPoint in _clients)
            {
                var data = Encoding.Unicode.GetBytes(message);
                _socket.SendTo(data, ipEndPoint);
                Console.WriteLine($"Sent message to client {ipEndPoint.Address}:{ipEndPoint.Port}");
            }
        }

        private void ParseClients()
        {
            if (!File.Exists(Path)) throw new FileNotFoundException(Path);

            var lines = File.ReadAllLines(Path);

            List<Tuple<string, int>> adresses = lines
                .Select(l => l.Split(':'))
                .Select(pair => new Tuple<string, int>(pair[0], int.Parse(pair[1]))).ToList();

            _clients = adresses.Select(t => new IPEndPoint(IPAddress.Parse(t.Item1), t.Item2)).ToList();
        }
    }
}