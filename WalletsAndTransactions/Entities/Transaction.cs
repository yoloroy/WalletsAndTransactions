using static WalletsAndTransactions.Entities.TransactionType;

namespace WalletsAndTransactions.Entities;

public record Transaction(
    int Id,
    int WalletId,
    DateOnly Date,
    decimal SumUpdate,
    string? Description = null)
{
    public decimal AbsoluteAmount => decimal.Abs(SumUpdate);

    public TransactionType Type => SumUpdate >= 0 ? Income : Expense;
}