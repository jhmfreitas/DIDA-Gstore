using Client.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils;
using Client.Controllers;

namespace Client.Commands
{

    class ListGlobalCommand : Command
    {
        public static int EXPECTED_ARGUMENTS = 0;

        private readonly ConnectionManager ConnectionManager;

        public ListGlobalCommand(ConnectionManager connectionManager) : base(false)
        {
            ConnectionManager = connectionManager ?? throw new ArgumentNullException("ConnectionManager cannot be null.");
        }

        public override async Task ExecuteAsync(List<string> arguments)
        {
            if (arguments.Count != EXPECTED_ARGUMENTS)
            {
                Console.WriteLine("Expected " + EXPECTED_ARGUMENTS + " arguments but found " + arguments.Count + ".");
                return;
            }
            Console.WriteLine("listGlobal");

            IDictionary<string, Task<HashSet<GStoreObjectReplica>>> listServerTaskPairs = new Dictionary<string, Task<HashSet<GStoreObjectReplica>>>();

            foreach (Server server in ConnectionManager.GetAliveServers())
            {
                listServerTaskPairs.Add(server.Id, ListServerController.Execute(ConnectionManager, server.Id));
            }

            foreach(KeyValuePair<string, Task<HashSet<GStoreObjectReplica>>> listServerTaskPair in listServerTaskPairs)
            {
                string serverId = listServerTaskPair.Key;
                Task<HashSet<GStoreObjectReplica>> task = listServerTaskPair.Value;

                HashSet<GStoreObjectReplica> gStoreObjectReplicas = await task;

                Console.WriteLine($"List Server: {serverId}");
                if (gStoreObjectReplicas == null)
                {
                    Console.WriteLine($"=> Server {serverId} crashed.");
                    continue;
                }
                foreach (GStoreObjectReplica replica in gStoreObjectReplicas)
                {
                    Console.WriteLine($"=> {replica.Object.Identifier.PartitionId}, {replica.Object.Identifier.ObjectId}, {replica.Object.Value}, {(replica.IsMaster ? "Master" : "Replica")}");
                }
            }
        }
    }
}
