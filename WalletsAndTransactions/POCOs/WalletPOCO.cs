using WalletsAndTransactions.Model;

namespace WalletsAndTransactions.POCOs;

public class WalletPOCO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string CurrencyId { get; set; }
    public decimal StartingBalance { get; set; }

    public Wallet ToEntity(List<Transaction> transactionsTable) => new(Id, Name, CurrencyId, StartingBalance, transactionsTable);
}