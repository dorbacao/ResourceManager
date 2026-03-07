using System.Runtime.CompilerServices;
using System.Transactions;
using ResourceManagerSample.ResourceManagers;

namespace ResourceManagerSample
{
    public class EmailTransactionManager
    {
        private static readonly ConditionalWeakTable<Transaction, TransactionEmailResourceManager> _table = new();

        public static TransactionEmailResourceManager GetResourceManager()
        {
            var transaction = Transaction.Current;
            if (transaction == null)
            {
                throw new InvalidOperationException("No active transaction.");
            }

            if (!_table.TryGetValue(transaction, out var rm))
            {
                rm = new TransactionEmailResourceManager();
                _table.Add(transaction, rm);
                transaction.EnlistVolatile(rm, EnlistmentOptions.None);
                transaction.TransactionCompleted += (_, __) =>
                {
                    _table.Remove(transaction);
                };
            }

            return _table.GetValue(transaction, t =>
            {
                var rm = new TransactionEmailResourceManager();
                t.EnlistVolatile(rm, EnlistmentOptions.None);
                return rm;
            });
        }
    }
}