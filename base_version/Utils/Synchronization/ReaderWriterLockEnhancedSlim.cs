using System;
using System.Collections.Generic;

namespace Utils
{
    public class ReaderWriterLockEnhancedSlim
    {
        private readonly object LockObject = new object();

        private bool writing = false;
        private int nCurrentReaders = 0;
        private int nWaitingWriters = 0;
        private readonly ResetEventMultiple resetEventMultiple = new ResetEventMultiple();

        private readonly HashSet<int> readLocks = new HashSet<int>();
        private int writeLock = 0;
        private readonly Random random = new Random();


        public int EnterReadLock()
        {
            int id;
            lock (LockObject)
            {
                do
                {
                    id = random.Next(int.MinValue, int.MaxValue);
                } while (readLocks.Contains(id));

                if (writing == false && nWaitingWriters == 0)
                {
                    resetEventMultiple.Set();
                }
            }

            resetEventMultiple.Wait();

            lock (LockObject)
            {
                nCurrentReaders += 1;
                readLocks.Add(id);
                writeLock = 0;
                if (writing == true)
                {
                    Console.WriteLine("=================================================================================");
                    Console.WriteLine("============================= SYNCHRONIZATION ERROR =============================");
                    Console.WriteLine("============================= SYNCHRONIZATION ERROR =============================");
                    Console.WriteLine("============================= SYNCHRONIZATION ERROR =============================");
                    Console.WriteLine("============================= SYNCHRONIZATION ERROR =============================");
                    Console.WriteLine("=================================================================================");
                    throw new LockSynchronizationException("Enter read lock while writing.");
                }

            }

            return id;
        }

        public int EnterWriteLock()
        {
            int id;

            lock (LockObject)
            {
                id = random.Next(int.MinValue, int.MaxValue);
                if (nCurrentReaders > 0) resetEventMultiple.Reset();
                nWaitingWriters += 1;
            }
            resetEventMultiple.WaitOne();

            lock (LockObject)
            {
                writing = true;
                writeLock = id;
                nWaitingWriters -= 1;

                if (readLocks.Count != 0)
                {
                    Console.WriteLine("=================================================================================");
                    Console.WriteLine("============================= SYNCHRONIZATION ERROR =============================");
                    Console.WriteLine("============================= SYNCHRONIZATION ERROR =============================");
                    Console.WriteLine("============================= SYNCHRONIZATION ERROR =============================");
                    Console.WriteLine("============================= SYNCHRONIZATION ERROR =============================");
                    Console.WriteLine("=================================================================================");
                    throw new LockSynchronizationException("Enter write lock while reading.");
                }
            }

            return id;      
        }

        public void ExitReadLock(int id)
        {
            lock (LockObject)
            {
                if (nCurrentReaders == 0)
                {
                    throw new InvalidLockExitException("No current readers.");
                }
                if (!readLocks.Contains(id))
                {
                    throw new InvalidLockExitException("Invalid reader lock id.");
                }
                nCurrentReaders -= 1;
                if (nCurrentReaders == 0)
                {
                    resetEventMultiple.Set();
                }

                readLocks.Remove(id);
            }
        }

        public void ExitWriteLock(int id)
        {
            lock (LockObject)
            {
                if (writing == false)
                {
                    throw new InvalidLockExitException("No current writer.");
                }
                if (!(writeLock == id))
                {
                    throw new InvalidLockExitException("Invalid writer lock id.");
                }
                writing = false;
                resetEventMultiple.Set();
            }
        }

        public bool IsWriteLockValid(int id)
        {
            lock (LockObject)
            {
                return writing && writeLock == id;
            }
        }

        public bool IsWriteLocked()
        {
            lock (LockObject)
            {
                return writing;
            }
        }
    }

    [Serializable]
    public class InvalidLockExitException : Exception
    {
        public InvalidLockExitException()
        { }

        public InvalidLockExitException(string message)
            : base(message)
        { }

        public InvalidLockExitException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    [Serializable]
    public class LockSynchronizationException : Exception
    {
        public LockSynchronizationException()
        { }

        public LockSynchronizationException(string message)
            : base(message)
        { }

        public LockSynchronizationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
