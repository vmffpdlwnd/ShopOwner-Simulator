// Services/Interfaces/IDungeonService.cs
using ShopOwnerSimulator.Models.DTOs;
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services;

public interface IDungeonService
{
    Task<List<Dungeon>> GetAvailableDungeonsAsync();
    Task<DungeonStartResponse> StartDungeonAsync(DungeonStartRequest request);
    Task<DungeonProgress> GetDungeonProgressAsync(string progressId);
    Task<bool> CompleteDungeonAsync(string progressId);
    Task<bool> AbandonDungeonAsync(string progressId);
}