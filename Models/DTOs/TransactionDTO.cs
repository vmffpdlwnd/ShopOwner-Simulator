// Models/DTOs/TransactionDTO.cs
namespace ShopOwnerSimulator.Models.DTOs;

public class TransactionDTO
{
    public string Id { get; set; }
    public string BuyerId { get; set; }
    public string SellerId { get; set; }
    public string ItemTemplateId { get; set; }
    public string ItemName { get; set; }
    public int Quantity { get; set; }
    public long UnitPrice { get; set; }
    public long TotalGold { get; set; }
    public DateTime TransactionTime { get; set; }
    public string Type { get; set; }
}