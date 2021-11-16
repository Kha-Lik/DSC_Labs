using System;
using System.Threading.Tasks;
using Grpc.Core;

namespace TheSmartest.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Enter the address of the server you want to connect to in format [127.0.0.1:56001]");
            var input = Console.ReadLine();
            var channel = new Channel(input, ChannelCredentials.Insecure);
            var client = new Game.GameClient(channel);
            var service = new GameService(client);

            await service.Start();
            
            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}