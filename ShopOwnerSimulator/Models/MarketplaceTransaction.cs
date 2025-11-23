// Models/MarketplaceTransaction.cs
public class MarketplaceTransaction
{
    public string TransactionId { get; set; } = Guid.NewGuid().ToString();
    public string SellerId { get; set; } = string.Empty;
    public string BuyerId { get; set; } = string.Empty;
    public Item Item { get; set; } = new Item();
    public long Price { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public TransactionStatus Status { get; set; } = TransactionStatus.Completed;
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Cancelled
}