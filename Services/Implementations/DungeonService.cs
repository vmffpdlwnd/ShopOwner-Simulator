// Services/Implementations/DungeonService.cs
using ShopOwnerSimulator.Models.DTOs;
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services.Implementations;

public class DungeonService : IDungeonService
{
    private readonly IStateService _stateService;
    private readonly IStorageService _storage;
    private readonly ITimerService _timerService;
    private readonly IInventoryService _inventoryService;
    private readonly IMercenaryService _mercenaryService;

    // Mock dungeon data
    private readonly List<Dungeon> _dungeons = new()
    {
        new Dungeon { Id = "dng_001", Name = "Beginner Cave", Level = 1, BaseRewardTime = 300 },
        new Dungeon { Id = "dng_002", Name = "Dark Forest", Level = 5, BaseRewardTime = 600 },
        new Dungeon { Id = "dng_003", Name = "Ancient Ruins", Level = 10, BaseRewardTime = 900 }
    };

    private readonly Dictionary<string, DungeonProgress> _progressTracker = new();

    public DungeonService(IStateService stateService, IStorageService storage, ITimerService timerService,
        IInventoryService inventoryService, IMercenaryService mercenaryService)
    {
        _stateService = stateService;
        _storage = storage;
        _timerService = timerService;
        _inventoryService = inventoryService;
        _mercenaryService = mercenaryService;
    }

    public async Task<List<Dungeon>> GetAvailableDungeonsAsync()
    {
        return await Task.FromResult(_dungeons);
    }

    public async Task<DungeonStartResponse> StartDungeonAsync(DungeonStartRequest request)
    {
        var dungeon = _dungeons.FirstOrDefault(d => d.Id == request.DungeonId);
        if (dungeon == null)
            throw new Exception("Dungeon not found");

        var mercenary = _stateService.Mercenaries.FirstOrDefault(m => m.Id == request.MercenaryId);
        if (mercenary == null)
            throw new Exception("Mercenary not found");

        // Calculate reward time based on mercenary stats
        var speedMultiplier = 1.0 - (mercenary.Stats.Speed / 100.0);
        var rewardTime = (int)(dungeon.BaseRewardTime * speedMultiplier);
        var endTime = DateTime.UtcNow.AddSeconds(rewardTime);

        var progress = new DungeonProgress
        {
            Id = Guid.NewGuid().ToString(),
            MercenaryId = request.MercenaryId,
            DungeonId = request.DungeonId,
            StartTime = DateTime.UtcNow,
            EndTime = endTime,
            Status = DungeonProgressStatus.InProgress
        };

        _progressTracker[progress.Id] = progress;
        // Store the progress Id on the mercenary so we can reference/abandon later
        mercenary.CurrentDungeonId = progress.Id;
        mercenary.DungeonEndTime = endTime;

        // Start timer
        _timerService.StartTimer(progress.Id, endTime, async () =>
        {
            await CompleteDungeonAsync(progress.Id);
        });

        await _storage.SetAsync($"dungeon_progress_{progress.Id}", progress);
        await _storage.SetAsync($"mercenary_{mercenary.Id}", mercenary);

        return new DungeonStartResponse
        {
            ProgressId = progress.Id,
            EndTime = endTime,
            EstimatedRewardItems = GenerateRewards(dungeon)
        };
    }

    public async Task<DungeonProgress> GetDungeonProgressAsync(string progressId)
    {
        if (_progressTracker.TryGetValue(progressId, out var progress))
        {
            return progress;
        }

        return await _storage.GetAsync<DungeonProgress>($"dungeon_progress_{progressId}");
    }

    public async Task<bool> CompleteDungeonAsync(string progressId)
    {
        var progress = await GetDungeonProgressAsync(progressId);
        if (progress == null)
            return false;

        progress.Status = DungeonProgressStatus.Completed;
        var dungeon = _dungeons.FirstOrDefault(d => d.Id == progress.DungeonId);

        // Generate rewards and process them
        var rewards = GenerateRewards(dungeon!);

        // Award gold (mocked: each reward unit gives some gold)
        foreach (var reward in rewards)
        {
            _stateService.CurrentPlayer.Gold += reward.Value * 10;
        }

        // Also add item rewards to inventory
        if (_stateService.CurrentPlayer != null)
        {
            foreach (var reward in rewards)
            {
                // If reward is a material/equipment template id, add to inventory
                await _inventoryService.AddItemAsync(_stateService.CurrentPlayer.Id, reward.Key, reward.Value);
            }
        }

        // Award experience to the mercenary and clear dungeon assignment
        var merc = _stateService.Mercenaries.FirstOrDefault(m => m.Id == progress.MercenaryId);
        if (merc != null && dungeon != null)
        {
            var rnd = new Random();
            var xpGain = dungeon.Level * 10 + rnd.Next(5, 16);
            merc.Experience += xpGain;

            // Simple level-up loop
            while (merc.Experience >= merc.Level * 100)
            {
                merc.Experience -= merc.Level * 100;
                merc.Level += 1;
            }

            // Clear dungeon fields
            merc.CurrentDungeonId = null;
            merc.DungeonEndTime = null;

            // Recalculate stats after potential level up / equipment
            await _mercenaryService.UpdateMercenaryStatsAsync(merc.Id);
        }

        _timerService.StopTimer(progressId);

        // Persist progress and full player state (includes mercenaries & inventory)
        await _storage.SetAsync($"dungeon_progress_{progressId}", progress);
        await _stateService.SavePlayerStateAsync();

        _stateService.NotifyStateChanged();

        return true;
    }

    public async Task<bool> AbandonDungeonAsync(string progressId)
    {
        var progress = await GetDungeonProgressAsync(progressId);
        if (progress == null)
            return false;

        progress.Status = DungeonProgressStatus.Abandoned;
        _timerService.StopTimer(progressId);
        await _storage.SetAsync($"dungeon_progress_{progressId}", progress);

        // Clear mercenary assignment if present
        var merc = _stateService.Mercenaries.FirstOrDefault(m => m.Id == progress.MercenaryId);
        if (merc != null)
        {
            merc.CurrentDungeonId = null;
            merc.DungeonEndTime = null;
            await _storage.SetAsync($"mercenary_{merc.Id}", merc);
        }

        // Persist overall state (inventory/mercenaries/player)
        await _stateService.SavePlayerStateAsync();
        _stateService.NotifyStateChanged();

        return true;
    }

    private List<KeyValuePair<string, int>> GenerateRewards(Dungeon dungeon)
    {
        var rewards = new List<KeyValuePair<string, int>>
        {
            new("material_ore", new Random().Next(5, 15)),
            new("material_wood", new Random().Next(3, 10))
        };

        if (new Random().Next(0, 100) < 5) // 5% chance for rare item
        {
            rewards.Add(new("equipment_sword", 1));
        }

        return rewards;
    }
}

// Dungeon and DungeonProgressStatus moved to Models/Entities