using System.Collections.Concurrent;
using System.Transactions;

namespace ResourceManagerSample
{
    public class TransactionFileResourceManager : IEnlistmentNotification
    {
        public ConcurrentQueue<string> files = new ConcurrentQueue<string>();
        public void Commit(Enlistment enlistment)
        {
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        public void Rollback(Enlistment enlistment)
        {
            while (files.Any())
            {
                if(files.TryDequeue(out var path))
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }                    
                }
            }
            
            enlistment.Done();
        }

        internal void AddFile(string path)
        {
            files.Enqueue(path);
        }
    }
}