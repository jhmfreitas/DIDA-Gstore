using System;

namespace PuppetMaster.Domain
{
    class PCS
    {
        public PuppetMasterPCSService.PuppetMasterPCSServiceClient Stub { get; }

        public PCS(PuppetMasterPCSService.PuppetMasterPCSServiceClient client)
        {
            Stub = client ?? throw new ArgumentNullException("Client cannot be null.");
        }
    }
}
