namespace Core.POCOs;

public sealed class TransactionPOCO
{
    public int Id { get; set; }
    public int WalletId { get; set; }
    public DateOnly Date { get; set; }
    public decimal SumUpdate { get; set; }
    public string? Description { get; set; } = null;
}