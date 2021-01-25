using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Utils.ServerBindException;

namespace Utils
{
    public abstract class GenericConnectionManager<TServer, T> where TServer : GenericServer<T>
    {
        private IDictionary<string, TServer> Servers { get; }

        protected IDictionary<string, Partition> Partitions { get; }

        protected GenericConnectionManager(IDictionary<string, TServer> servers, IDictionary<string, Partition> partitions)
        {
            Servers = servers;
            Partitions = partitions;
        }

        public bool PartitionContainsAlive(string partitionId, string serverId)
        {
            Partition partition = GetPartition(partitionId);
            TServer server = GetServer(serverId);

            return partition.Contains(serverId) && server.Alive;
        }

        // Get replicas that are alive for a partition
        public IImmutableSet<TServer> GetPartitionAliveReplicas(string partitionId)
        {
            Partition partition = GetPartition(partitionId);
            ISet<TServer> replicaSet = new HashSet<TServer>();
            ImmutableList<string> replicaIdList = partition.GetAllReplicas();

            foreach (string serverId in replicaIdList)
            {
                TServer replica = GetServer(serverId);
                if (replica.Alive) replicaSet.Add(replica);
            }
            return replicaSet.ToImmutableHashSet();
        }

        public string GetPartitionMasterId(string partitionId)
        {
            Partition partition = GetPartition(partitionId);
            return partition.MasterId;
        }

        protected Partition GetPartition(string partitionId)
        {
            if (string.IsNullOrWhiteSpace(partitionId))
            {
                throw new ArgumentException($"'{partitionId}' cannot be null or whitespace", nameof(partitionId));
            }

            Partitions.TryGetValue(partitionId, out Partition partition);
            if (partition == null)
            {
                throw new ArgumentException($"Partition '{partitionId}' not found.", nameof(partitionId));
            }
            return partition;
        }

        // Only returns server if alive
        public TServer GetAliveServer(string serverId)
        {
            TServer server = GetServer(serverId);
            if (!server.Alive)
            {
                throw new ServerBindException($"Server '{serverId}' is dead.", ServerBindExceptionStatus.SERVER_DEAD);
            }
            return server;
        }

        // Returns a server that might be dead
        protected TServer GetServer(string serverId)
        {
            if (string.IsNullOrWhiteSpace(serverId))
            {
                throw new ArgumentException($"'{serverId}' cannot be null or whitespace", nameof(serverId));
            }

            Servers.TryGetValue(serverId, out TServer server);
            if (server == null)
            {
                throw new ArgumentException($"Server '{serverId}' not found.", nameof(serverId));
            }

            return server;
        }

        // Returns all servers that are alive
        public IImmutableSet<TServer> GetAliveServers()
        {
            ISet<TServer> aliveServers = new HashSet<TServer>();

            foreach (TServer server in Servers.Values)
            {
                if (server.Alive) aliveServers.Add(server);
            }

            return aliveServers.ToImmutableHashSet();
        }

        public IImmutableSet<TServer> GetAliveServers(string partitionId)
        {
            Partition partition = GetPartition(partitionId);
            ImmutableList<string> servers = partition.GetAllServers();

            ISet<TServer> aliveServers = new HashSet<TServer>();
            foreach (string serverId in servers)
            {
                TServer server = GetServer(serverId);
                if (server.Alive) aliveServers.Add(server);
            }

            return aliveServers.ToImmutableHashSet();
        }

        public void DeclareDead(string serverId)
        {
            TServer server = GetServer(serverId);
            lock (server)
            {
                server.DeclareDead();
            }
        }

        protected void ElectNewMaster(string partitionId, string newMasterId)
        {
            Partition partition = GetPartition(partitionId);
            lock (partition)
            {
                TServer oldMaster = GetServer(partition.MasterId);
                if (oldMaster.Alive) throw new Exception($"Master '{oldMaster.Id}' is alive and cannot be replaced.");
                partition.ElectNewMaster(newMasterId);
            }
        }

        public override string ToString()
        {
            string lines = "\n=== ConnectionManager ===\nServers:\n";

            foreach (KeyValuePair<string, TServer> serverEntry in Servers)
            {
                lines += $"  {serverEntry.Value}\n";
            }

            lines += "\nPartitions:\n";
            foreach (KeyValuePair<string, Partition> partitionEntry in Partitions)
            {
                lines += $"  {partitionEntry.Value}\n";
            }
            return lines;
        }
    }

    [Serializable]
    public class ServerBindException : Exception
    {

        public enum ServerBindExceptionStatus
        {
            DEFAULT,
            SERVER_DEAD
        }

        public ServerBindExceptionStatus Status { get; }

        public ServerBindException() => Status = ServerBindExceptionStatus.DEFAULT;

        public ServerBindException(string message, ServerBindExceptionStatus status = ServerBindExceptionStatus.DEFAULT)
            : base(message) => Status = status;

        public ServerBindException(string message, Exception innerException, ServerBindExceptionStatus status = ServerBindExceptionStatus.DEFAULT)
            : base(message, innerException) => Status = status;


    }
}

