// Models/Entities/PersonalShopListing.cs
namespace ShopOwnerSimulator.Models.Entities;

public class PersonalShopListing
{
    public string Id { get; set; }
    public string PlayerId { get; set; }
    public string ItemTemplateId { get; set; }
    public int Quantity { get; set; }
    public long UnitPrice { get; set; }
    public DateTime ListedTime { get; set; }
    public DateTime ExpireTime { get; set; }
    public ListingStatus Status { get; set; }
    public long TotalGoldOnSale { get; set; }
}
