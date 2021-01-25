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
    class ReadCommand : Command
    {
        public static int EXPECTED_ARGUMENTS = 3;

        private readonly ConnectionManager ConnectionManager;

        public ReadCommand(ConnectionManager connectionManager) : base(false)
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

            string partitionId = arguments.ElementAt(0);
            string objectId = arguments.ElementAt(1);
            string serverId = arguments.ElementAt(2);

            try
            {
                Console.WriteLine($"Read... {partitionId} {objectId} {serverId}");
                GStoreObject gstoreObject = await ReadController.Execute(ConnectionManager, partitionId, serverId, objectId);
                Console.WriteLine($"PartitionId: {gstoreObject.Identifier.PartitionId} | ObjectId: {gstoreObject.Identifier.ObjectId} | Value: {gstoreObject.Value}");
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
