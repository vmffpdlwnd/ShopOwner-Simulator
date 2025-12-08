// Services/Implementations/StateService.cs
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services.Implementations;

public class StateService : IStateService
{
    private readonly IStorageService _storage;
    private readonly IPlayFabService _playFab;
    
    public event Action OnStateChanged;

    public Player CurrentPlayer { get; set; }
    public List<Mercenary> Mercenaries { get; set; } = new();
    public List<InventoryItem> Inventory { get; set; } = new();
    public List<Transaction> Transactions { get; set; } = new();

    public StateService(IStorageService storage, IPlayFabService playFab)
    {
        _storage = storage;
        _playFab = playFab;
    }

    public async Task LoadPlayerStateAsync(string playerId)
    {
        try
        {
            // Load from PlayFab
            CurrentPlayer = await _playFab.GetPlayerAsync(playerId);
            Mercenaries = await _playFab.GetMercenariesAsync(playerId);
            Inventory = await _playFab.GetInventoryAsync(playerId);

            // Cache locally
            await _storage.SetAsync("current_player", CurrentPlayer);
            await _storage.SetAsync("mercenaries", Mercenaries);
            await _storage.SetAsync("inventory", Inventory);

            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading player state: {ex.Message}");
        }
    }

    public async Task SavePlayerStateAsync()
    {
        try
        {
            if (CurrentPlayer != null)
            {
                await _playFab.UpdatePlayerAsync(CurrentPlayer);
            }

            foreach (var merc in Mercenaries)
            {
                await _playFab.UpdateMercenaryAsync(merc);
            }

            if (CurrentPlayer != null)
            {
                foreach (var item in Inventory)
                {
                    await _playFab.UpdateInventoryAsync(CurrentPlayer.Id, item);
                }
            }

            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error saving player state: {ex.Message}");
        }
    }

    public async Task PersistLocalAsync()
    {
        try
        {
            await _storage.SetAsync("current_player", CurrentPlayer);
            await _storage.SetAsync("mercenaries", Mercenaries);
            await _storage.SetAsync("inventory", Inventory);
            await _storage.SetAsync("transactions", Transactions);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"StateService: Failed to persist local state: {ex}");
        }
    }

    public void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }
}