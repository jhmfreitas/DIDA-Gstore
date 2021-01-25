using Client.Domain;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Utils;

namespace Client.Controllers
{
    class GetMasterController
    {
        public static int RETRY_DELAY = 1000;
        public static async Task<string> Execute(ConnectionManager connectionManager, string partitionId)
        {
            string masterId = connectionManager.GetPartitionMasterId(partitionId);
            IImmutableSet<Server> replicaSet = connectionManager.GetPartitionAliveReplicas(partitionId);
            foreach (Server replica in replicaSet)
            {
                try
                {
                    while (true)
                    {
                        GStoreGetMasterResponse gStoreGetMasterResponse = await replica.Stub.GetMasterAsync(new GStoreGetMasterRequest
                        {
                            PartitionId = partitionId
                        });
                        string newMasterId = gStoreGetMasterResponse.MasterId;
                        if (newMasterId != masterId)
                        {
                            masterId = newMasterId;
                            break;
                        }
                        await Task.Delay(RETRY_DELAY);
                    }
                    break;
                }
                catch (Grpc.Core.RpcException exception) when (exception.StatusCode == Grpc.Core.StatusCode.Internal)
                {
                    // should handle this error
                    Console.WriteLine(exception.Message);
                }    
            }
            return masterId;
        }

    }
}
