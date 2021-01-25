using System;
using Grpc.Core;

namespace PCS
{
    class Program
    {
        private const int PORT = 10000;

        static void Main(string[] args)
        {

            Server server = new Server
            {
                Services = { PuppetMasterPCSService.BindService(new PCSServiceImpl()) },
                Ports = { new ServerPort("localhost", PORT, ServerCredentials.Insecure) }
            };
            server.Start();
            Console.WriteLine("PCS listening on port " + PORT);
            Console.WriteLine("Press ENTER to stop the PCS...");
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey();
            }
            while (keyInfo.Key != ConsoleKey.Enter);

            server.ShutdownAsync().Wait();

        }

    }
}
