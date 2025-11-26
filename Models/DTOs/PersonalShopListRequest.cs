// Models/DTOs/PersonalShopListRequest.cs
namespace ShopOwnerSimulator.Models.DTOs;

public class PersonalShopListRequest
{
    public string ItemTemplateId { get; set; }
    public int Quantity { get; set; }
    public long UnitPrice { get; set; }
    public int ExpireHours { get; set; }
}