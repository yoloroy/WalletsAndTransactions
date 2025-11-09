using WalletsAndTransactions.POCOs;

namespace WalletsAndTransactions.Model;

public class Repository
{
    private int _nextWalletId = 0;
    private int _nextTransactionId = 0;

    private readonly Dictionary<int, Wallet> _wallets = [];

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
}