namespace Core.POCOs;

public sealed class WalletPOCO
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string CurrencyId { get; set; }
    public decimal StartingBalance { get; set; }
}