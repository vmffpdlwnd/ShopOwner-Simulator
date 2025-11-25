// Services/GameService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShopOwnerSimulator.Models;

namespace ShopOwnerSimulator.Services;

public class GameService
{
    private static User? _currentUser = null;
    private static List<Dungeon> _dungeons = new List<Dungeon>();
    private readonly PlayFabService _playFabService;
    private readonly DataService _dataService;
    private readonly bool _useLocalSave;

    public GameService(Microsoft.Extensions.Configuration.IConfiguration configuration, PlayFabService playFabService, DataService dataService)
    {
        _dungeons = Dungeon.GetDefaultDungeons();
        _playFabService = playFabService;
        _dataService = dataService;
        _useLocalSave = configuration.GetValue<bool>("UseLocalSave", true);

        try
        {
            if (_currentUser == null)
            {
                if (_useLocalSave && _dataService.HasSavedGame())
                    _currentUser = _dataService.LoadUser() ?? null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"초기 데이터 로드 오류: {ex.Message}");
        }
    }

    public async Task<User> CreateNewGameAsync(string username)
    {
        _currentUser = new User
        {
            UserId = Guid.NewGuid().ToString(),
            Username = username,
            Level = 1,
            Gold = 5000,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
            Inventory = new Inventory { UserId = string.Empty },
            Characters = new List<Character>(),
            Shop = new Shop(),
            PlayFabId = string.Empty
        };

        _currentUser.Characters.Add(CreateCharacter("초급 전사", CharacterJob.Warrior, 1, 500));

        if (_useLocalSave) try { _dataService.SaveUser(_currentUser); } catch { }
        else await SaveToPlayFabAsync(_currentUser);

        return _currentUser;
    }

    public User? GetCurrentUser() => _currentUser;
    public void SetCurrentUser(User? user) => _currentUser = user;

    public Character CreateCharacter(string name, CharacterJob job, int level, long salary)
    {
        var character = new Character
        {
            Name = name,
            Job = job,
            Level = level,
            MonthlySalary = salary,
            IsRentable = true
        };

        switch (job)
        {
            case CharacterJob.Warrior:
                character.MaxHP = 100; character.Attack = 20; character.Defense = 15; character.Speed = 8; break;
            case CharacterJob.Archer:
                character.MaxHP = 70; character.Attack = 25; character.Defense = 8; character.Speed = 15; break;
            case CharacterJob.Mage:
                character.MaxHP = 60; character.Attack = 30; character.Defense = 5; character.Speed = 12; break;
            case CharacterJob.Priest:
                character.MaxHP = 80; character.Attack = 15; character.Defense = 10; character.Speed = 10; break;
        }

        character.HP = character.MaxHP;
        return character;
    }

    public async Task<bool> HireCharacterAsync(Character character)
    {
        if (_currentUser == null) return false;

        if (_currentUser.Gold >= character.MonthlySalary)
        {
            _currentUser.Characters.Add(character);
            _currentUser.Gold -= character.MonthlySalary;

            if (_useLocalSave) try { _dataService.SaveUser(_currentUser); } catch { }
            else await SaveToPlayFabAsync(_currentUser);

            return true;
        }
        return false;
    }

    public async Task PayCharacterSalariesAsync()
    {
        if (_currentUser == null) return;

        long totalSalary = _currentUser.Characters.Sum(c => c.MonthlySalary);
        if (_currentUser.Gold >= totalSalary)
        {
            _currentUser.Gold -= totalSalary;

            if (_useLocalSave) try { _dataService.SaveUser(_currentUser); } catch { }
            else await SaveToPlayFabAsync(_currentUser);
        }
    }

    public async Task<DungeonResult> ExploreDungeonAsync(Character character, Dungeon dungeon)
    {
        if (_currentUser == null) throw new InvalidOperationException("CurrentUser is null");

        var result = new DungeonResult
        {
            DungeonName = dungeon.Name ?? "",
            CharacterName = character.Name ?? "",
            Success = CalculateSuccess(character, dungeon)
        };

        if (result.Success)
        {
            result.WoodGained = dungeon.Wood;
            result.IronGained = dungeon.Iron;
            result.StoneGained = dungeon.Stone;
            result.CrystalGained = dungeon.Crystal;
            result.GoldGained = dungeon.Gold;
            result.ExpGained = dungeon.Experience;

            _currentUser.Inventory.AddResources(result.WoodGained, result.IronGained, result.StoneGained, result.CrystalGained);
            _currentUser.Gold += result.GoldGained;
            character.GainExperience(result.ExpGained);
        }
        else
        {
            result.ExpGained = dungeon.Experience / 2;
            character.GainExperience(result.ExpGained);
        }

        if (_useLocalSave) try { _dataService.SaveUser(_currentUser); } catch { }
        else await SaveToPlayFabAsync(_currentUser);

        return result;
    }

