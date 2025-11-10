using Core.Model;
using Core.POCOs;

namespace Core;

public sealed class Repository
{
    private int _nextWalletId;
    private int _nextTransactionId;

    private readonly Dictionary<int, Wallet> _wallets = [];

    public bool IsEmpty => _wallets.Count == 0;

    public IEnumerable<Wallet> Wallets => _wallets.Values;

    public IEnumerable<Transaction> Transactions => Wallets.SelectMany(wallet => wallet.Transactions);

    /// <summary>
    /// Добавляет кошельки и транзакции к уже существующим в репозитории, оригинальные идентификаторы при этои теряются.
    /// </summary>
    /// <param name="wallets">Кошельки</param>
    /// <param name="transactions">Транзакции</param>
    /// <returns>Были ли загружены данные</returns>
    public bool TryLoad(IEnumerable<WalletPOCO> wallets, IEnumerable<TransactionPOCO> transactions)
    {
        var transactionsList = transactions.ToLookup(keySelector: poco => poco.WalletId);

        foreach (var wallet in wallets)
        {
            if (wallet.StartingBalance < 0)
            {
                return false;
            }

            var id = _nextWalletId++;
            var loadingId = wallet.Id;

            var pocoTransactions = transactionsList[loadingId].OrderBy(poco => poco.Date).ToList();

            var balance = wallet.StartingBalance;
            foreach (var poco in pocoTransactions)
            {
                if (poco.SumUpdate == 0 ||
                    balance + poco.SumUpdate < 0)
                {
                    return false;
                }

                balance += poco.SumUpdate;
            }

            var walletTransactions = pocoTransactions
                .Select(poco => new Transaction(_nextTransactionId++, poco.Date, poco.SumUpdate, poco.Description));

            _wallets[id] = new Wallet(id, wallet.Name, wallet.CurrencyId, wallet.StartingBalance, walletTransactions.ToList());
        }

        return true;
    }

    public Wallet AddWallet(string name, string currencyId, decimal startingBalance)
    {
        var id = _nextWalletId++;
        var wallet = new Wallet(id, name, currencyId, startingBalance, []);
        _wallets.Add(id, wallet);
        return wallet;
    }

    /// <summary>
    /// Проверяет, могла ли транзакция быть осуществлена и возвращает сформированную транзакцию
    /// </summary>
    /// <param name="walletId">ID кошелька</param>
    /// <param name="date">Дата проведения транзакции</param>
    /// <param name="sumUpdate">Дельта суммы на счету</param>
    /// <param name="description">Описание</param>
    /// <param name="transaction">Сформированный объект транзакции</param>
    /// <returns><c>true</c>, если транзакция могла быть осущствлена, <c>false</c>, если нет</returns>
    public bool TryAddTransaction(int walletId, DateOnly date, decimal sumUpdate, string? description, out Transaction transaction)
    {
        transaction = new Transaction(_nextTransactionId++, date, sumUpdate, description);
        return Transaction.AmountIsNonZero(sumUpdate) && _wallets[walletId].TryAddTransaction(transaction);
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

        return new(incomes, expenses, incomes.Sum(t => t.AbsoluteAmount), expenses.Sum(t => t.AbsoluteAmount));
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
}