// Models/DTOs/ExchangeBuyRequest.cs
namespace ShopOwnerSimulator.Models.DTOs;

public class ExchangeBuyRequest
{
    public string OrderId { get; set; }
    public int Quantity { get; set; }
}