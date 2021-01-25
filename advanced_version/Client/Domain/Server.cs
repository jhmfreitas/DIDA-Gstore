using Utils;

namespace Client.Domain
{
    class Server : GenericServer<GStoreService.GStoreServiceClient>
    {
        public Server(string id, GStoreService.GStoreServiceClient stub) : base(id, stub) { }
    }
}
