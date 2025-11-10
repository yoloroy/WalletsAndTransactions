using static Core.Model.TransactionType;

namespace Core.Model;

public sealed record Transaction(
    int Id,
    DateOnly Date,
    decimal SumUpdate,
    string? Description = null)
{
    public decimal AbsoluteAmount => decimal.Abs(SumUpdate);

    public TransactionType Type => SumUpdate >= 0 ? Income : Expense;

    public static bool AmountIsNonZero(decimal amount) => amount != 0;
}