// Services/Interfaces/IMercenaryService.cs
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services;

public interface IMercenaryService
{
    Task<List<Mercenary>> GetMercenariesAsync(string playerId);
    Task<Mercenary> GetMercenaryAsync(string mercenaryId);
    Task<Mercenary> HireMercenaryAsync(string playerId);
    Task<bool> EquipAsync(string mercenaryId, string itemId);
    Task<bool> UnequipAsync(string mercenaryId, int slotIndex);
    Task<bool> UpdateMercenaryStatsAsync(string mercenaryId);
}