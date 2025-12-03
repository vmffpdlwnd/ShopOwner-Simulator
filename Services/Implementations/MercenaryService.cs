// Services/Implementations/MercenaryService.cs
using ShopOwnerSimulator.Models.Entities;
using ShopOwnerSimulator.Services;

namespace ShopOwnerSimulator.Services.Implementations;

public class MercenaryService : IMercenaryService
{
    private readonly IStateService _stateService;
    private readonly IStorageService _storage;
    private readonly IPlayFabService _playFab;
    private readonly IInventoryService _inventoryService;

    public MercenaryService(
        IStateService stateService,
        IStorageService storage,
        IPlayFabService playFab,
        IInventoryService inventoryService)
    {
        _stateService = stateService;
        _storage = storage;
        _playFab = playFab;
        _inventoryService = inventoryService;
    }

    public async Task<List<Mercenary>> GetMercenariesAsync(string playerId)
    {
        return _stateService.Mercenaries.Where(m => m.PlayerId == playerId).ToList();
    }

    public async Task<Mercenary> GetMercenaryAsync(string mercenaryId)
    {
        return _stateService.Mercenaries.FirstOrDefault(m => m.Id == mercenaryId);
    }

    public async Task<Mercenary> HireMercenaryAsync(string playerId)
    {
        // Check player has enough gold (cost defined in constants)
        const long hireCost = 1000;
        if (_stateService.CurrentPlayer.Gold < hireCost)
            throw new Exception("Insufficient gold to hire mercenary");

        string[] koreanNames = new[] { "서준", "민준", "하윤", "지우", "도윤", "수아", "예린", "준호", "현우", "지훈", "유진", "은우", "지민", "소율", "태민" };
        var rnd = new Random();
        var chosen = koreanNames[rnd.Next(koreanNames.Length)];

        var mercenary = new Mercenary
        {
            Id = Guid.NewGuid().ToString(),
            PlayerId = playerId,
            Name = chosen,
            Level = 1,
            Experience = 0,
            Stats = new MercenaryStats
            {
                Attack = 10,
                Defense = 5,
                Speed = 10
            },
            EquipmentInventory = new Dictionary<int, string>(),
            CurrentDungeonId = null,
            DungeonEndTime = null,
            IsActive = true
        };

        _stateService.CurrentPlayer.Gold -= hireCost;
        _stateService.Mercenaries.Add(mercenary);

        await _playFab.UpdateMercenaryAsync(mercenary);
        await _storage.SetAsync($"mercenary_{mercenary.Id}", mercenary);

        _stateService.NotifyStateChanged();
        return mercenary;
    }

    public async Task<bool> EquipAsync(string mercenaryId, string itemId)
    {
        var mercenary = await GetMercenaryAsync(mercenaryId);
        if (mercenary == null)
            return false;

        var item = _stateService.Inventory.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            return false;

        Console.Error.WriteLine($"MercenaryService.EquipAsync: Equipping itemId={itemId} (template={item.ItemTemplateId}) to mercenaryId={mercenaryId}");

        string equippedItemId;

        // If player has more than one of this template, consume one and create an equipped instance
        if (item.Quantity > 1)
        {
            // Remove one from the stack
            await _inventoryService.RemoveItemAsync(item.PlayerId, item.Id, 1);

            // Create a unique equipped item entry
            var newEquipped = new InventoryItem
            {
                Id = Guid.NewGuid().ToString(),
                PlayerId = item.PlayerId,
                ItemTemplateId = item.ItemTemplateId,
                Quantity = 1,
                IsEquipped = true,
                EquippedMercenaryId = mercenaryId
            };

            _stateService.Inventory.Add(newEquipped);
            await _storage.SetAsync($"inventory_item_{newEquipped.Id}", newEquipped);
            await _playFab.UpdateInventoryAsync(newEquipped.PlayerId, newEquipped);

            equippedItemId = newEquipped.Id;
        }
        else
        {
            // Single item - mark as equipped
            item.IsEquipped = true;
            item.EquippedMercenaryId = mercenaryId;
            await _storage.SetAsync($"inventory_item_{item.Id}", item);
            await _playFab.UpdateInventoryAsync(item.PlayerId, item);

            equippedItemId = item.Id;
        }

        // Find first empty slot
        int slotIndex = 0;
        while (mercenary.EquipmentInventory.ContainsKey(slotIndex))
        {
            slotIndex++;
        }

        mercenary.EquipmentInventory[slotIndex] = equippedItemId;

        Console.Error.WriteLine($"MercenaryService.EquipAsync: Equipped slot={slotIndex}, itemId={equippedItemId}, mercenary={mercenaryId}");

        await UpdateMercenaryStatsAsync(mercenaryId);
        await _storage.SetAsync($"mercenary_{mercenaryId}", mercenary);

        _stateService.NotifyStateChanged();
        return true;
    }

    public async Task<bool> UnequipAsync(string mercenaryId, int slotIndex)
    {
        var mercenary = await GetMercenaryAsync(mercenaryId);
        if (mercenary == null || !mercenary.EquipmentInventory.ContainsKey(slotIndex))
            return false;

        var itemId = mercenary.EquipmentInventory[slotIndex];
        mercenary.EquipmentInventory.Remove(slotIndex);

        Console.Error.WriteLine($"MercenaryService.UnequipAsync: Unequipping slot={slotIndex} itemId={itemId} from mercenaryId={mercenaryId}");

        var item = _stateService.Inventory.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            if (item.IsEquipped)
            {
                // Remove the equipped instance and return stack of template +1
                await _inventoryService.RemoveItemAsync(item.PlayerId, item.Id, 1);
                await _inventoryService.AddItemAsync(item.PlayerId, item.ItemTemplateId, 1);
            }
            else
            {
                // Fallback: mark as unequipped
                item.IsEquipped = false;
                item.EquippedMercenaryId = null;
            }
        }

        await UpdateMercenaryStatsAsync(mercenaryId);
        await _storage.SetAsync($"mercenary_{mercenaryId}", mercenary);

        _stateService.NotifyStateChanged();
        return true;
    }

    public async Task<bool> UpdateMercenaryStatsAsync(string mercenaryId)
    {
        var mercenary = await GetMercenaryAsync(mercenaryId);
        if (mercenary == null)
            return false;

        // Reset to base stats
        mercenary.Stats.Attack = 10 + (mercenary.Level * 2);
        mercenary.Stats.Defense = 5 + (mercenary.Level * 1);
        mercenary.Stats.Speed = 10 + (mercenary.Level * 1);

        // Apply equipment bonuses
        foreach (var equipment in mercenary.EquipmentInventory.Values)
        {
            var item = _stateService.Inventory.FirstOrDefault(i => i.Id == equipment);
            if (item != null)
            {
                // Apply stat bonuses from equipment template
                // This would come from ItemTemplate in real implementation
                mercenary.Stats.Attack += 5;
                mercenary.Stats.Defense += 3;
            }
        }

        await _storage.SetAsync($"mercenary_{mercenaryId}", mercenary);
        return true;
    }
}

