using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GStoreServer.Controllers
{
    class HeartbeatController
    {
        public static async Task ExecuteAsync(MasterReplicaService.MasterReplicaServiceClient Stub, string selfServerId, int timeout)
        {
            await Stub.HeartBeatAsync(new HeartBeatRequest
            {
                ServerId = selfServerId
            }, deadline: DateTime.UtcNow.AddMilliseconds(timeout));
        }
    }
}
