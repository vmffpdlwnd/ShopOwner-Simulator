// Models/DTOs/ExchangeListRequest.cs
namespace ShopOwnerSimulator.Models.DTOs;

public class ExchangeListRequest
{
    public string ItemTemplateId { get; set; }
    public int Quantity { get; set; }
    public long UnitPrice { get; set; }
}