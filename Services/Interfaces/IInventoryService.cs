// Services/Interfaces/IInventoryService.cs
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services;

public interface IInventoryService
{
    Task<List<InventoryItem>> GetInventoryAsync(string playerId);
    Task<InventoryItem> GetItemAsync(string playerId, string itemId);
    Task<bool> AddItemAsync(string playerId, string itemTemplateId, int quantity);
    Task<bool> RemoveItemAsync(string playerId, string itemId, int quantity);
    Task<bool> TransferItemAsync(string fromPlayerId, string toPlayerId, string itemId, int quantity);
}