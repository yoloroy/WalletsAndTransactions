namespace WalletsAndTransactions.Model;

public class Wallet(
    int id,
    string name,
    string currencyId,
    decimal startingBalance,
    List<Transaction> transactions)
{
    private readonly List<Transaction> _transactions = transactions;

    public readonly int Id = id;
    public readonly string Name = name;
    public readonly string CurrencyId = currencyId;
    public readonly decimal StartingBalance = startingBalance;

    public IEnumerable<Transaction> Transactions => _transactions;

    public decimal Balance => StartingBalance + _transactions.Sum(transaction => transaction.SumUpdate);

    public bool TryAddTransaction(Transaction transaction)
    {
        if (!SupportsTransactionUpdate(transaction.SumUpdate) ||
            !TransactionStoryWillFitWith(transaction.Date, transaction.SumUpdate))
        {
            return false;
        }
        _transactions.Add(transaction);
        return true;
    }

    public bool SupportsTransactionUpdate(decimal update) => Balance + update >= 0;

    public bool TransactionStoryWillFitWith(DateOnly date, decimal update)
    {
        if (update > 0)
        {
            return true;
        }

        var balance = _transactions
            .Where(transaction => transaction.Date <= date)
            .Sum(transaction => transaction.SumUpdate) + StartingBalance;

        if ((balance += update) < 0)
        {
            return false;
        }

        return _transactions
            .Where(transaction => transaction.Date > date)
            .OrderBy(transaction => transaction.Date)
            .ThenBy(transaction => transaction.Id)
            .All(transaction => (balance += transaction.SumUpdate) >= 0);
    }

    public static bool NameIsNotEmpty(string name) => name.Length > 0;

    public static bool CurrencyIdIsNotEmpty(string currencyId) => currencyId.Length > 0;

    public static bool StartingBalanceIsNotNegative(decimal balance) => balance >= 0;
}