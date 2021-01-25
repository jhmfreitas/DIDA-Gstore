using GStoreServer.Domain;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace GStoreServer.Controllers
{
    class ReadRecoveryController
    {

        private static async Task<ReadRecoveryReply> ExecuteServerAsync(MasterReplicaService.MasterReplicaServiceClient Stub, GStoreObjectIdentifier gStoreObjectIdentifier)
        {
            ReadRecoveryRequest request = new ReadRecoveryRequest
            {
                ObjectIdentifier = new ObjectIdentifierDto
                {
                    PartitionId = gStoreObjectIdentifier.PartitionId,
                    ObjectId = gStoreObjectIdentifier.ObjectId
                }
            };

            return await Stub.ReadRecoveryAsync(request);
        }


        public static async Task<string> ExecuteAsync(ConnectionManager connectionManager, GStoreObjectIdentifier gStoreObjectIdentifier)
        {
            string partitionId = gStoreObjectIdentifier.PartitionId;
            IImmutableSet<Server> partitionServers = connectionManager.GetAliveServers(partitionId);

            List<Task<ReadRecoveryReply>> tasks = new List<Task<ReadRecoveryReply>>();

            foreach (Server server in partitionServers)
            {
                Task<ReadRecoveryReply> t = ExecuteServerAsync(server.Stub, gStoreObjectIdentifier);
                tasks.Add(t);
            }

            foreach (Task<ReadRecoveryReply> t in tasks)
            {
                ReadRecoveryReply reply = await t;
                if (reply.Valid)
                {
                    return reply.Object.Value;
                }
            }
            return null;
        }
    }
}
