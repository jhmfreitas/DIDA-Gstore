using System;

namespace PuppetMaster.Domain
{
    class Client
    {
        public string Username { get; }

        public PuppetMasterClientService.PuppetMasterClientServiceClient Stub { get; }

        public Client(string username, PuppetMasterClientService.PuppetMasterClientServiceClient client)
        {
            Username = username ?? throw new ArgumentNullException("Client Username cannot be null.");
            Stub = client ?? throw new ArgumentNullException("Client cannot be null.");
        }
    }
}
