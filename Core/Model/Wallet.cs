namespace Core.Model;

public sealed class Wallet(
    int id,
    string name,
    string currencyId,
    decimal startingBalance,
    IReadOnlyList<Transaction> transactions)
{
    private readonly List<Transaction> _transactions = transactions.ToList();

    public int Id { get; } = id;
    public string Name { get; } = name;
    public string CurrencyId { get; } = currencyId;
    public decimal StartingBalance { get; } = startingBalance;

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

    public static bool NameIsNotEmpty(string name) => name is { Length: > 0 };

    public static bool CurrencyIdIsNotEmpty(string currencyId) => !string.IsNullOrEmpty(currencyId);

    public static bool StartingBalanceIsNotNegative(decimal balance) => balance >= 0;
}