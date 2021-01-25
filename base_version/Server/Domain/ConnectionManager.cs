using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Utils;
using System.Timers;
using System.Collections.Immutable;
using GStoreServer.Controllers;

namespace GStoreServer.Domain
{
    class ConnectionManager : GenericConnectionManager<Server, MasterReplicaService.MasterReplicaServiceClient>
    {
        private readonly string selfServerId;

        // Partitions in which this server is a Master
        private readonly ISet<string> masterPartitions;

        // Partitions in which this server is a Replica
        private readonly ISet<string> replicaPartitions;

        private readonly IDictionary<string, Timer> replicasWatchdogs = new Dictionary<string, Timer>();

        // Delay between each heartbeat round
        private static readonly int HEARTBEAT_INTERVAL = 1000;

        // Time for a heartbeat to timeout. After this time the request will be canceled.
        private static readonly int HEARTBEAT_TIMEOUT = 10000;

        // Time for watchdog to timeout. After this replica replica will be considered dead. This value should be higher than HEARTBEAT_INTERVAL
        private static readonly int WATCHDOG_TIMEOUT = HEARTBEAT_INTERVAL + HEARTBEAT_TIMEOUT;

        // Delay between initialization and when heartbeats start being sent
        private static readonly int GRACE_PERIOD = 2000;

        private bool useWatchDog = true;
        // Used to clean locks on crashes
        private GStore gStore;

        public ConnectionManager(IDictionary<string, Server> servers, IDictionary<string, Partition> partitions, string selfServerId) : base(servers, partitions)
        {
            if (string.IsNullOrWhiteSpace(selfServerId))
            {
                throw new ArgumentException($"'{selfServerId}' cannot be null or whitespace", nameof(selfServerId));
            }
            this.selfServerId = selfServerId;

            masterPartitions = new HashSet<string>();
            replicaPartitions = new HashSet<string>();
            foreach (Partition partition in partitions.Values)
            {
                if (partition.MasterId == selfServerId) masterPartitions.Add(partition.Id);
                else if (partition.ContainsReplica(selfServerId)) replicaPartitions.Add(partition.Id);
            }

            InitWatchdogs();
            InitHeartbeats();
        }

        public void AddGStore(GStore gStore)
        {
            this.gStore = gStore;
        }


        private ISet<Server> GetReplicasOfPartitionsWhereSelfMaster()
        {
            ISet<Server> replicas = new HashSet<Server>();
            foreach (string partitionId in masterPartitions)
            {
                replicas.UnionWith(GetPartitionAliveReplicas(partitionId));
            }
            return replicas;
        }


        // Returns the set of master servers for the partitions in which the current server corresponding to this instance of the ConnectionManager is a replica
        private ISet<Server> GetMastersOfPartitionsWhereSelfReplica()
        {
            ISet<Server> masters = new HashSet<Server>();
            foreach (string partitionId in replicaPartitions)
            {
                Partition partition = GetPartition(partitionId);
                Server master = GetServer(partition.MasterId);
                masters.Add(master);
            }
            return masters;
        }

        public bool IsMasterForPartition(string partitionId)
        {
            lock(this)
            {
                return masterPartitions.Contains(partitionId);
            }
        }

        public new void DeclareDead(string deadServerId)
        {
            lock(this)
            {
                if (deadServerId == selfServerId)
                {
                    throw new Exception("Self Declared Dead");
                }

                base.DeclareDead(deadServerId);

                foreach (Partition partition in Partitions.Values)
                {
                    if (partition.MasterId == deadServerId)
                    {
                        List<string> sortedServerIds = partition.GetSortedServers();
                        int newMasterIndex = sortedServerIds.IndexOf(deadServerId);
                        string newMasterId;
                        do
                        {
                            newMasterIndex = (newMasterIndex + 1) % sortedServerIds.Count;
                            newMasterId = sortedServerIds.ElementAt(newMasterIndex);
                        } while (newMasterId != selfServerId && !GetServer(newMasterId).Alive);

                        ElectNewMaster(partition.Id, newMasterId);
                    }
                }
            }
            if (gStore != null) _ = gStore.CleanLocks(deadServerId);
        }


