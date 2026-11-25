using System.Transactions;

namespace DominoTrainGame.Server.Persistence.Data;

internal static class SerializableTransaction
{
    public static TransactionScope Begin()
    {
        TransactionOptions transactionOptions = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.Serializable
        };
        TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, transactionOptions);
        return transaction;
    }
}
