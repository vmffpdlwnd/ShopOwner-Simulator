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
            Console.Error.WriteLine($"InventoryService.AddItemAsync: Found existing item Id={existingItem.Id}, Template={existingItem.ItemTemplateId}, QtyBefore={existingItem.Quantity}, Add={quantity}");
            existingItem.Quantity += quantity;
            Console.Error.WriteLine($"InventoryService.AddItemAsync: Updated QtyAfter={existingItem.Quantity}");
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
            Console.Error.WriteLine($"InventoryService.AddItemAsync: Added newItem Id={newItem.Id}, Template={newItem.ItemTemplateId}, Quantity={newItem.Quantity}");
        }

        var toUpdate = existingItem ?? _stateService.Inventory.First(i => i.ItemTemplateId == itemTemplateId && i.PlayerId == playerId);
        Console.Error.WriteLine($"InventoryService.AddItemAsync: Updating PlayFab inventory for player={playerId}, itemId={toUpdate.Id}, template={toUpdate.ItemTemplateId}, qty={toUpdate.Quantity}");
        await _playFab.UpdateInventoryAsync(playerId, toUpdate);

        _stateService.NotifyStateChanged();
        return true;
    }

    public async Task<bool> RemoveItemAsync(string playerId, string itemId, int quantity)
    {
        var item = _stateService.Inventory.FirstOrDefault(i =>
            i.PlayerId == playerId && i.Id == itemId);

        if (item == null || item.Quantity < quantity)
            return false;

        Console.Error.WriteLine($"InventoryService.RemoveItemAsync: Removing quantity player={playerId}, itemId={itemId}, template={item.ItemTemplateId}, qtyBefore={item.Quantity}, remove={quantity}");
        item.Quantity -= quantity;
        Console.Error.WriteLine($"InventoryService.RemoveItemAsync: qtyAfter={item.Quantity}");

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