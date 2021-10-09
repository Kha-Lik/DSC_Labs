using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UdpBroadcastClient
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter the port to listen on: ");
            var port = int.Parse(Console.ReadLine() ?? "5555");

            var client = new Client(port);
            client.Start();
        }
    }
}