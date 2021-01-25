using Client.Domain;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Utils;

namespace Client.Controllers
{
    class WriteController
    {
        public static async Task Execute(ConnectionManager connectionManager, string partitionId, string objectId, string value)
        {
            Server server = null;
            try
            {
                server = connectionManager.ChooseServer(partitionId, "-1");
                if (!server.Alive) server = null;
            }
            catch (ServerBindException)
            {
                // nothing
            }

            while (true)
            {
                if (server == null)
                {
                    IImmutableSet<Server> replicas = connectionManager.GetAliveServers(partitionId);
                    Random rnd = new Random();
                    server = replicas.ElementAt(rnd.Next(0, replicas.Count));
                    connectionManager.Attach(server);
                }

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
                    connectionManager.DeclareDead(server.Id);
                    server = null;
                }
            }
        }
    }
}
