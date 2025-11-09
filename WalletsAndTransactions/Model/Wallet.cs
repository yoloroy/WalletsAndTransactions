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
        if (Balance + transaction.SumUpdate < 0)
        {
            return false;
        }

        transactionsTable.Add(transaction);
        return true;
    }
}