using GStoreServer.Domain;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Utils;

namespace GStoreServer.Controllers
{
    class WriteReplicaController
    {
        private static async Task ExecuteServerAsync(ConnectionManager connectionManager, MasterReplicaService.MasterReplicaServiceClient stub, GStoreObject gStoreObject, int version)
        {
            WriteRequest writeRequest = new WriteRequest
            {
                Object = new ObjectDto
                {
                    ObjectIdentifier = new ObjectIdentifierDto
                    {
                        PartitionId = gStoreObject.Identifier.PartitionId,
                        ObjectId = gStoreObject.Identifier.ObjectId
                    },
                    Value = gStoreObject.Value
                },
                ServerId = connectionManager.SelfServerId,
                Version = version
            };

            await stub.WriteAsync(writeRequest);
        }

        public static async Task ExecuteAsync(ConnectionManager connectionManager, GStoreObject gStoreObject, int version)
        {
            GStoreObjectIdentifier gStoreObjectIdentifier = gStoreObject.Identifier;

            // Get all replicas associated to this Partition
            IImmutableSet<Server> servers = connectionManager.GetAliveServers(gStoreObjectIdentifier.PartitionId);

            IDictionary<string, Task> writeTasks = new Dictionary<string, Task>();
            foreach (Server server in servers)
            {
                if (server.Id != connectionManager.SelfServerId)
                {
                    writeTasks.Add(server.Id, ExecuteServerAsync(connectionManager, server.Stub, gStoreObject, version));
                }
            }

            foreach (KeyValuePair<string, Task> writeTaskPair in writeTasks)
            {
                string serverId = writeTaskPair.Key;
                try
                {
                    await writeTaskPair.Value;
                }
                catch (Grpc.Core.RpcException e) when (e.StatusCode == Grpc.Core.StatusCode.Internal)
                {
                    connectionManager.DeclareDead(serverId);
                }
            }
        }
    }
}
