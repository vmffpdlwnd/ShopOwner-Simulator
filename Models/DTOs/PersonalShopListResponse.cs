// Models/DTOs/PersonalShopListResponse.cs
namespace ShopOwnerSimulator.Models.DTOs;

public class PersonalShopListResponse
{
    public string ListingId { get; set; }
    public long TotalPrice { get; set; }
    public DateTime ExpireTime { get; set; }
}