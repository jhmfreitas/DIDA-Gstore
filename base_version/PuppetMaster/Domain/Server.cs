using System;

namespace PuppetMaster.Domain
{
    class Server
    {
        public string Id { get; }

        public PuppetMasterServerService.PuppetMasterServerServiceClient Stub { get; }

        public Server(string id, PuppetMasterServerService.PuppetMasterServerServiceClient client)
        {
            Id = id ?? throw new ArgumentNullException("Server Id cannot be null.");
            Stub = client ?? throw new ArgumentNullException("Client cannot be null.");
        }
    }
}
