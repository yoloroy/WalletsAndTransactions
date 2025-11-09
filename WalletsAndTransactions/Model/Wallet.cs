namespace WalletsAndTransactions.Model;

public class Wallet(
    int id,
    string name,
    string currencyId,
    decimal startingBalance,
    List<Transaction> transactionsTable)
{
    public readonly int Id = id;
    public readonly string Name = name;
    public readonly string CurrencyId = currencyId;
    public readonly decimal StartingBalance = startingBalance;

    public IEnumerable<Transaction> Transactions => transactionsTable.Where(transaction => transaction.WalletId == Id);
    public decimal Balance => StartingBalance + Transactions.Sum(transaction => transaction.SumUpdate);

    public bool TryAddTransaction(Transaction transaction)
    {
        if (SupportsTransactionUpdate(transaction.SumUpdate))
        {
            return false;
        }

        transactionsTable.Add(transaction);
        return true;
    }

    public bool SupportsTransactionUpdate(decimal update) => Balance + update >= 0;

    public static bool NameIsNotEmpty(string name) => name.Length > 0;

    public static bool CurrencyIdIsNotEmpty(string currencyId) => currencyId.Length > 0;

    public static bool StartingBalanceIsNotNegative(decimal balance) => balance >= 0;
}