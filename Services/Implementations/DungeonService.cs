// Services/Implementations/DungeonService.cs
using ShopOwnerSimulator.Models.DTOs;
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services.Implementations;

public class DungeonService : IDungeonService
{
    private readonly IStateService _stateService;
    private readonly IStorageService _storage;
    private readonly ITimerService _timerService;

    // Mock dungeon data
    private readonly List<Dungeon> _dungeons = new()
    {
        new Dungeon { Id = "dng_001", Name = "Beginner Cave", Level = 1, BaseRewardTime = 300 },
        new Dungeon { Id = "dng_002", Name = "Dark Forest", Level = 5, BaseRewardTime = 600 },
        new Dungeon { Id = "dng_003", Name = "Ancient Ruins", Level = 10, BaseRewardTime = 900 }
    };

    private readonly Dictionary<string, DungeonProgress> _progressTracker = new();

    public DungeonService(IStateService stateService, IStorageService storage, ITimerService timerService)
    {
        _stateService = stateService;
        _storage = storage;
        _timerService = timerService;
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
        mercenary.CurrentDungeonId = request.DungeonId;
        mercenary.DungeonEndTime = endTime;

        // Start timer
        _timerService.StartTimer(progress.Id, endTime, async () =>
        {
            await CompleteDungeonAsync(progress.Id);
        });

        await _storage.SetAsync($"dungeon_progress_{progress.Id}", progress);

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

        // Generate rewards and add to inventory
        var rewards = GenerateRewards(dungeon);
        foreach (var reward in rewards)
        {
            await _stateService.CurrentPlayer.Gold += reward.Value * 10; // Mock
        }

        _timerService.StopTimer(progressId);
        await _storage.SetAsync($"dungeon_progress_{progressId}", progress);
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

public enum DungeonProgressStatus
{
    InProgress,
    Completed,
    Abandoned
}

public class Dungeon
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public int BaseRewardTime { get; set; }
}