using System;
using Grpc.Core;

namespace TheSmartest.Server
{
    internal static class Program
    {
        private const int Port = 56001;
        public static void Main(string[] args)
        {
            var questionSource = new JsonQuestionSource(@".\questions.json");
            var questionService = new QuestionService(questionSource);
            
            var server = new Grpc.Core.Server
            {
                Services = { Game.BindService(new GameServicesImplementation(questionService)) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine($"TheSmartest server listening on port {Port}");
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}