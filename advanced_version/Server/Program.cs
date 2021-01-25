using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using GStoreServer.Domain;
using GStoreServer.Services;
using Utils;

namespace GStoreServer
{
    class Program
    {
        private static string myServerId;

        public static void PressToExit()
        {
            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey();
            }
            while (keyInfo.Key != ConsoleKey.Enter);
        }
        static void Main(string[] args)
        {
            try
            {
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                myServerId = args[0];
                Console.WriteLine($"ServerId: {myServerId}");
                string[] protocolAndHostnameAndPort = args[1].Split("://");
                string[] hotnameAndPort = protocolAndHostnameAndPort[1].Split(":");
                int port = int.Parse(hotnameAndPort[1]);
                int minDelay = int.Parse(args[2]);
                int maxDelay = int.Parse(args[3]);

                ConnectionManager connectionManager = CreateServerConnectionManager(args[4]);
                GStore gStore = new GStore(connectionManager);
                connectionManager.AddGStore(gStore);
                Console.WriteLine(connectionManager);

                ManualResetEventSlim freezeLock = new ManualResetEventSlim(true);
                RequestInterceptor requestInterceptor = new RequestInterceptor(freezeLock, minDelay, maxDelay);

                Grpc.Core.Server server = new Grpc.Core.Server
                {
                    Services =
                {
                    GStoreService.BindService(new ServerServiceImpl(gStore)).Intercept(requestInterceptor),
                    MasterReplicaService.BindService(new MasterReplicaServiceImpl(gStore)).Intercept(requestInterceptor),
                    PuppetMasterServerService.BindService(new PuppetMasterServerServiceImpl(freezeLock, connectionManager))
                },
                    Ports = { new ServerPort(hotnameAndPort[0], port, ServerCredentials.Insecure) }
                };

                server.Start();
                Console.WriteLine("GStore server listening on port " + port);
                Console.WriteLine("Press any key to stop the server...");

                PressToExit();
                Console.WriteLine("\nShutting down...");
                server.ShutdownAsync().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                PressToExit();
            }
        }

        private static ConnectionManager CreateServerConnectionManager(string networkConfiguration)
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
                if (myServerId != serverId)
                {
                    string address = serverConfig.Item2;
                    GrpcChannel channel = GrpcChannel.ForAddress(address);
                    MasterReplicaService.MasterReplicaServiceClient stub = new MasterReplicaService.MasterReplicaServiceClient(channel);
                    Domain.Server server = new Domain.Server(serverId, stub);
                    servers.Add(serverId, server);
                }
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
            ConnectionManager connectionManager = new ConnectionManager(servers, partitions, myServerId);
            return connectionManager;
        }
    }
}
