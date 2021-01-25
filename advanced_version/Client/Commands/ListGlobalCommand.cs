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

            IDictionary<string, Task<HashSet<GStoreObject>>> listServerTaskPairs = new Dictionary<string, Task<HashSet<GStoreObject>>>();

            foreach (Server server in ConnectionManager.GetAliveServers())
            {
                listServerTaskPairs.Add(server.Id, ListServerController.Execute(ConnectionManager, server.Id));
            }

            foreach(KeyValuePair<string, Task<HashSet<GStoreObject>>> listServerTaskPair in listServerTaskPairs)
            {
                string serverId = listServerTaskPair.Key;
                Task<HashSet<GStoreObject>> task = listServerTaskPair.Value;

                HashSet<GStoreObject> gStoreObjects = await task;

                Console.WriteLine($"List Server: {serverId}");
                if (gStoreObjects == null)
                {
                    Console.WriteLine($"=> Server {serverId} crashed.");
                    continue;
                }
                foreach (GStoreObject gStoreObject in gStoreObjects)
                {
                    Console.WriteLine($"=> {gStoreObject.Identifier.PartitionId}, {gStoreObject.Identifier.ObjectId}, {gStoreObject.Value}");
                }
            }
        }
    }
}
