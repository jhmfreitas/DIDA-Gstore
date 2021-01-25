using Utils;

namespace GStoreServer.Domain
{
    class Server : GenericServer<MasterReplicaService.MasterReplicaServiceClient>
    {
        public Server(string id, MasterReplicaService.MasterReplicaServiceClient stub) : base(id, stub) { }
    }
}
