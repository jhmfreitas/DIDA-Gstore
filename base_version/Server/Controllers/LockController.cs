using GStoreServer.Domain;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Utils;

namespace GStoreServer.Controllers
{
    class LockController
    {
        private static async Task<int> ExecuteReplicaAsync(MasterReplicaService.MasterReplicaServiceClient Stub, GStoreObjectIdentifier gStoreObjectIdentifier)
        {
            LockRequest lockRequest = new LockRequest
            {
                ObjectIdentifier = new ObjectIdentifierDto
                {
                    PartitionId = gStoreObjectIdentifier.PartitionId,
                    ObjectId = gStoreObjectIdentifier.ObjectId
                }
            };

            LockReply lockReply = await Stub.LockAsync(lockRequest);
            return lockReply.LockId;
        }

        public static async Task<IDictionary<string, int>> ExecuteAsync(ConnectionManager connectionManager, GStoreObjectIdentifier gStoreObjectIdentifier)
        {
            // Get all replicas associated to this Partition
            IImmutableSet<Server> replicas = connectionManager.GetPartitionAliveReplicas(gStoreObjectIdentifier.PartitionId);

            IDictionary<string, Task<int>> lockTasks = new Dictionary<string, Task<int>>();
            foreach (Server replica in replicas)
            {
                lockTasks.Add(replica.Id, ExecuteReplicaAsync(replica.Stub, gStoreObjectIdentifier));
            }

            // Await for lock requests and save their values
            IDictionary<string, int> lockValues = new Dictionary<string, int>();
            foreach (KeyValuePair<string, Task<int>> lockTaskPair in lockTasks)
            {
                string replicaId = lockTaskPair.Key;
                try
                {
                    lockValues.Add(replicaId, await lockTaskPair.Value);
                }
                catch (Grpc.Core.RpcException e) when (e.StatusCode == Grpc.Core.StatusCode.Internal)
                {
                    connectionManager.DeclareDead(replicaId);
                }
            }
            return lockValues;
        }
    }
}
