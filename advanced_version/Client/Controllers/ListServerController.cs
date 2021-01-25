using Client.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace Client.Controllers
{
    class ListServerController
    {

        public static async Task<HashSet<GStoreObject>> Execute(ConnectionManager connectionManager, string serverId)
        {
            try
            {
                Server server;
                server = connectionManager.GetAliveServer(serverId);
                GStoreListServerReply gStoreListServerReply = await server.Stub.ListServerAsync(new Google.Protobuf.WellKnownTypes.Empty());

                HashSet<GStoreObject> gStoreObjects = new HashSet<GStoreObject>();
                foreach (DataObject dataObject in gStoreListServerReply.Objects)
                {
                    gStoreObjects.Add(CreateObject(dataObject));
                }
                return gStoreObjects;
            }
            catch (Grpc.Core.RpcException e) when (e.StatusCode == Grpc.Core.StatusCode.Internal)
            {
                connectionManager.DeclareDead(serverId);
                return null;
            }
        }

        private static GStoreObject CreateObject(DataObject dataObject)
        {
            GStoreObjectIdentifier gStoreObjectIdentifier = new GStoreObjectIdentifier(dataObject.ObjectIdentifier.PartitionId, dataObject.ObjectIdentifier.ObjectId);
            return new GStoreObject(gStoreObjectIdentifier, dataObject.Value);
        }
    }
}
