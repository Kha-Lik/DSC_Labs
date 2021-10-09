using System;

namespace UdpBroadcastServer
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the port to start on:");
            var port = int.Parse(Console.ReadLine() ?? "6699");
            
            var server = new Server(port, "127.0.0.1");
            server.Init();

            bool flag = true;
            while (flag)
            {
                Console.WriteLine("==================\n" +
                                  "\t1 - Send message\n" +
                                  "\t2 - Exit\n");
                var input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        Console.WriteLine("Enter message:\n");
                        var msg = Console.ReadLine();
                        server.SendMessage(msg);
                        break;
                    case "2":
                        flag = false;
                        break;
                    default:
                        Console.WriteLine("Error! Unexpected input!");
                        break;
                }
            }
        }
    }
}