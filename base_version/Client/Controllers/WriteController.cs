using Client.Domain;
using System;
using System.Threading.Tasks;

namespace Client.Controllers
{
    class WriteController
    {
        public static async Task Execute(ConnectionManager connectionManager, string partitionId, string objectId, string value)
        {
            while (true)
            {
                Server server = connectionManager.ChooseServerForWrite(partitionId);
                Console.WriteLine($"Trying: {server.Id}");

                GStoreWriteRequest writeRequest = new GStoreWriteRequest()
                {
                    Object = new DataObject()
                    {
                        ObjectIdentifier = new DataObjectIdentifier
                        {
                            PartitionId = partitionId,
                            ObjectId = objectId
                        },
                        Value = value
                    }
                };

                try
                {
                    await server.Stub.WriteAsync(writeRequest);
                    return;
                }
                catch (Grpc.Core.RpcException e) when (e.StatusCode == Grpc.Core.StatusCode.Internal)
                {
                    await connectionManager.DeclareDead(server.Id);
                }
            }            
        }
    }
}
