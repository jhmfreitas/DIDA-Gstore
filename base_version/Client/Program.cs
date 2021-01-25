using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Client.Commands;
using Client.Domain;
using Utils;
using Grpc.Core;
using System.Diagnostics;
using Grpc.Net.Client;
using System.Linq;
using Client.Controllers;

namespace Client
{

    class Program
    {

        private static void RegisterCommands(CommandDispatcher commandDispatcher, ConnectionManager connectionManager)
        {
            commandDispatcher.Register("read", new ReadCommand(connectionManager));
            commandDispatcher.Register("write", new WriteCommand(connectionManager));
            commandDispatcher.Register("listServer", new ListServerCommand(connectionManager));
            commandDispatcher.Register("listGlobal", new ListGlobalCommand(connectionManager));
            commandDispatcher.Register("wait", new WaitCommand());
        }

        public static ConnectionManager CreateConnectionManager(string networkConfiguration)
        {
            ConnectionManager connectionManager = CreateClientConnectionManager(networkConfiguration);
            Console.WriteLine(connectionManager);
            return connectionManager;
        }

        public static void PressToExit()
        {
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey();
            }
            while (keyInfo.Key != ConsoleKey.Enter);
        }

        static async Task Main(string[] args)
        {            
            try
            {
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

                string username = args[0];
                string url = args[1];
                string[] protocolAndHostnameAndPort = url.Split("://");
                string[] hostnameAndPort = protocolAndHostnameAndPort[1].Split(":");
                int port = int.Parse(hostnameAndPort[1]);
                string filename = args[2] + ".txt";
                string networkConfiguration = args[3];
                string[] lines;
                lines = System.IO.File.ReadAllLines(filename);

                CommandDispatcher commandDispatcher = new CommandDispatcher();
                ConnectionManager connectionManager = CreateConnectionManager(networkConfiguration);
                RegisterCommands(commandDispatcher, connectionManager);

                Grpc.Core.Server server = new Grpc.Core.Server
                {
                    Services =
                    {
                        PuppetMasterClientService.BindService(new PuppetmasterClientServiceImpl(connectionManager))
                    },
                    Ports = { new ServerPort(hostnameAndPort[0], port, ServerCredentials.Insecure) }
                };
                Console.WriteLine("Client listening on port " + port);

                server.Start();

                List<string> preprocessed = CommandPreprocessor.Preprocess(lines);

                await commandDispatcher.ExecuteAllAsync(preprocessed.ToArray());

                Console.WriteLine("Press ENTER to stop the client...");
                PressToExit();

                Console.WriteLine("\nShutting down...");
                server.ShutdownAsync().Wait();

            }
            catch (System.IO.FileNotFoundException e)
            {
                Console.WriteLine("ERROR: File " + args[2] + " not found in current directory.");
                Console.WriteLine(e);
                PressToExit();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                PressToExit();
                return;
            }
        }

        private static ConnectionManager CreateClientConnectionManager(string networkConfiguration)
        {
            InitializationParser initializationParser = new InitializationParser(networkConfiguration);
            List<Tuple<string, string>> serversConfiguration = initializationParser.GetServersConfiguration();
            List<Tuple<string, List<string>>> partitionsConfiguration = initializationParser.GetPartitionsConfiguration();

            IDictionary<string, Domain.Server> servers = new Dictionary<string, Domain.Server>();
            IDictionary<string, Partition> partitions = new Dictionary<string, Partition>();
            for (int i = 0; i < serversConfiguration.Count; i++)
            {
                Tuple<string, string> serverConfig = serversConfiguration[i];
                string serverId = serverConfig.Item1;
                string address = serverConfig.Item2;
                GrpcChannel channel = GrpcChannel.ForAddress(address);
                GStoreService.GStoreServiceClient stub = new GStoreService.GStoreServiceClient(channel);
                Domain.Server server = new Domain.Server(serverId, stub);
                servers.Add(serverId, server);
            }

            foreach (Tuple<string, List<string>> partitionConfig in partitionsConfiguration)
            {
                string partitionId = partitionConfig.Item1;
                string masterId = partitionConfig.Item2.ElementAt(0);
                ISet<string> partitionReplicaSet = new HashSet<string>();

                foreach (string serverId in partitionConfig.Item2)
                {
                    if (serverId != masterId) partitionReplicaSet.Add(serverId);
                }

                Partition partition = new Partition(partitionId, masterId, partitionReplicaSet);
                partitions.Add(partitionId, partition);
            }

            return new ConnectionManager(servers, partitions);
        }
    }
}
