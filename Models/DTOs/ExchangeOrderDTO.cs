// Models/DTOs/ExchangeOrderDTO.cs
namespace ShopOwnerSimulator.Models.DTOs;

public class ExchangeOrderDTO
{
    public string Id { get; set; }
    public string SellerId { get; set; }
    public string ItemTemplateId { get; set; }
    public string ItemName { get; set; }
    public int Quantity { get; set; }
    public int Remaining { get; set; }
    public long UnitPrice { get; set; }
    public DateTime ListedTime { get; set; }
    public string Status { get; set; }
}