// Services/Interfaces/IPersonalShopService.cs
using ShopOwnerSimulator.Models.DTOs;
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services;

public interface IPersonalShopService
{
    Task<PersonalShopListResponse> ListItemAsync(PersonalShopListRequest request);
    Task<List<PersonalShopListing>> GetMyListingsAsync(string playerId);
    Task<bool> CancelListingAsync(string listingId);
    Task<bool> SettleListingAsync(string listingId);
    Task<List<PersonalShopListing>> GetExpiredListingsAsync();
}