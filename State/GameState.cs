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
        public List<Transaction> Transactions { get; private set; } = new();
    public bool IsLoading { get; private set; } = false;
    public bool IsInitialized { get; private set; } = false;

    public GameState(IGameService gameService, IStateService stateService)
    {
        _gameService = gameService;
        _stateService = stateService;
        _stateService.OnStateChanged += StateServiceChanged;
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

    // Initialize using an existing player id (e.g. after server-side guest/login)
    public async Task InitializeWithPlayerIdAsync(string playerId)
    {
        if (IsInitialized)
            return;

        IsLoading = true;
        try
        {
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
                Transactions = new List<Transaction>(_stateService.Transactions);
            
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

    public async Task LogoutAsync()
    {
        IsLoading = true;
        try
        {
            // Clear state service and in-memory copies
            try
            {
                _stateService.CurrentPlayer = null;
                _stateService.Mercenaries = new List<Mercenary>();
                _stateService.Inventory = new List<InventoryItem>();
                _stateService.Transactions = new List<Transaction>();

                await _stateService.PersistLocalAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Logout: failed to clear state service: {ex}");
            }

            Player = null;
            Mercenaries = new List<Mercenary>();
            Inventory = new List<InventoryItem>();
            Transactions = new List<Transaction>();

            IsInitialized = false;
            NotifyStateChanged();
        }
        finally
        {
            IsLoading = false;
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

    private void StateServiceChanged()
    {
        // When the underlying StateService changes, refresh GameState copies
        _ = LoadGameStateAsync();
    }

    private Task<string> GetOrCreatePlayerId()
    {
        var playerId = Guid.NewGuid().ToString();
        return Task.FromResult(playerId);
    }

    // Initialize a local, ephemeral guest session without contacting remote services.
    public async Task InitializeGuestAsync(Player guestPlayer)
    {
        if (IsInitialized)
            return;

        IsLoading = true;
        try
        {
            // Populate state service with guest data (stored only in memory)
            _stateService.CurrentPlayer = guestPlayer;
            _stateService.Mercenaries = new List<Mercenary>();
            _stateService.Inventory = new List<InventoryItem>();

            // Provide basic starter items and a starter mercenary locally
            var starterMerc = new Mercenary
            {
                Id = Guid.NewGuid().ToString(),
                PlayerId = guestPlayer.Id,
                Name = "초보 용병",
                Level = 1,
                Experience = 0,
                Stats = new MercenaryStats { Attack = 10, Defense = 5, Speed = 10 },
                EquipmentInventory = new Dictionary<int, string>(),
                CurrentDungeonId = null,
                DungeonEndTime = null,
                IsActive = true
            };

            _stateService.Mercenaries.Add(starterMerc);

            var ore = new InventoryItem { Id = Guid.NewGuid().ToString(), PlayerId = guestPlayer.Id, ItemTemplateId = "material_ore", Quantity = 10 };
            var wood = new InventoryItem { Id = Guid.NewGuid().ToString(), PlayerId = guestPlayer.Id, ItemTemplateId = "material_wood", Quantity = 5 };

            _stateService.Inventory.Add(ore);
            _stateService.Inventory.Add(wood);

            // Persist guest data locally so it survives page refreshes
            try
            {
                await _stateService.PersistLocalAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"GameState: Failed to persist guest data: {ex}");
            }

            // Reflect into GameState properties
            await LoadGameStateAsync();
            IsInitialized = true;
        }
        finally
        {
            IsLoading = false;
        }
    }
}