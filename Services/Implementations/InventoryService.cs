// Services/Implementations/InventoryService.cs
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services.Implementations;

public class InventoryService : IInventoryService
{
    private readonly IStateService _stateService;
    private readonly IStorageService _storage;
    private readonly IPlayFabService _playFab;

    public InventoryService(
        IStateService stateService,
        IStorageService storage,
        IPlayFabService playFab)
    {
        _stateService = stateService;
        _storage = storage;
        _playFab = playFab;
    }

    public async Task<List<InventoryItem>> GetInventoryAsync(string playerId)
    {
        return _stateService.Inventory.Where(i => i.PlayerId == playerId).ToList();
    }

    public async Task<InventoryItem> GetItemAsync(string playerId, string itemId)
    {
        return _stateService.Inventory.FirstOrDefault(i =>
            i.PlayerId == playerId && i.Id == itemId);
    }

    public async Task<bool> AddItemAsync(string playerId, string itemTemplateId, int quantity)
    {
        // Check if item already exists in inventory
        var existingItem = _stateService.Inventory.FirstOrDefault(i =>
            i.PlayerId == playerId && i.ItemTemplateId == itemTemplateId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            var newItem = new InventoryItem
            {
                Id = Guid.NewGuid().ToString(),
                PlayerId = playerId,
                ItemTemplateId = itemTemplateId,
                Quantity = quantity,
                IsEquipped = false,
                EquippedMercenaryId = null
            };

            _stateService.Inventory.Add(newItem);
            await _storage.SetAsync($"inventory_item_{newItem.Id}", newItem);
        }

        await _playFab.UpdateInventoryAsync(playerId, existingItem ?? 
            _stateService.Inventory.First(i => i.ItemTemplateId == itemTemplateId && i.PlayerId == playerId));

        _stateService.NotifyStateChanged();
        return true;
    }

    public async Task<bool> RemoveItemAsync(string playerId, string itemId, int quantity)
    {
        var item = _stateService.Inventory.FirstOrDefault(i =>
            i.PlayerId == playerId && i.Id == itemId);

        if (item == null || item.Quantity < quantity)
            return false;

        item.Quantity -= quantity;

        if (item.Quantity <= 0)
        {
            _stateService.Inventory.Remove(item);
            await _storage.RemoveAsync($"inventory_item_{item.Id}");
        }
        else
        {
            await _storage.SetAsync($"inventory_item_{item.Id}", item);
        }

        await _playFab.UpdateInventoryAsync(playerId, item);
        _stateService.NotifyStateChanged();
        return true;
    }

    public async Task<bool> TransferItemAsync(string fromPlayerId, string toPlayerId, string itemId, int quantity)
    {
        var item = _stateService.Inventory.FirstOrDefault(i =>
            i.PlayerId == fromPlayerId && i.Id == itemId);

        if (item == null || item.Quantity < quantity)
            return false;

        // Remove from source
        await RemoveItemAsync(fromPlayerId, itemId, quantity);

        // Add to destination
        await AddItemAsync(toPlayerId, item.ItemTemplateId, quantity);

        return true;
    }
}