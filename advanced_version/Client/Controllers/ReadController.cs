using Client.Domain;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Utils;

namespace Client.Controllers
{
    class ReadController
    {
        public static async Task<GStoreObject> Execute(ConnectionManager connectionManager, string partitionId, string serverId, string objectId)
        {

            Server server = null;
            try
            {
                server = connectionManager.ChooseServer(partitionId, serverId);
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
                GStoreReadRequest gStoreReadRequest = new GStoreReadRequest()
                {
                    ObjectIdentifier = new DataObjectIdentifier
                    {
                        PartitionId = partitionId,
                        ObjectId = objectId
                    }
                };

                try
                {
                    GStoreReadReply gStoreReadReply = await server.Stub.ReadAsync(gStoreReadRequest);

                    return CreateObject(gStoreReadReply.Object);
                }
                catch (Grpc.Core.RpcException e) when (e.StatusCode == Grpc.Core.StatusCode.Internal)
                {
                    connectionManager.DeclareDead(server.Id);
                    server = null;
                }

            }
        }

        private static GStoreObject CreateObject(DataObject gStoreObject)
        {
            GStoreObjectIdentifier gStoreObjectIdentifier = new GStoreObjectIdentifier(gStoreObject.ObjectIdentifier.PartitionId, gStoreObject.ObjectIdentifier.ObjectId);
            return new GStoreObject(gStoreObjectIdentifier, gStoreObject.Value);
        }
    }
}
