using System.Collections.Concurrent;
using System.Transactions;

namespace ResourceManagerSample.ResourceManagers
{
    public class TransactionEmailResourceManager : IEnlistmentNotification
    {
        public ConcurrentQueue<Guid> ids = new ConcurrentQueue<Guid>();
        public void Commit(Enlistment enlistment)
        {
            using var email = new EmailApiClient();
            NextEnlistment(enlistment, email.ConfirmAsync);
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
            using var email = new EmailApiClient();
            NextEnlistment(enlistment, email.CancelAsync);
        }

        public void NextEnlistment(Enlistment enlistment, Func<Guid, Task> operation)
        {
            var tasks = new List<Task>();

            while (ids.Any())
            {
                if (ids.TryDequeue(out var id))
                {
                    tasks.Add(operation(id));
                }
            }

            Task.WaitAll(tasks.ToArray());

            enlistment.Done();
        }

        internal void AddEmailId(Guid id)
        {
            ids.Enqueue(id);
        }
    }
}