// Models/MarketplaceTransaction.cs
public class MarketplaceTransaction
{
    public string TransactionId { get; set; }
    public string SellerId { get; set; }
    public string BuyerId { get; set; }
    public Item Item { get; set; }
    public long Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public TransactionStatus Status { get; set; }

    public MarketplaceTransaction()
    {
        TransactionId = Guid.NewGuid().ToString();
        CreatedAt = DateTime.UtcNow;
        Status = TransactionStatus.Completed;
    }
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Cancelled
}