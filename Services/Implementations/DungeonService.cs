// Services/Implementations/DungeonService.cs
using ShopOwnerSimulator.Models.DTOs;
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services.Implementations;

public class DungeonService : IDungeonService
{
    private readonly IStateService _stateService;
    private readonly IDynamoDBService _dynamoDB;
    private readonly ITimerService _timerService;
    private readonly IPlayFabService _playFabService;

    public DungeonService(IStateService stateService, IDynamoDBService dynamoDB, ITimerService timerService, IPlayFabService playFabService)
    {
        _stateService = stateService;
        _dynamoDB = dynamoDB;
        _timerService = timerService;
        _playFabService = playFabService;
    }

    public async Task<List<Dungeon>> GetAvailableDungeonsAsync()
    {
        var dbDungeons = await _dynamoDB.GetAllDungeonsAsync();
        var dungeons = new List<Dungeon>();

        foreach (dynamic d in dbDungeons)
        {
            dungeons.Add(new Dungeon
            {
                Id = d.Id,
                Name = d.Name,
                Level = d.Level,
                BaseRewardTime = d.BaseRewardTime
            });
        }

        return dungeons;
    }

    public async Task<DungeonStartResponse> StartDungeonAsync(DungeonStartRequest request)
    {
        var dbDungeons = await _dynamoDB.GetAllDungeonsAsync();
        Dungeon dungeon = null;
        foreach (dynamic d in dbDungeons)
        {
            if (d.Id == request.DungeonId)
            {
                dungeon = new Dungeon
                {
                    Id = d.Id,
                    Name = d.Name,
                    Level = d.Level,
                    BaseRewardTime = d.BaseRewardTime
                };
                break;
            }
        }
        
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

        // DB에 저장
        await _dynamoDB.SaveDungeonProgressAsync(progress);
        
        mercenary.CurrentDungeonId = request.DungeonId;
        mercenary.DungeonEndTime = endTime;

        // Persist mercenary state to PlayFab
        await _playFabService.UpdateMercenaryAsync(mercenary);
        _stateService.NotifyStateChanged();

        // 타이머 시작
        _timerService.StartTimer(progress.Id, endTime, async () =>
        {
            await CompleteDungeonAsync(progress.Id);
        });

        return new DungeonStartResponse
        {
            ProgressId = progress.Id,
            EndTime = endTime,
            EstimatedRewardItems = GenerateRewards(dungeon)
        };
    }

    public async Task<DungeonProgress> GetDungeonProgressAsync(string progressId)
    {
        return await _dynamoDB.GetDungeonProgressAsync(progressId);
    }

    public async Task<bool> CompleteDungeonAsync(string progressId)
    {
        var progress = await GetDungeonProgressAsync(progressId);
        if (progress == null)
            return false;

        progress.Status = DungeonProgressStatus.Completed;
        var merc = _stateService.Mercenaries.FirstOrDefault(m => m.Id == progress.MercenaryId);
        if (merc != null)
        {
            merc.CurrentDungeonId = null;
            merc.DungeonEndTime = null;
            await _playFabService.UpdateMercenaryAsync(merc);
        }
        
        var dbDungeons = await _dynamoDB.GetAllDungeonsAsync();
        Dungeon dungeon = null;
        foreach (dynamic d in dbDungeons)
        {
            if (d.Id == progress.DungeonId)
            {
                dungeon = new Dungeon
                {
                    Id = d.Id,
                    Name = d.Name,
                    Level = d.Level,
                    BaseRewardTime = d.BaseRewardTime
                };
                break;
            }
        }

        // 보상 생성 및 인벤토리에 추가
        var rewards = GenerateRewards(dungeon);
        foreach (var reward in rewards)
        {
            // TODO: 인벤토리에 실제 추가
            _stateService.CurrentPlayer.Gold += reward.Value * 10;
        }

        _timerService.StopTimer(progressId);
        await _dynamoDB.SaveDungeonProgressAsync(progress);
        await _playFabService.UpdatePlayerAsync(_stateService.CurrentPlayer); // Persist gold changes
        _stateService.NotifyStateChanged();

        return true;
    }

    public async Task<bool> AbandonDungeonAsync(string progressId)
    {
        var progress = await GetDungeonProgressAsync(progressId);
        if (progress == null)
            return false;

        progress.Status = DungeonProgressStatus.Abandoned;
        var merc = _stateService.Mercenaries.FirstOrDefault(m => m.Id == progress.MercenaryId);
        if (merc != null)
        {
            merc.CurrentDungeonId = null;
            merc.DungeonEndTime = null;
            await _playFabService.UpdateMercenaryAsync(merc);
        }
        _timerService.StopTimer(progressId);
        await _dynamoDB.SaveDungeonProgressAsync(progress);
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

// Use `Dungeon` and `DungeonProgressStatus` from `ShopOwnerSimulator.Models.Entities`