        // Caller should ALWAYS ensure mutual exclusion within this function
        private new void ElectNewMaster(string partitionId, string newMasterId)
        {
            // vvv Redundant vvv
            Partition partition = GetPartition(partitionId);
            if (partition.MasterId == selfServerId)
            {
                masterPartitions.Remove(partitionId);
                replicaPartitions.Add(partitionId);
            }
            // ^^^ Redudant ^^^

            base.ElectNewMaster(partitionId, newMasterId);


            if (newMasterId == selfServerId)
            {
                masterPartitions.Add(partitionId);
                replicaPartitions.Remove(partitionId);

                IImmutableSet<Server> replicas = GetPartitionAliveReplicas(partitionId);
                foreach (Server replica in replicas)
                {
                    string replicaId = replica.Id;
                    // In case it is a new replica for this server
                    if (!replicasWatchdogs.ContainsKey(replicaId))
                    {
                        AddReplicaToWatchdog(replicaId);
                    }
                }
            }

        }

        public override string ToString()
        {
            string toString = base.ToString();

            toString += "\nServerId: " + selfServerId;
            toString += "\nPartitions where (self) master:";
            foreach(string partition in masterPartitions)
            {
                toString += " " + partition;
            }

            toString += "\nPartitions where (self) replica:";
            foreach (string partition in replicaPartitions)
            {
                toString += " " + partition;
            }

            toString += "\n=========================\n\n";

            return toString;
        }

        public async void InitHeartbeats()
        {

            await Task.Delay(GRACE_PERIOD);
            
            while (true)
            {
                _ = SendHeartbeats();
                await Task.Delay(HEARTBEAT_INTERVAL);
            }
        }

        // Heart beats are sent from replica to masters.
        private async Task SendHeartbeats()
        {
            IDictionary<string, Task> heartbeatTasks = new Dictionary<string, Task>();

            foreach (Server masterServer in GetMastersOfPartitionsWhereSelfReplica())
            {
                heartbeatTasks.Add(masterServer.Id, HeartbeatController.ExecuteAsync(masterServer.Stub, selfServerId, HEARTBEAT_TIMEOUT));
            }

            foreach (KeyValuePair<string, Task> heartbeatTask in heartbeatTasks)
            {
                string masterId = heartbeatTask.Key;
                Task task = heartbeatTask.Value;
                try
                {
                    await task;
                }
                catch (Grpc.Core.RpcException exception) when (exception.StatusCode == Grpc.Core.StatusCode.DeadlineExceeded || exception.StatusCode == Grpc.Core.StatusCode.Internal)
                {
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss tt} No response from master '{masterId}' heartbeat in '{HEARTBEAT_TIMEOUT}' milliseconds.");
                    DeclareDead(masterId);
                }
            }
        }

        private async void InitWatchdogs()
        {
            await Task.Delay(GRACE_PERIOD);

            foreach (string partitionId in masterPartitions)
            {
                foreach(Server replica in GetReplicasOfPartitionsWhereSelfMaster())
                {
                    string replicaId = replica.Id;
                    AddReplicaToWatchdog(replicaId);
                }
            }
        }

        private void AddReplicaToWatchdog(string replicaId)
        {
            if (!useWatchDog) return;
            Console.WriteLine($"{DateTime.Now:HH:mm:ss tt} Added replica '{replicaId}' to watchdog set");
            replicasWatchdogs.Add(replicaId, SetTimer(WATCHDOG_TIMEOUT, replicaId));
        }

        private Timer SetTimer(int timerInterval, string replicaId)
        {
            // Create a timer with a two second interval.
            Timer timer = new Timer(timerInterval);
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += delegate { RemoveReplica(replicaId); };
            timer.Enabled = true;
            return timer;
        }

        private void RemoveReplica(string replicaId)
        {
            lock(this)
            {
                if (!replicasWatchdogs.TryGetValue(replicaId, out Timer timer))
                {
                    return;
                }
                Console.WriteLine($"{DateTime.Now:HH:mm:ss tt} Watchdog timeout from replica '{replicaId}': {WATCHDOG_TIMEOUT} milliseconds ellapsed without heartbeat.");

                //Stops and releases resources used by the timer
                timer.Stop();
                timer.Dispose();

                //Removes timer from watch dog list
                replicasWatchdogs.Remove(replicaId);
                DeclareDead(replicaId);
            }
        }

        public void ResetTimer(string replicaId)
        {
            if (!useWatchDog) return;

            if (!replicasWatchdogs.TryGetValue(replicaId, out Timer timer)) {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss tt} ResetTimer of nonexisted server - Self: {selfServerId} | Reset: {replicaId} | Replicas: ");
                return;
            }
            ResetTimer(timer);
        }
        private void ResetTimer(Timer timer)
        {
            timer.Stop();
            timer.Start();
        }
    }
}
