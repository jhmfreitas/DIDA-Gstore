using Utils;
using System.Collections.Generic;
using System;
using Client.Controllers;
using System.Threading.Tasks;

namespace Client.Domain
{
    class ConnectionManager : GenericConnectionManager<Server, GStoreService.GStoreServiceClient>
    {

        private Server attachedServer;

        public ConnectionManager(IDictionary<string, Server> servers, IDictionary<string, Partition> partitions) : base(servers, partitions)
        {
        }


        public Server ChooseServerForRead(string partitionId, string serverId)
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


        // Throws exception if partitionId is not valid or if master server is dead
        public Server ChooseServerForWrite(string partitionId)
        {
            Partition partition = GetPartition(partitionId);
            attachedServer = GetAliveServer(partition.MasterId);
            return attachedServer;
        }

        public new async Task DeclareDead(string deadServerId)
        {
            IDictionary<string, Task<string>> PartitionGetMasterTasks = new Dictionary<string, Task<string>>();

            lock (this)
            {
                base.DeclareDead(deadServerId);

                foreach (Partition partition in Partitions.Values)
                {
                    if (partition.MasterId == deadServerId)
                    {
                        PartitionGetMasterTasks.Add(partition.Id, GetMasterController.Execute(this, partition.Id));
                    }
                }
            }

            IDictionary<string, string> partitionNewMasters = new Dictionary<string, string>();
            foreach (KeyValuePair<string, Task<string>> partitionGetMasterTask in PartitionGetMasterTasks)
            {
                string partitionId = partitionGetMasterTask.Key;
                Task<string> getMasterTask = partitionGetMasterTask.Value;
                partitionNewMasters.Add(partitionId, await getMasterTask);
            }

            lock (this)
            {
                foreach (KeyValuePair<string, string> partitionNewMaster in partitionNewMasters)
                {
                    ElectNewMaster(partitionNewMaster.Key, partitionNewMaster.Value);
                }
            }
        }

        public override string ToString()
        {
            return base.ToString() + "=========================\n\n";
        }
    }
}
