// Services/Implementations/PersonalShopService.cs
using ShopOwnerSimulator.Models.DTOs;
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services.Implementations;

public class PersonalShopService : IPersonalShopService
{
    private readonly IStateService _stateService;
    private readonly IStorageService _storage;
    private readonly ITimerService _timerService;
    private readonly IInventoryService _inventoryService;

    private readonly List<PersonalShopListing> _listings = new();

    public PersonalShopService(
        IStateService stateService,
        IStorageService storage,
        ITimerService timerService,
        IInventoryService inventoryService)
    {
        _stateService = stateService;
        _storage = storage;
        _timerService = timerService;
        _inventoryService = inventoryService;
    }

    public async Task<PersonalShopListResponse> ListItemAsync(PersonalShopListRequest request)
    {
        // Verify item exists in inventory
        var inventoryItem = _stateService.Inventory.FirstOrDefault(
            i => i.ItemTemplateId == request.ItemTemplateId);
        
        if (inventoryItem == null || inventoryItem.Quantity < request.Quantity)
            throw new Exception("Insufficient inventory");

        // Create listing
        var listing = new PersonalShopListing
        {
            Id = Guid.NewGuid().ToString(),
            PlayerId = _stateService.CurrentPlayer.Id,
            ItemTemplateId = request.ItemTemplateId,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            ListedTime = DateTime.UtcNow,
            ExpireTime = DateTime.UtcNow.AddHours(request.ExpireHours),
            Status = ListingStatus.Active,
            TotalGoldOnSale = request.UnitPrice * request.Quantity
        };

        // Remove from inventory (mark as listed) - use the actual inventory item id
        await _inventoryService.RemoveItemAsync(
            _stateService.CurrentPlayer.Id,
            inventoryItem.Id,
            request.Quantity);

        _listings.Add(listing);
        await _storage.SetAsync($"personal_listing_{listing.Id}", listing);

        // Start settle timer
        _timerService.StartTimer(listing.Id, listing.ExpireTime, async () =>
        {
            await SettleListingAsync(listing.Id);
        });

        return new PersonalShopListResponse
        {
            ListingId = listing.Id,
            TotalPrice = listing.TotalGoldOnSale,
            ExpireTime = listing.ExpireTime
        };
    }

    public Task<List<PersonalShopListing>> GetMyListingsAsync(string playerId)
    {
        return Task.FromResult(_listings.Where(l => l.PlayerId == playerId).ToList());
    }

    public async Task<bool> CancelListingAsync(string listingId)
    {
        var listing = _listings.FirstOrDefault(l => l.Id == listingId);
        if (listing == null || listing.Status != ListingStatus.Active)
            return false;

        // Return item to inventory
        await _inventoryService.AddItemAsync(
            listing.PlayerId,
            listing.ItemTemplateId,
            listing.Quantity);

        listing.Status = ListingStatus.Cancelled;
        _timerService.StopTimer(listingId);
        await _storage.SetAsync($"personal_listing_{listingId}", listing);

        return true;
    }

    public async Task<bool> SettleListingAsync(string listingId)
    {
        var listing = _listings.FirstOrDefault(l => l.Id == listingId);
        if (listing == null)
            return false;

        // Add gold to player
        _stateService.CurrentPlayer.Gold += listing.TotalGoldOnSale;
        listing.Status = ListingStatus.Sold;

        _timerService.StopTimer(listingId);
        await _storage.SetAsync($"personal_listing_{listingId}", listing);
        _stateService.NotifyStateChanged();

        return true;
    }

    public Task<List<PersonalShopListing>> GetExpiredListingsAsync()
    {
        return Task.FromResult(_listings.Where(l =>
            l.Status == ListingStatus.Active &&
            DateTime.UtcNow >= l.ExpireTime).ToList());
    }
}

// PersonalShopListing moved to Models/Entities/PersonalShopListing.cs