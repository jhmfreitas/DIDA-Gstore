using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Utils
{
    public class Partition : IEquatable<Partition>
    {
        public string Id { get; }

        public string MasterId { get; set; }

        private SortedSet<string> ReplicaSet { get; }

        public Partition(string id, string masterId, ISet<string> replicaSet)
        {

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"'{id}' cannot be null or whitespace", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(masterId))
            {
                throw new ArgumentException($"'{masterId}' cannot be null or whitespace", nameof(masterId));
            }

            if (replicaSet == null || replicaSet.Count == 0)
            {
                throw new ArgumentException($"{nameof(replicaSet)} cannot be null or empty", nameof(replicaSet));
            }

            Id = id;
            MasterId = masterId;
            ReplicaSet = new SortedSet<string>(replicaSet);
        }

        public List<string> GetSortedServers()
        {
            SortedSet<string> serverSet = new SortedSet<string>(ReplicaSet)
            {
                MasterId
            };

            return serverSet.ToList();
        }

        public ImmutableList<string> GetAllReplicas()
        {
            return ReplicaSet.ToImmutableList();
        }

        public ImmutableList<string> GetAllServers()
        {
            ISet<string> serverSet = new HashSet<string>(ReplicaSet)
            {
                MasterId
            };
            return serverSet.ToImmutableList();
        }


        public bool ContainsReplica(string serverId)
        {
            if (string.IsNullOrEmpty(serverId)) return false;
            return ReplicaSet.Contains(serverId);
        }

        protected internal bool Contains(string serverId)
        {
            if (string.IsNullOrEmpty(serverId)) return false;
            return (MasterId == serverId) || ContainsReplica(serverId);
        }

        public void ElectNewMaster(string serverId)
        {
            if (string.IsNullOrWhiteSpace(serverId))
            {
                throw new ArgumentException($"'{serverId}' cannot be null or whitespace", nameof(serverId));
            }

            if (!ReplicaSet.Contains(serverId))
            {
                throw new ArgumentException($"'{serverId}' is not a replica of the partition '{Id}'", nameof(serverId));
            }

            ReplicaSet.Add(MasterId);
            ReplicaSet.Remove(serverId);
            MasterId = serverId;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Partition);
        }

        public bool Equals(Partition other)
        {
            return other != null &&
                   Id == other.Id;
        }

        public bool Equals(string otherId)
        {
            return otherId != null &&
                   Id == otherId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public override string ToString()
        {
            string partition = $"Partition: {Id}, Master: {MasterId}, Replicas: ";

            foreach(string replicaId in ReplicaSet)
            {
                partition += $"{replicaId} ";
            }

            return partition;
        }
    }
}
