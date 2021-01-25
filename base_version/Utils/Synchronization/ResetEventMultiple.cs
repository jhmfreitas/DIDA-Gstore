using System.Threading;

namespace Utils
{
    class ResetEventMultiple
    {
        private readonly object lockObject = new object();

        private bool queuedSet = true;

        private readonly ManualResetEventSlim manual = new ManualResetEventSlim(false);
        private readonly AutoResetEvent auto = new AutoResetEvent(false);

        private int waitingOne = 0;
        private int waitingAll = 0;

        public void Set()
        {
            lock (lockObject)
            {
                if (waitingOne > 0)
                {
                    auto.Set();
                }
                else if (waitingAll > 0)
                {
                    manual.Set();
                }
                else
                {
                    queuedSet = true;
                }

            }
        }

        public void Reset()
        {
            manual.Reset();
            auto.Reset();
        }

        public void Wait()
        {
            bool wait = true;
            lock (lockObject)
            {

                if (queuedSet == true)
                {
                    manual.Set();
                    queuedSet = false;
                    wait = false;
                }
                else
                {
                    waitingAll += 1;
                }
            }
            if (wait)
            {
                manual.Wait();
                lock (lockObject)
                {
                    waitingAll -= 1;
                }
            }

        }

        public void WaitOne()
        {
            bool wait = true;
            lock (lockObject)
            {
                manual.Reset();
                if (queuedSet == true)
                {
                    queuedSet = false;
                    wait = false;
                }
                else
                {
                    waitingOne += 1;
                }
            }

            if (wait)
            {
                auto.WaitOne();
                lock (lockObject)
                {
                    waitingOne -= 1;
                }
            }
        }
    }
}
