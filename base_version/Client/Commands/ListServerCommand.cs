using Client.Controllers;
using Client.Domain;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils;

namespace Client.Commands
{
    class ListServerCommand : Command
    {
        public static int EXPECTED_ARGUMENTS = 1;

        private ConnectionManager ConnectionManager;

        public ListServerCommand(ConnectionManager connectionManager) : base(false)
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

            string serverId = arguments.ElementAt(0);

            Console.WriteLine($"List Server: {serverId}");

            try
            {
                HashSet<GStoreObjectReplica> gStoreObjectReplicas = await ListServerController.Execute(ConnectionManager, serverId);
                if (gStoreObjectReplicas == null)
                {
                    Console.WriteLine($"=> Server {serverId} crashed.");
                    return;
                }
                foreach (GStoreObjectReplica replica in gStoreObjectReplicas)
                {
                    Console.WriteLine($"=> {replica.Object.Identifier.PartitionId}, {replica.Object.Identifier.ObjectId}, {replica.Object.Value}, {(replica.IsMaster ? "Master" : "Replica")}");
                }
            }
            catch (ServerBindException e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Internal)
            {
                Console.WriteLine($"Could not establish connection with server.");
            }
        }
    }
}
