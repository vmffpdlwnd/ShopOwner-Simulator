// State/GameState.cs
using ShopOwnerSimulator.Models.Entities;
using ShopOwnerSimulator.Services;

namespace ShopOwnerSimulator.State;

public class GameState
{
    private readonly IGameService _gameService;
    private readonly IStateService _stateService;

    public event Action? OnStateChanged;

    public Player? Player { get; private set; }
    public List<Mercenary> Mercenaries { get; private set; } = new();
    public List<InventoryItem> Inventory { get; private set; } = new();
    public bool IsLoading { get; private set; } = false;
    public bool IsInitialized { get; private set; } = false;

    public GameState(IGameService gameService, IStateService stateService)
    {
        _gameService = gameService;
        _stateService = stateService;
        _stateService.OnStateChanged += NotifyStateChanged;
    }

    public async Task InitializeAsync()
    {
        if (IsInitialized)
            return;

        IsLoading = true;
        try
        {
            var playerId = await GetOrCreatePlayerId();
            await _gameService.InitializeGameAsync(playerId);
            
            await LoadGameStateAsync();
            IsInitialized = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadGameStateAsync()
    {
        IsLoading = true;
        try
        {
            Player = _stateService.CurrentPlayer;
            Mercenaries = new List<Mercenary>(_stateService.Mercenaries);
            Inventory = new List<InventoryItem>(_stateService.Inventory);
            
            NotifyStateChanged();
            await Task.CompletedTask;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadPlayerAsync()
    {
        if (Player == null)
        {
            await LoadGameStateAsync();
        }
    }

    public async Task SaveGameAsync()
    {
        IsLoading = true;
        try
        {
            await _gameService.SaveGameAsync();
            NotifyStateChanged();
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task SyncWithServerAsync()
    {
        IsLoading = true;
        try
        {
            await _gameService.SyncWithServerAsync();
            await LoadGameStateAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void UpdatePlayer(Player player)
    {
        Player = player;
        _stateService.CurrentPlayer = player;
        NotifyStateChanged();
    }

    public void UpdateMercenaries(List<Mercenary> mercenaries)
    {
        Mercenaries = new List<Mercenary>(mercenaries);
        _stateService.Mercenaries = mercenaries;
        NotifyStateChanged();
    }

    public void UpdateInventory(List<InventoryItem> inventory)
    {
        Inventory = new List<InventoryItem>(inventory);
        _stateService.Inventory = inventory;
        NotifyStateChanged();
    }

    public void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }

    private Task<string> GetOrCreatePlayerId()
    {
        var playerId = Guid.NewGuid().ToString();
        return Task.FromResult(playerId);
    }
}