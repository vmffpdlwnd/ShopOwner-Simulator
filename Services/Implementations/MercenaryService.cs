// Services/Implementations/MercenaryService.cs
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services.Implementations;

public class MercenaryService : IMercenaryService
{
    private readonly IStateService _stateService;
    private readonly IStorageService _storage;
    private readonly IPlayFabService _playFab;

    public MercenaryService(
        IStateService stateService,
        IStorageService storage,
        IPlayFabService playFab)
    {
        _stateService = stateService;
        _storage = storage;
        _playFab = playFab;
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

        var mercenary = new Mercenary
        {
            Id = Guid.NewGuid().ToString(),
            PlayerId = playerId,
            Name = $"Mercenary_{Guid.NewGuid().ToString().Substring(0, 8)}",
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

        // Find first empty slot
        int slotIndex = 0;
        while (mercenary.EquipmentInventory.ContainsKey(slotIndex))
        {
            slotIndex++;
        }

        mercenary.EquipmentInventory[slotIndex] = itemId;
        item.IsEquipped = true;
        item.EquippedMercenaryId = mercenaryId;

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

        var item = _stateService.Inventory.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            item.IsEquipped = false;
            item.EquippedMercenaryId = null;
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

public class MercenaryStats
{
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }
}