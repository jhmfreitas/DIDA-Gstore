using GStoreServer.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils;

namespace GStoreServer.Controllers
{
    class WriteReplicaController
    {
        private static async Task ExecuteReplicaAsync(ConnectionManager connectionManager, string replicaId, GStoreObject gStoreObject, int lockId)
        {
            WriteRequest writeRequest = new WriteRequest
            {
                LockId = lockId,
                Object = new ObjectDto
                {
                    ObjectIdentifier = new ObjectIdentifierDto
                    {
                        PartitionId = gStoreObject.Identifier.PartitionId,
                        ObjectId = gStoreObject.Identifier.ObjectId
                    },
                    Value = gStoreObject.Value
                }
            };

            Server replica = connectionManager.GetAliveServer(replicaId);
            await replica.Stub.WriteAsync(writeRequest);
        }

        public static async Task ExecuteAsync(ConnectionManager connectionManager, GStoreObject gStoreObject, IDictionary<string, int> replicaLocks)
        {
            IDictionary<string, Task> writeTasks = new Dictionary<string, Task>();
            foreach(KeyValuePair<string, int> replicaLock in replicaLocks) 
            {
                string replicaId = replicaLock.Key;
                int lockId = replicaLock.Value;
                writeTasks.Add(replicaId, ExecuteReplicaAsync(connectionManager, replicaId, gStoreObject, lockId));
            }

            // Await lock write requests
            foreach (KeyValuePair<string, Task> writeTaskPair in writeTasks)
            {
                string replicaId = writeTaskPair.Key;
                try
                {
                    await writeTaskPair.Value;
                }
                catch (Grpc.Core.RpcException e) when (e.StatusCode == Grpc.Core.StatusCode.Internal)
                {
                    connectionManager.DeclareDead(replicaId);
                }
            }
        }
    }
}