    private bool CalculateSuccess(Character character, Dungeon dungeon)
    {
        int successRate = (character.Level * 10) - (dungeon.Difficulty * 15);
        successRate = Math.Max(30, Math.Min(95, successRate));
        var random = new Random();
        return random.Next(100) < successRate;
    }

    public List<Dungeon> GetAvailableDungeons() => _dungeons;

    public async Task<Item?> CraftItemAsync(Recipe recipe)
    {
        if (_currentUser == null) return null;

        if (!_currentUser.Inventory.HasResources(recipe.RequiredWood, recipe.RequiredIron, recipe.RequiredStone, recipe.RequiredCrystal))
            return null;

        _currentUser.Inventory.ConsumeResources(recipe.RequiredWood, recipe.RequiredIron, recipe.RequiredStone, recipe.RequiredCrystal);

        var craftedItem = new Item
        {
            ItemId = Guid.NewGuid().ToString(),
            Name = recipe.ResultName ?? "",
            Type = recipe.ItemType,
            Price = recipe.SellPrice,
            Quantity = 1,
            CreatedAt = DateTime.UtcNow
        };

        _currentUser.Inventory.Items.Add(craftedItem);

        if (_useLocalSave) try { _dataService.SaveUser(_currentUser); } catch { }
        else await SaveToPlayFabAsync(_currentUser);

        return craftedItem;
    }

    public List<Recipe> GetAvailableRecipes()
    {
        return new List<Recipe>
        {
            new Recipe { RecipeId = "recipe_1", ResultName = "철 검", RequiredWood = 2, RequiredIron = 3, ItemType = ItemType.Weapon, SellPrice = 500 },
            new Recipe { RecipeId = "recipe_2", ResultName = "나무 방패", RequiredWood = 5, RequiredStone = 2, ItemType = ItemType.Armor, SellPrice = 300 },
            new Recipe { RecipeId = "recipe_3", ResultName = "강철 창", RequiredWood = 1, RequiredIron = 5, ItemType = ItemType.Weapon, SellPrice = 800 },
            new Recipe { RecipeId = "recipe_4", ResultName = "마법사 지팡이", RequiredIron = 2, RequiredCrystal = 1, ItemType = ItemType.Weapon, SellPrice = 1000 }
        };
    }

    public void GenerateShopItems()
    {
        if (_currentUser == null) return;
        _currentUser.Shop.ResetShopIfNeeded();
    }

    public async Task<bool> BuyFromNPCShopAsync(ShopItem shopItem, int quantity)
    {
        if (_currentUser == null) return false;

        long totalPrice = shopItem.Price * quantity;
        if (_currentUser.Gold < totalPrice) return false;

        _currentUser.Gold -= totalPrice;

        var existingItem = _currentUser.Inventory.Items.FirstOrDefault(i => i.Name == shopItem.Name);
        if (existingItem != null) existingItem.Quantity += quantity;
        else _currentUser.Inventory.Items.Add(new Item
        {
            ItemId = Guid.NewGuid().ToString(),
            Name = shopItem.Name,
            Type = ItemType.Material,
            Price = (int)shopItem.Price,
            Quantity = quantity,
            CreatedAt = DateTime.UtcNow
        });

        if (_useLocalSave) try { _dataService.SaveUser(_currentUser); } catch { }
        else await SaveToPlayFabAsync(_currentUser);

        return true;
    }

    public bool SellItemToMarketplace(Item item, long price)
    {
        if (_currentUser == null) return false;
        var transaction = new MarketplaceTransaction { SellerId = _currentUser.UserId, Item = item, Price = price };
        return true;
    }

    public bool BuyFromMarketplace(MarketplaceTransaction transaction)
    {
        if (_currentUser == null) return false;
        if (_currentUser.Gold < transaction.Price) return false;
        _currentUser.Gold -= transaction.Price;
        _currentUser.Inventory.Items.Add(transaction.Item);
        return true;
    }

    public GameStatistics GetGameStatistics()
    {
        if (_currentUser == null) return new GameStatistics();
        return new GameStatistics
        {
            TotalGold = _currentUser.Gold,
            TotalAssets = _currentUser.Gold + CalculateInventoryValue(),
            CharacterCount = _currentUser.Characters.Count,
            ItemCount = _currentUser.Inventory.Items.Count,
            TotalResources = _currentUser.Inventory.Wood + _currentUser.Inventory.Iron + _currentUser.Inventory.Stone + _currentUser.Inventory.Crystal,
            ShopRevenue = _currentUser.Shop.TotalRevenue
        };
    }

