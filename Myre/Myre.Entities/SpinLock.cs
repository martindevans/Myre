using System.Threading;

namespace Myre.Entities
{
    struct SpinLock
    {
        private const int FALSE = default(int);
        private const int TRUE = FALSE + 1;     //Ensure false != true

        private int locked;                     //Locked is automatically initialised to default(int);

        public void Lock()
        {
            while (Interlocked.CompareExchange(ref locked, TRUE, FALSE) != FALSE)
                Thread.Sleep(0);
        }

        public void Unlock()
        {
            Interlocked.CompareExchange(ref locked, FALSE, TRUE);
        }
    }
}
