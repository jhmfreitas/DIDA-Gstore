using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils;

namespace GStoreServer.Services
{
    class ServerServiceImpl : GStoreService.GStoreServiceBase
    {
        private GStore gStore;
        public ServerServiceImpl(GStore gStore)
        {
            this.gStore = gStore;
        }

        public override Task<GStoreReadReply> Read(GStoreReadRequest request, ServerCallContext context)
        {
            return Task.FromResult(ExecuteRead(request));
        }

        private GStoreReadReply ExecuteRead(GStoreReadRequest request)
        {
            Console.WriteLine($"Read request -> PartitionId: {request.ObjectIdentifier.PartitionId} ObjectId: {request.ObjectIdentifier.ObjectId}");
            GStoreObjectIdentifier gStoreObjectIdentifier = new GStoreObjectIdentifier(request.ObjectIdentifier.PartitionId, request.ObjectIdentifier.ObjectId);
            string value = gStore.Read(gStoreObjectIdentifier);

            if (value == null) value = "N/A";
            return new GStoreReadReply
            {
                Object = DataObjectBuilder.FromObjectIdentifier(request.ObjectIdentifier, value)
            };
        }

        public override Task<Empty> Write(GStoreWriteRequest request, ServerCallContext context)
        {
            return ExecuteWrite(request);
        }

        private async Task<Empty> ExecuteWrite(GStoreWriteRequest request)
        {
            Console.WriteLine($"Write request -> PartitionId: {request.Object.ObjectIdentifier.PartitionId} ObjectId: {request.Object.ObjectIdentifier} Value: {request.Object.Value}");

            GStoreObjectIdentifier gStoreObjectIdentifier = new GStoreObjectIdentifier(request.Object.ObjectIdentifier.PartitionId, request.Object.ObjectIdentifier.ObjectId);
            await gStore.Write(gStoreObjectIdentifier, request.Object.Value);

            return new Empty();
        }

        public override Task<GStoreListServerReply> ListServer(Empty request, ServerCallContext context)
        {
            return ExecuteListServer();
        }

        private async Task<GStoreListServerReply> ExecuteListServer()
        {
            Console.WriteLine($"ListServer request");

            ICollection<GStoreObjectReplica> gStoreObjectReplicas = await gStore.ReadAll();

            GStoreListServerReply reply = new GStoreListServerReply();
            foreach (GStoreObjectReplica gStoreObjectReplica in gStoreObjectReplicas)
            {
                reply.ObjectReplicas.Add(DataObjectReplicaBuilder.FromObjectReplica(gStoreObjectReplica));
            }
            return reply;
        }

        public override Task<GStoreGetMasterResponse> GetMaster(GStoreGetMasterRequest request, ServerCallContext context)
        {
            return Task.FromResult(ExecuteGetMaster(request));
        }

        public GStoreGetMasterResponse ExecuteGetMaster(GStoreGetMasterRequest request)
        {
            Console.WriteLine($"GetMaster request -> PartitionId: {request.PartitionId}");
            string masterId;
            try
            {
                masterId = gStore.GetMaster(request.PartitionId);
            }
            catch (Exception)
            {
                masterId = "-1"; //fixme
            }

            GStoreGetMasterResponse gStoreGetMasterResponse = new GStoreGetMasterResponse
            {
                MasterId = masterId
            };

            return gStoreGetMasterResponse;

        }

        class DataObjectIdentifierBuilder
        {
            internal static DataObjectIdentifier FromString(string partitionId, string objectId)
            {
                return new DataObjectIdentifier
                {
                    PartitionId = partitionId,
                    ObjectId = objectId
                };
            }

            internal static DataObjectIdentifier FromObjectIdentifier(GStoreObjectIdentifier objectIdentifier)
            {
                return FromString(objectIdentifier.PartitionId, objectIdentifier.ObjectId);
            }
        }

        class DataObjectBuilder
        {

            internal static DataObject FromString(string partitionId, string objectId, string value)
            {
                return new DataObject
                {
                    ObjectIdentifier = DataObjectIdentifierBuilder.FromString(partitionId, objectId),
                    Value = value
                };
            }

            internal static DataObject FromObjectIdentifier(DataObjectIdentifier objectIdentifier, string value)
            {
                return new DataObject
                {
                    ObjectIdentifier = objectIdentifier,
                    Value = value
                };
            }

        }

        class DataObjectReplicaBuilder
        {
            internal static DataObjectReplica FromString(string partitionId, string objectId, string value, bool isMasterReplica)
            {
                return new DataObjectReplica
                {
                    Object = DataObjectBuilder.FromString(partitionId, objectId, value),
                    IsMasterReplica = isMasterReplica
                };
            }

            internal static DataObjectReplica FromObjectReplica(GStoreObjectReplica gStoreObjectReplica)
            {
                return FromString(gStoreObjectReplica.Object.Identifier.PartitionId, gStoreObjectReplica.Object.Identifier.ObjectId, gStoreObjectReplica.Object.Value, gStoreObjectReplica.IsMaster);
            }
        }
    }
}
