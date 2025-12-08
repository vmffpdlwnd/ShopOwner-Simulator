using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services;

public interface IPlayFabService
{
    Task<string> LoginWithEmailAsync(string email, string password);
    Task<string> RegisterWithEmailAsync(string email, string password, string username);
    Task<string> LoginAsGuestAsync(string deviceId);
    Task<Player> GetPlayerAsync(string playerId);
    Task<Player> CreatePlayerAsync(string username);
    Task<bool> UpdatePlayerAsync(Player player);
    Task<bool> UpdatePlayerGoldAsync(string playerId, long goldAmount);
    Task<bool> UpdateDisplayNameAsync(string displayName);
    Task<List<Mercenary>> GetMercenariesAsync(string playerId);
    Task<bool> UpdateMercenaryAsync(Mercenary mercenary);
    Task<List<InventoryItem>> GetInventoryAsync(string playerId);
    Task<bool> UpdateInventoryAsync(string playerId, InventoryItem item);
    Task<bool> IsCurrentUserAdminAsync();
    Task<List<AdminPlayerInfo>> SearchPlayersAsync(string searchTerm);
    Task DeletePlayerAsync(string playFabId);
    Task ResetPlayerAsync(string playFabId);
}

public class AdminPlayerInfo
{
    public string PlayFabId { get; set; }
    public string Username { get; set; }
}