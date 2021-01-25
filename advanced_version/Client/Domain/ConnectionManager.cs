using Utils;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Client.Domain
{
    class ConnectionManager : GenericConnectionManager<Server, GStoreService.GStoreServiceClient>
    {

        private Server attachedServer;

        public ConnectionManager(IDictionary<string, Server> servers, IDictionary<string, Partition> partitions) : base(servers, partitions)
        {
        }

        public void Attach(Server attachedServer)
        {
            this.attachedServer = attachedServer;
        }

        public Server ChooseServer(string partitionId, string serverId)
        {
            _ = GetPartition(partitionId); // throws exception if partition doesn't exist

            if (attachedServer != null && PartitionContainsAlive(partitionId, attachedServer.Id))
            {
                return attachedServer;
            }

            Server defaultServer = null;
            if (serverId != "-1")
            {
                defaultServer = GetServer(serverId);
            }

            if (defaultServer != null && PartitionContainsAlive(partitionId, defaultServer.Id))
            {
                attachedServer = defaultServer;
                return attachedServer;
            }

            throw new ServerBindException($"No valid attached or default server. Partition: {partitionId} | AttachedServer: ({attachedServer}) | DefaultServer: {serverId}");
            // Choose a valid server if needed
        }

        public IImmutableSet<Server> GetAliveServers(string partitionId)
        {
            Partition partition = GetPartition(partitionId);
            ImmutableList<string> servers = partition.GetAllServers();

            ISet<Server> aliveServers = new HashSet<Server>();
            foreach (string serverId in servers)
            {
                Server server = GetServer(serverId);
                if (server.Alive) aliveServers.Add(server);
            }

            return aliveServers.ToImmutableHashSet();
        }

        public override string ToString()
        {
            return base.ToString() + "=========================\n\n";
        }
    }
}
