// Services/Interfaces/IGameService.cs
namespace ShopOwnerSimulator.Services;

public interface IGameService
{
    Task InitializeGameAsync(string playerId);
    Task<bool> SaveGameAsync();
    Task<bool> SyncWithServerAsync();
    Task<Dictionary<string, object>> GetGameStateAsync();
}