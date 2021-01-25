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
            return Task.FromResult(ExecuteWrite(request));
        }

        private Empty ExecuteWrite(GStoreWriteRequest request)
        {
            Console.WriteLine($"Write request -> PartitionId: {request.Object.ObjectIdentifier.PartitionId} ObjectId: {request.Object.ObjectIdentifier.ObjectId} Value: {request.Object.Value}");

            GStoreObjectIdentifier gStoreObjectIdentifier = new GStoreObjectIdentifier(request.Object.ObjectIdentifier.PartitionId, request.Object.ObjectIdentifier.ObjectId);
            gStore.Write(gStoreObjectIdentifier, request.Object.Value);

            return new Empty();
        }

        public override Task<GStoreListServerReply> ListServer(Empty request, ServerCallContext context)
        {
            return Task.FromResult(ExecuteListServer());
        }

        private GStoreListServerReply ExecuteListServer()
        {
            Console.WriteLine($"ListServer request");

            ICollection<GStoreObject> gStoreObjects = gStore.ReadAll();

            GStoreListServerReply reply = new GStoreListServerReply();
            foreach (GStoreObject gStoreObject in gStoreObjects)
            {
                reply.Objects.Add(DataObjectBuilder.FromString(gStoreObject.Identifier.PartitionId, gStoreObject.Identifier.ObjectId, gStoreObject.Value));
            }
            return reply;
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
    }
}