    private long CalculateInventoryValue()
    {
        if (_currentUser == null) return 0;
        long value = 0;
        value += _currentUser.Inventory.Wood * 10;
        value += _currentUser.Inventory.Iron * 20;
        value += _currentUser.Inventory.Stone * 15;
        value += _currentUser.Inventory.Crystal * 50;
        value += _currentUser.Inventory.Items.Sum(i => i.Price * i.Quantity);
        return value;
    }

    private async Task SaveToPlayFabAsync(User user)
    {
        if (user == null) return;
        if (string.IsNullOrEmpty(user.PlayFabId))
            user.PlayFabId = await _playFabService.AuthenticatePlayerAsync(user.UserId) ?? string.Empty;
        if (!string.IsNullOrEmpty(user.PlayFabId))
            await _playFabService.SaveUserDataAsync(user.PlayFabId, user);
    }

    public async Task<bool> SaveCurrentUserToPlayFabAsync()
    {
        if (_currentUser == null) return false;
        await SaveToPlayFabAsync(_currentUser);
        return true;
    }

    public async Task<(bool Success, string Message)> LoginAndSyncAsync(string username, string password)
    {
        try
        {
            var playFabId = await _playFabService.AuthenticateWithPasswordAsync(username, password);
            if (string.IsNullOrEmpty(playFabId))
            {
                await CreateNewGameAsync(username);
                return (true, "계정이 없거나 인증 실패하여 로컬 새 게임을 시작했습니다.");
            }

            var remote = await _playFabService.LoadUserDataAsync(playFabId);
            if (remote == null)
            {
                await CreateNewGameAsync(username);
                return (true, "PlayFab에 저장된 데이터가 없어 로컬 새 게임을 시작했습니다.");
            }

            if (_dataService.HasSavedGame())
            {
                var local = _dataService.LoadUser();
                var localJson = System.Text.Json.JsonSerializer.Serialize(local);
                var remoteJson = System.Text.Json.JsonSerializer.Serialize(remote);

                if (localJson == remoteJson)
                {
                    _currentUser = local;
                    return (true, "로컬 데이터와 PlayFab 데이터가 동일하여 로컬 데이터를 사용합니다.");
                }
                else
                {
                    try { _dataService.SaveUser(remote); } catch { }
                    _currentUser = remote;
                    return (true, "PlayFab 데이터를 로컬에 덮어썼습니다.");
                }
            }
            else
            {
                try { _dataService.SaveUser(remote); } catch { }
                _currentUser = remote;
                return (true, "PlayFab에서 데이터를 가져와 로컬에 저장했습니다.");
            }
        }
        catch (Exception ex)
        {
            return (false, $"오류: {ex.Message}");
        }
    }

    public async Task<bool> OverwriteLocalWithPlayFabAsync(User? user = null)
    {
        var target = user ?? _currentUser;
        if (target == null) return false;

        var playFabId = target.PlayFabId;
        if (string.IsNullOrEmpty(playFabId))
            playFabId = await _playFabService.AuthenticatePlayerAsync(target.UserId) ?? string.Empty;

        if (string.IsNullOrEmpty(playFabId)) return false;

        var remote = await _playFabService.LoadUserDataAsync(playFabId);
        if (remote == null) return false;

        if (string.IsNullOrEmpty(remote.UserId)) remote.UserId = target.UserId;
        remote.PlayFabId = playFabId;

        try { _dataService.SaveUser(remote); } catch { }

        _currentUser = remote;
        return true;
    }
}

// 헬퍼 클래스
public class Recipe
{
    public string RecipeId { get; set; } = string.Empty;
    public string ResultName { get; set; } = string.Empty;
    public int RequiredWood { get; set; }
    public int RequiredIron { get; set; }
    public int RequiredStone { get; set; }
    public int RequiredCrystal { get; set; }
    public ItemType ItemType { get; set; }
    public int SellPrice { get; set; }
}

public class DungeonResult
{
    public string DungeonName { get; set; } = string.Empty;
    public string CharacterName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int WoodGained { get; set; }
    public int IronGained { get; set; }
    public int StoneGained { get; set; }
    public int CrystalGained { get; set; }
    public int GoldGained { get; set; }
    public int ExpGained { get; set; }
}

public class GameStatistics
{
    public long TotalGold { get; set; }
    public long TotalAssets { get; set; }
    public int CharacterCount { get; set; }
    public int ItemCount { get; set; }
    public int TotalResources { get; set; }
    public long ShopRevenue { get; set; }
}
