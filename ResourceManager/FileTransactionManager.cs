using System.Runtime.CompilerServices;
using System.Transactions;
using ResourceManagerSample.ResourceManagers;

namespace ResourceManagerSample
{
    public class FileTransactionManager
    {
        private static readonly ConditionalWeakTable<Transaction, TransactionFileResourceManager> _table = new();

        public static TransactionFileResourceManager GetResourceManager()
        {
            var transaction = Transaction.Current;
            if (transaction == null)
            {
                throw new InvalidOperationException("No active transaction.");
            }

            if (!_table.TryGetValue(transaction, out var rm))
            {
                rm = new TransactionFileResourceManager();
                _table.Add(transaction, rm);
                transaction.EnlistVolatile(rm, EnlistmentOptions.None);
                transaction.TransactionCompleted += (_, __) =>
                {
                    _table.Remove(transaction);
                };
            }

            return _table.GetValue(transaction, t =>
            {
                var rm = new TransactionFileResourceManager();
                t.EnlistVolatile(rm, EnlistmentOptions.None);
                return rm;
            });
        }
    }
}