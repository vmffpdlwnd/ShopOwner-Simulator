// Services/Implementations/PersonalShopService.cs
using ShopOwnerSimulator.Models.DTOs;
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services.Implementations;

public class PersonalShopService : IPersonalShopService
{
    private readonly IStateService _stateService;
    private readonly IDynamoDBService _dynamoDB;
    private readonly ITimerService _timerService;
    private readonly IInventoryService _inventoryService;

    public PersonalShopService(
        IStateService stateService,
        IDynamoDBService dynamoDB,
        ITimerService timerService,
        IInventoryService inventoryService)
    {
        _stateService = stateService;
        _dynamoDB = dynamoDB;
        _timerService = timerService;
        _inventoryService = inventoryService;
    }

    public async Task<PersonalShopListResponse> ListItemAsync(PersonalShopListRequest request)
    {
        // 인벤토리 확인
        var inventoryItem = _stateService.Inventory.FirstOrDefault(
            i => i.ItemTemplateId == request.ItemTemplateId);
        
        if (inventoryItem == null || inventoryItem.Quantity < request.Quantity)
            throw new Exception("인벤토리가 부족합니다");

        // 등록 생성
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

        // 인벤토리에서 제거
        await _inventoryService.RemoveItemAsync(
            _stateService.CurrentPlayer.Id,
            request.ItemTemplateId,
            request.Quantity);

        // DB에 저장
        await _dynamoDB.SavePersonalShopListingAsync(listing);

        // 타이머 시작
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

    public async Task<List<PersonalShopListing>> GetMyListingsAsync(string playerId)
    {
        return await _dynamoDB.GetPlayerListingsAsync(playerId);
    }

    public async Task<bool> CancelListingAsync(string listingId)
    {
        var listing = await _dynamoDB.GetPersonalShopListingAsync(listingId);
        if (listing == null || listing.Status != ListingStatus.Active)
            return false;

        // 인벤토리에 반환
        await _inventoryService.AddItemAsync(
            listing.PlayerId,
            listing.ItemTemplateId,
            listing.Quantity);

        listing.Status = ListingStatus.Cancelled;
        _timerService.StopTimer(listingId);
        await _dynamoDB.SavePersonalShopListingAsync(listing);

        return true;
    }

    public async Task<bool> SettleListingAsync(string listingId)
    {
        var listing = await _dynamoDB.GetPersonalShopListingAsync(listingId);
        if (listing == null)
            return false;

        // 골드 추가
        _stateService.CurrentPlayer.Gold += listing.TotalGoldOnSale;
        listing.Status = ListingStatus.Sold;

        _timerService.StopTimer(listingId);
        await _dynamoDB.SavePersonalShopListingAsync(listing);
        _stateService.NotifyStateChanged();

        return true;
    }

    public async Task<List<PersonalShopListing>> GetExpiredListingsAsync()
    {
        var allListings = await _dynamoDB.GetPlayerListingsAsync(_stateService.CurrentPlayer.Id);
        return allListings.Where(l =>
            l.Status == ListingStatus.Active &&
            DateTime.UtcNow >= l.ExpireTime).ToList();
    }
}