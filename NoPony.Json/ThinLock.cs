using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NoPony.Json
{
    // TODO: Does using this actually take any less time than using lock()?
    internal struct ThinLock
    {
        private int _lock;

        public void Enter()
        {
            // exchange 1 for whatever is in _lock
            int was = Interlocked.Exchange(ref _lock, 1);

            // if _lock WAS 0, the lock was free, and now it's ours
            // if _lock WAS 1, the lock is busy and we wait
            if (was == 1)
            {
                var w = new SpinWait();

                while (was == 1)
                {
                    w.SpinOnce();
                    was = Interlocked.Exchange(ref _lock, 1);
                }
            }
        }

        public void Exit()
        {
            int was = Interlocked.Exchange(ref _lock, 0);
        }
    }
}
