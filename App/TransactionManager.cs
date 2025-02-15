using App.DataTypes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace App;

public static class TransactionManager
{
    public static List<Transaction> GetTransactions()
    {
        // Get the transactions from the configuration or create a new one
        var transactions = Configuration.GetValue<List<Transaction>>("Transactions") ?? new();
        return transactions;
    }

    public static void AddTransaction(Transaction transactionToAdd)
    {
        var transactions = GetTransactions();

        // Add the transaction and save the configuration
        transactions.Add(transactionToAdd);
        Configuration.SetValue("Transactions", transactions);
        Configuration.WriteBuffer();
    }

    public static void AddTransactions(IEnumerable<Transaction> transactionsToAdd)
    {
        var transactions = GetTransactions();

        // Add the transaction and save the configuration
        transactions.AddRange(transactionsToAdd);
        Configuration.SetValue("Transactions", transactions);
        Configuration.WriteBuffer();
    }

    public static void SaveTransaction(Transaction transactionToSave)
    {
        var transactions = GetTransactions();

        // Find the transaction. If the transaction is not found, index will be -1
        var index = transactions.FindIndex(x => x.Id == transactionToSave.Id);

        // If the transaction is not found, add it
        if (index < 0)
        {
            AddTransaction(transactionToSave);
            return;
        }

        // If the transaction is found, replace it. And save the configuration
        transactions[index] = transactionToSave;
        Configuration.SetValue("Transactions", transactions);
        Configuration.WriteBuffer();
    }

    public static void RemoveTransaction(string transactionId)
    {
        var transactions = GetTransactions();

        // Remove the transaction and save the configuration
        transactions.RemoveAll(x => x.Id == transactionId);
        Configuration.SetValue("Transactions", transactions);
        Configuration.WriteBuffer();
    }

    public static void RemoveTransactionsByRecordId(string recordId)
    {
        var transactions = GetTransactions();

        // Remove the transactions with the specified record id and save the configuration
        transactions.RemoveAll(x => x.RecordId == recordId);
        Configuration.SetValue("Transactions", transactions);
        Configuration.WriteBuffer();
    }

    public static void ClearTransactions()
    {
        // Clear the transactions and save the configuration
        Configuration.SetValue("Transactions", new List<Transaction>());
        Configuration.WriteBuffer();
    }
}
