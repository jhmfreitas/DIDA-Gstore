using Client.Domain;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace Client
{
    class PuppetmasterClientServiceImpl : PuppetMasterClientService.PuppetMasterClientServiceBase
    {
        private readonly ConnectionManager connectionManager;
        public PuppetmasterClientServiceImpl(ConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager;
        }

        public override Task<Empty> Status(Empty request, ServerCallContext context)
        {
            Console.WriteLine(connectionManager);
            return Task.FromResult(new Empty());
        }
    }
}
