using GStoreServer.Controllers;
using GStoreServer.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Utils;

namespace GStoreServer
{
    class GStore
    {
        private readonly ConcurrentDictionary<GStoreObjectIdentifier, GStoreObject> DataStore;
        private readonly ConcurrentDictionary<GStoreObjectIdentifier, ReaderWriterLockSlim> ObjectLocks;
        private readonly ConcurrentDictionary<GStoreObjectIdentifier, int> ObjectVersionNumber;
        private readonly ConcurrentDictionary<GStoreObjectIdentifier, string> ObjectVersionServerWriter;
        private readonly ConcurrentDictionary<GStoreObjectIdentifier, GStoreObjectVersioning> ObjectVersionings;
        private readonly ConnectionManager connectionManager;

        public GStore(ConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            DataStore = new ConcurrentDictionary<GStoreObjectIdentifier, GStoreObject>();
            ObjectLocks = new ConcurrentDictionary<GStoreObjectIdentifier, ReaderWriterLockSlim>();
            ObjectVersionNumber = new ConcurrentDictionary<GStoreObjectIdentifier, int>();
            ObjectVersionServerWriter = new ConcurrentDictionary<GStoreObjectIdentifier, string>();
            ObjectVersionings = new ConcurrentDictionary<GStoreObjectIdentifier, GStoreObjectVersioning>();
        }

        public void Write(GStoreObjectIdentifier gStoreObjectIdentifier, string newValue)
        {
            // Acquire lock on local object
            ReaderWriterLockSlim objectLock = GetObjectLock(gStoreObjectIdentifier);
            objectLock.EnterWriteLock();
            try
            {
                int version = GetAndIncrementObjectVersionNumber(gStoreObjectIdentifier);
                AddOrUpdateObjectVersionServerWriter(gStoreObjectIdentifier, connectionManager.SelfServerId);
                GStoreObject gStoreObject = AddOrUpdate(gStoreObjectIdentifier, newValue);

                _ = WriteReplicaController.ExecuteAsync(connectionManager, gStoreObject, version);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                objectLock.ExitWriteLock();
            }
        }

        public void WriteReplica(GStoreObjectIdentifier gStoreObjectIdentifier, string newValue, string newServerId, int newVersion)
        {
            ReaderWriterLockSlim objectLock = GetObjectLock(gStoreObjectIdentifier);
            objectLock.EnterWriteLock();
            try
            {
                int version = GetObjectVersionNumber(gStoreObjectIdentifier);
                string serverId = GetObjectVersionServerWriter(gStoreObjectIdentifier);

                if (newVersion > version || newVersion == version && string.Compare(newServerId, serverId) < 0)
                {
                    _ = AddOrUpdate(gStoreObjectIdentifier, newValue);
                    SetObjectVersionNumber(gStoreObjectIdentifier, newVersion);
                }
            }
            finally
            {
                objectLock.ExitWriteLock();
            }
        }

        public string Read(GStoreObjectIdentifier gStoreObjectIdentifier)
        {
            ReaderWriterLockSlim objectLock = GetObjectLock(gStoreObjectIdentifier);
            objectLock.EnterReadLock();
            try
            {
                string value = null;
                if (DataStore.TryGetValue(gStoreObjectIdentifier, out GStoreObject gStoreObject))
                {
                    value = gStoreObject.Value;
                }
                return value;
            }
            finally
            {
                objectLock.ExitReadLock();
            }
                      
        }

        public ICollection<GStoreObject> ReadAll()
        {
            ISet<GStoreObject> values = new HashSet<GStoreObject>();
            foreach (KeyValuePair<GStoreObjectIdentifier, ReaderWriterLockSlim> objectPair in ObjectLocks)
            {
                GStoreObjectIdentifier gStoreObjectIdentifier = objectPair.Key;
                ReaderWriterLockSlim objectLock = objectPair.Value;

                objectLock.EnterReadLock();
                try
                {
                    DataStore.TryGetValue(gStoreObjectIdentifier, out GStoreObject gStoreObject);
                    values.Add(gStoreObject);
                }
                finally
                {
                    objectLock.ExitReadLock();
                }
            }
            return values;
        }

        public int GetObjectVersionNumber(GStoreObjectIdentifier gStoreObjectIdentifier)
        {
            int version = ObjectVersionNumber.GetOrAdd(gStoreObjectIdentifier,
                (key) =>
                {
                    return 0;
                });
            return version;
        }

        private void SetObjectVersionNumber(GStoreObjectIdentifier gStoreObjectIdentifier, int version)
        {
            _ = ObjectVersionNumber.AddOrUpdate(gStoreObjectIdentifier,
                (key) =>
                {
                    return version;
                },
                (key, value) =>
                {
                    return version;
                }
                );
        }

        private int GetAndIncrementObjectVersionNumber(GStoreObjectIdentifier gStoreObjectIdentifier)
        {
            int version = ObjectVersionNumber.AddOrUpdate(gStoreObjectIdentifier,
                (key) =>
                {
                    return 1;
                },
                (key, value) =>
                {
                    return ++value;
                }
                );
            return version;
        }


        private void AddOrUpdateObjectVersionServerWriter(GStoreObjectIdentifier gStoreObjectIdentifier, string serverId)
        {
            _ = ObjectVersionServerWriter.AddOrUpdate(gStoreObjectIdentifier,
                (key) =>
                {
                    return serverId;
                },
                (key, value) =>
                {
                    return serverId;
                });
        }

        private string GetObjectVersionServerWriter(GStoreObjectIdentifier gStoreObjectIdentifier)
        {
            ObjectVersionServerWriter.TryGetValue(gStoreObjectIdentifier, out string serverId);
            return serverId;
        }


        private GStoreObjectVersioning GetObjectVersioning(GStoreObjectIdentifier gStoreObjectIdentifier)
        {
            return ObjectVersionings.GetOrAdd(gStoreObjectIdentifier,
                (key) =>
                {
                    return new GStoreObjectVersioning();
                });
        }

        private ReaderWriterLockSlim GetObjectLock(GStoreObjectIdentifier gStoreObjectIdentifier)
        {
            return ObjectLocks.GetOrAdd(gStoreObjectIdentifier,
                (key) =>
                {
                    return new ReaderWriterLockSlim();
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

        public ConnectionManager GetConnectionManager()
        {
            return connectionManager;
        }
    }
}
