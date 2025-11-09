using static WalletsAndTransactions.Model.TransactionType;

namespace WalletsAndTransactions.Model;

public record Transaction(
    int Id,
    int WalletId,
    DateOnly Date,
    decimal SumUpdate,
    string? Description = null)
{
    public decimal AbsoluteAmount => decimal.Abs(SumUpdate);

    public TransactionType Type => SumUpdate >= 0 ? Income : Expense;

    public static bool AmountIsNonZero(decimal amount) => amount != 0;
}