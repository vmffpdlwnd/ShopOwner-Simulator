// Services/Implementations/GameService.cs
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services.Implementations;

public class GameService : IGameService
{
    private readonly IPlayFabService _playFab;
    private readonly IStateService _stateService;
    private readonly IStorageService _storage;
    private readonly IMercenaryService _mercenaryService;
    private readonly IInventoryService _inventoryService;

    public GameService(
        IPlayFabService playFab,
        IStateService stateService,
        IStorageService storage,
        IMercenaryService mercenaryService,
        IInventoryService inventoryService)
    {
        _playFab = playFab;
        _stateService = stateService;
        _storage = storage;
        _mercenaryService = mercenaryService;
        _inventoryService = inventoryService;
    }

    public async Task InitializeGameAsync(string playerId)
    {
        try
        {
            // Load or create player
            Player player = null;
            try
            {
                player = await _playFab.GetPlayerAsync(playerId);
            }
            catch
            {
                // Create new player if doesn't exist
                player = await _playFab.CreatePlayerAsync($"Player_{Guid.NewGuid().ToString().Substring(0, 8)}");
            }

            // Load player state
            await _stateService.LoadPlayerStateAsync(player.Id);

            // Give starter mercenary if none exists
            if (_stateService.Mercenaries.Count == 0)
            {
                await _mercenaryService.HireMercenaryAsync(player.Id);
            }

            // Give starter items if inventory empty
            if (_stateService.Inventory.Count == 0)
            {
                await _inventoryService.AddItemAsync(player.Id, "material_ore", 10);
                await _inventoryService.AddItemAsync(player.Id, "material_wood", 5);
            }

            await _stateService.SavePlayerStateAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error initializing game: {ex}");
            throw;
        }
    }

    public async Task<bool> SaveGameAsync()
    {
        try
        {
            await _stateService.SavePlayerStateAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error saving game: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SyncWithServerAsync()
    {
        try
        {
            // Sync player data
            if (_stateService.CurrentPlayer != null)
            {
                await _playFab.UpdatePlayerAsync(_stateService.CurrentPlayer);
            }

            // Sync mercenaries
            foreach (var merc in _stateService.Mercenaries)
            {
                await _playFab.UpdateMercenaryAsync(merc);
            }

            // Sync inventory
            foreach (var item in _stateService.Inventory)
            {
                await _playFab.UpdateInventoryAsync(_stateService.CurrentPlayer.Id, item);
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error syncing with server: {ex.Message}");
            return false;
        }
    }

    public async Task<Dictionary<string, object>> GetGameStateAsync()
    {
        return await Task.FromResult(new Dictionary<string, object>
        {
            { "player", _stateService.CurrentPlayer },
            { "mercenaries", _stateService.Mercenaries },
            { "inventory", _stateService.Inventory },
            { "timestamp", DateTime.UtcNow }
        });
    }
}