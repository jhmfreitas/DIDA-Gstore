using GStoreServer.Controllers;
using GStoreServer.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils;

namespace GStoreServer
{
    class GStore
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, GStoreObjectIdentifier>> MastersLocks;
        private readonly ConcurrentDictionary<GStoreObjectIdentifier, GStoreObject> DataStore;
        private readonly ConcurrentDictionary<GStoreObjectIdentifier, ReaderWriterLockEnhancedSlim> ObjectLocks;
        private readonly ConnectionManager connectionManager;

        public GStore(ConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            MastersLocks = new ConcurrentDictionary<string, ConcurrentDictionary<int, GStoreObjectIdentifier>>();
            DataStore = new ConcurrentDictionary<GStoreObjectIdentifier, GStoreObject>();
            ObjectLocks = new ConcurrentDictionary<GStoreObjectIdentifier, ReaderWriterLockEnhancedSlim>();
        }

        public async Task Write(GStoreObjectIdentifier gStoreObjectIdentifier, string newValue)
        {
            try
            {
                if (!connectionManager.IsMasterForPartition(gStoreObjectIdentifier.PartitionId))
                {
                    throw new Exception("Not master");
                }

                // Acquire lock on local object
                ReaderWriterLockEnhancedSlim objectLock = GetObjectLock(gStoreObjectIdentifier);
                int lockId = objectLock.EnterWriteLock();

                // Send lock requests to all remote objects
                IDictionary<string, int> replicaLocks = await LockController.ExecuteAsync(connectionManager, gStoreObjectIdentifier);

                // Once lock confirmations arrive, write to local object and unlock it
                GStoreObject gStoreObject = AddOrUpdate(gStoreObjectIdentifier, newValue);
                objectLock.ExitWriteLock(lockId);

                // Send write requests to all remote objects
                await WriteReplicaController.ExecuteAsync(connectionManager, gStoreObject, replicaLocks);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }

        public string Read(GStoreObjectIdentifier gStoreObjectIdentifier)
        {
            ReaderWriterLockEnhancedSlim objectLock = GetObjectLock(gStoreObjectIdentifier);
            string value = null;
            int id = objectLock.EnterReadLock();
            if (DataStore.TryGetValue(gStoreObjectIdentifier, out GStoreObject gStoreObject))
            {
                value = gStoreObject.Value;
            }
            objectLock.ExitReadLock(id);
            return value;           
        }

        public async Task<ICollection<GStoreObjectReplica>> ReadAll()
        {

            ICollection<ReaderWriterLockEnhancedSlim> locks = ObjectLocks.Values;

            IDictionary<ReaderWriterLockEnhancedSlim, Task<int>> lockTasks = new Dictionary<ReaderWriterLockEnhancedSlim, Task<int>>();
            foreach (ReaderWriterLockEnhancedSlim objectLock in locks)
            {
                lockTasks.Add(objectLock, Task.Run(objectLock.EnterReadLock));
            }

            IDictionary<ReaderWriterLockEnhancedSlim, int> lockWithReadIdSet = new Dictionary<ReaderWriterLockEnhancedSlim, int>();

            foreach(KeyValuePair<ReaderWriterLockEnhancedSlim, Task<int>> lockTask in lockTasks)
            {
                ReaderWriterLockEnhancedSlim objectLock = lockTask.Key;
                Task<int> task = lockTask.Value;
                lockWithReadIdSet.Add(objectLock, await task);
            }

            ISet<GStoreObjectReplica> values = new HashSet<GStoreObjectReplica>();

            foreach (GStoreObject gStoreObject in DataStore.Values)
            {
                // should lock partitions during this operation
                string partitionId = gStoreObject.Identifier.PartitionId;
                GStoreObjectReplica gStoreObjectReplica = new GStoreObjectReplica(gStoreObject, connectionManager.IsMasterForPartition(partitionId));
                values.Add(gStoreObjectReplica);
            }

            foreach(KeyValuePair<ReaderWriterLockEnhancedSlim, int> lockWithReadId in lockWithReadIdSet)
            {
                ReaderWriterLockEnhancedSlim objectLock = lockWithReadId.Key;
                int lockId = lockWithReadId.Value;
                objectLock.ExitReadLock(lockId);
            }

            return values;
        }

        public int Lock(GStoreObjectIdentifier gStoreObjectIdentifier)
        {
            ReaderWriterLockEnhancedSlim objectLock = GetObjectLock(gStoreObjectIdentifier);
            string masterId = connectionManager.GetPartitionMasterId(gStoreObjectIdentifier.PartitionId);
            int lockId = objectLock.EnterWriteLock();
            ConcurrentDictionary<int, GStoreObjectIdentifier> locks = MastersLocks.GetOrAdd(masterId, (masterId) => new ConcurrentDictionary<int, GStoreObjectIdentifier>());
            locks.TryAdd(lockId, gStoreObjectIdentifier);
            return lockId;
        }

        public void WriteReplica(GStoreObjectIdentifier gStoreObjectIdentifier, string newValue, int lockId)
        {
            ReaderWriterLockEnhancedSlim objectLock = GetObjectLock(gStoreObjectIdentifier);

            if (!objectLock.IsWriteLockValid(lockId))
            {
                // throw error
            }

            AddOrUpdate(gStoreObjectIdentifier, newValue);

            objectLock.ExitWriteLock(lockId);

            string masterId = connectionManager.GetPartitionMasterId(gStoreObjectIdentifier.PartitionId);
            MastersLocks.TryGetValue(masterId, out ConcurrentDictionary<int, GStoreObjectIdentifier> locks);
            locks.TryRemove(lockId, out _);
        }

        public string GetMaster(string partitionId)
        {
            return connectionManager.GetPartitionMasterId(partitionId);
        }

        public bool IsLocked(GStoreObjectIdentifier gStoreObjectIdentifier)
        {
            ReaderWriterLockEnhancedSlim objectLock = GetObjectLock(gStoreObjectIdentifier);
            return objectLock.IsWriteLocked();
        }

        private ReaderWriterLockEnhancedSlim GetObjectLock(GStoreObjectIdentifier gStoreObjectIdentifier)
        {
            return ObjectLocks.GetOrAdd(gStoreObjectIdentifier,
                (key) =>
                {
                    return new ReaderWriterLockEnhancedSlim();
                });
        }

        private GStoreObject AddOrUpdate(GStoreObjectIdentifier gStoreObjectIdentifier, string newValue)
        {
            return DataStore.AddOrUpdate(gStoreObjectIdentifier,
                (id) =>
                {
                    return new GStoreObject(id, newValue);
                },
                (id, gStoreObject) =>
                {
                    gStoreObject.Value = newValue;
                    return gStoreObject;
                });
        }

        public async Task CleanLocks(string masterId)
        {
            MastersLocks.TryGetValue(masterId, out ConcurrentDictionary<int, GStoreObjectIdentifier> masterLocks);

            if (masterLocks == null) return;

            IDictionary<int, Task<string>> tasks = new Dictionary<int, Task<string>>();
            foreach (KeyValuePair<int, GStoreObjectIdentifier> locks in masterLocks)
            {
                int lockId = locks.Key;
                GStoreObjectIdentifier gStoreObjectIdentifier = locks.Value;

                ReaderWriterLockEnhancedSlim objectLock = GetObjectLock(gStoreObjectIdentifier);
                if (!objectLock.IsWriteLockValid(lockId)) {
                    throw new Exception("Invalid lock");
                }

                Task<string> task = ReadRecoveryController.ExecuteAsync(connectionManager, gStoreObjectIdentifier);
                tasks.Add(lockId, task);
            }

            foreach (KeyValuePair<int, Task<string>> taskPair in tasks)
            {
                Task<string> task = taskPair.Value;
                int lockId = taskPair.Key;

                masterLocks.TryGetValue(lockId, out GStoreObjectIdentifier gStoreObjectIdentifier);
                ReaderWriterLockEnhancedSlim objectLock = GetObjectLock(gStoreObjectIdentifier);

                string value = await task;
                if (value != null)
                {
                    _ = AddOrUpdate(gStoreObjectIdentifier, value);
                }

                Console.WriteLine("Cleaned one lock");
                objectLock.ExitWriteLock(lockId);
            }

        }

        public ConnectionManager GetConnectionManager()
        {
            return connectionManager;
        }
    }
}
