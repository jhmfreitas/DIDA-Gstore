using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace GStoreServer.Services
{
    class MasterReplicaServiceImpl : MasterReplicaService.MasterReplicaServiceBase
    {

        private GStore gStore;

        public MasterReplicaServiceImpl(GStore gStore)
        {
            this.gStore = gStore ?? throw new ArgumentNullException("gstore cannot be null");
        }

        public override Task<Empty> Write(WriteRequest request, ServerCallContext context)
        {
            return Task.FromResult(ExecuteWrite(request));
        }

        private Empty ExecuteWrite(WriteRequest request)
        {
            Console.WriteLine($"Write Replica request -> PartitionId: {request.Object.ObjectIdentifier.PartitionId} ObjectId: {request.Object.ObjectIdentifier.ObjectId} Value: {request.Object.Value} ServerId: {request.ServerId} Version: {request.Version}");
            GStoreObjectIdentifier gStoreObjectIdentifier = new GStoreObjectIdentifier(request.Object.ObjectIdentifier.PartitionId, request.Object.ObjectIdentifier.ObjectId);
            gStore.WriteReplica(gStoreObjectIdentifier, request.Object.Value, request.ServerId, request.Version);
            return new Empty();
        }

        public override Task<Empty> HeartBeat(HeartBeatRequest request, ServerCallContext context)
        {
            return Task.FromResult(ExecuteHeartbeat(request));
        }

        private Empty ExecuteHeartbeat(HeartBeatRequest request)
        {
            try
            {
                string replicaId = request.ServerId;
                gStore.GetConnectionManager().ResetTimer(replicaId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return new Empty();
        }

        public override Task<Empty> Crash(Empty request, ServerCallContext context)
        {
            return Task.FromResult(ExecuteCrash(request));
        }

        private Empty ExecuteCrash(Empty request)
        {
            Console.WriteLine($"Crash Replica request");
            return new Empty();
        }
    }
}
