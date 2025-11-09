using WalletsAndTransactions.POCOs;

namespace WalletsAndTransactions.Model;

public class Repository
{
    private int _nextWalletId = 0;
    private int _nextTransactionId = 0;

    private readonly Dictionary<int, Wallet> _wallets = [];

    public bool IsEmpty => _wallets.Count == 0;

    public IEnumerable<Wallet> Wallets => _wallets.Values;

    public IEnumerable<Transaction> Transactions => Wallets.SelectMany(wallet => wallet.Transactions);

    /// <summary>
    /// Добавляет кошельки и транзакции к уже существующим в репозитории, оригинальные идентификаторы при этои теряются.
    /// </summary>
    /// <param name="wallets">Кошельки</param>
    /// <param name="transactions">Транзакции</param>
    public void Load(IEnumerable<WalletPOCO> wallets, IEnumerable<TransactionPOCO> transactions)
    {
        var transactionsList = transactions.ToList();

        foreach (var wallet in wallets)
        {
            var id = _nextWalletId++;
            var loadingId = wallet.Id;

            var walletTransactions = transactionsList
                .Where(poco => poco.WalletId == loadingId)
                .Select(poco => new Transaction(_nextTransactionId++, poco.Date, poco.SumUpdate, poco.Description))
                .ToList();

            _wallets[id] = new Wallet(id, wallet.Name, wallet.CurrencyId, wallet.StartingBalance, walletTransactions);
        }
    }

    public Wallet AddWallet(string name, string currencyId, decimal startingBalance)
    {
        var id = _nextWalletId++;
        var wallet = new Wallet(id, name, currencyId, startingBalance, []);
        _wallets.Add(id, wallet);
        return wallet;
    }

    public bool TryAddTransaction(int walletId, DateOnly date, decimal sumUpdate, string? description, out Transaction transaction)
    {
        transaction = new Transaction(_nextTransactionId, date, sumUpdate, description);
        return _wallets[walletId].TryAddTransaction(transaction);
    }

    public bool TryGetWalletById(int id, out Wallet? wallet) => _wallets.TryGetValue(id, out wallet);

    /// <param name="year">Год</param>
    /// <param name="month">Месяц</param>
    /// <returns><c>MonthlyTransactionsReport</c></returns>
    public MonthlyTransactionsReport GetMonthlyTransactionsReport(int year, int month)
    {
        var thatMonth = Transactions
            .Where(t => t.Date.Month == month && t.Date.Year == year)
            .ToList();

        var incomes = thatMonth
            .Where(transaction => transaction.Type == TransactionType.Income)
            .OrderBy(transaction => transaction.Date)
            .ToList();

        var expenses = thatMonth
            .Where(transaction => transaction.Type == TransactionType.Expense)
            .OrderBy(transaction => transaction.Date)
            .ToList();

        return new (incomes, expenses, incomes.Sum(t => t.AbsoluteAmount), expenses.Sum(t => t.AbsoluteAmount));
    }

    /// <param name="year">Год</param>
    /// <param name="month">Месяц</param>
    /// <returns>3 самые большие траты за указанный месяц для каждого кошелька, отсортированные по убыванию суммы</returns>
    public IEnumerable<(Wallet Wallet, IEnumerable<Transaction> Top3)> GetTop3TransactionsByMonth(int year, int month)
    {
        return Wallets
            .Select(wallet => (wallet, wallet.Transactions
                .Where(transaction =>
                    transaction.Date.Year == year &&
                    transaction.Date.Month == month &&
                    transaction.Type == TransactionType.Expense)
                .OrderByDescending(transaction => transaction.AbsoluteAmount)
                .Take(3)
                .AsEnumerable()))
            .Where(group => group.Item2.Any());
    }

    /// <summary>
    /// Сгруппированные транзакции по типам (Income/Expense),
    /// группы отсортированные по общей сумме (по убыванию),
    /// в каждой группе транзакции отсортированны по дате (от самых старых к самым новым).
    /// </summary>
    /// <param name="Incomes">Зачисления</param>
    /// <param name="Expenses">Списания</param>
    /// <param name="IncomesSum">Абсолютная сумма зачислений</param>
    /// <param name="ExpensesSum">Абсолютная сумма списания</param>
    public record struct MonthlyTransactionsReport(
        IReadOnlyList<Transaction> Incomes,
        IReadOnlyList<Transaction> Expenses,
        decimal IncomesSum,
        decimal ExpensesSum
    );
}