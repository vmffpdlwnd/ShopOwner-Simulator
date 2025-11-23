using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShopOwnerSimulator.Services;

namespace ShopOwnerSimulator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly GameService _gameService;

    [BindProperty] public User? CurrentUser { get; set; }
    [BindProperty] public string Action { get; set; } = "";
    [BindProperty] public string Username { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    [BindProperty] public int SelectedCharacterId { get; set; }
    [BindProperty] public int SelectedDungeonId { get; set; }
    [BindProperty] public string SelectedRecipeId { get; set; } = "";
    [BindProperty] public int BuyQuantity { get; set; } = 1;

    public List<Character> AvailableCharacters { get; set; } = new();
    public List<Dungeon> AvailableDungeons { get; set; } = new();
    public List<Recipe> AvailableRecipes { get; set; } = new();
    public List<ShopItem> ShopItems { get; set; } = new();
    public DungeonResult? LastDungeonResult { get; set; }
    public GameStatistics? Statistics { get; set; }
    public string Message { get; set; } = "";
    public string MessageType { get; set; } = "info";

    public IndexModel(ILogger<IndexModel> logger, GameService gameService)
    {
        _logger = logger;
        _gameService = gameService;
    }

    public async Task OnGetAsync()
    {
        if (_gameService.GetCurrentUser() == null)
        {
            await _gameService.CreateNewGameAsync("Guest Player");
        }

        CurrentUser = _gameService.GetCurrentUser();
        LoadGameData();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        CurrentUser = _gameService.GetCurrentUser();
        if (CurrentUser == null)
        {
            await _gameService.CreateNewGameAsync("Guest Player");
            CurrentUser = _gameService.GetCurrentUser();
        }

        switch (Action)
        {
            case "new_game": return await HandleNewGame();
            case "explore_dungeon": return await HandleExploreDungeon();
            case "craft_item": return await HandleCraftItem();
            case "buy_shop": return await HandleBuyFromShop();
            case "upload_playfab": return await HandleUploadToPlayFab();
            case "login": return await HandleLogin();
            case "logout": return HandleLogout();
            case "sync_from_playfab": return await HandleSyncFromPlayFab();
            case "sell_marketplace": return HandleSellMarketplace();
            case "pay_salaries": return await HandlePaySalaries();
            case "hire_character": return await HandleHireCharacter();
            default: break;
        }

        LoadGameData();
        return Page();
    }

    // ==================== 액션 핸들러 ====================

    private async Task<IActionResult> HandleNewGame()
    {
        await _gameService.CreateNewGameAsync(Username ?? "Guest Player");
        CurrentUser = _gameService.GetCurrentUser();
        Message = "새 게임을 시작했습니다!";
        MessageType = "success";
        LoadGameData();
        return Page();
    }

    private async Task<IActionResult> HandleExploreDungeon()
    {
        try
        {
            if (CurrentUser == null)
            {
                Message = "유저 정보가 없습니다.";
                MessageType = "error";
                return Page();
            }

            var selectedDungeon = _gameService.GetAvailableDungeons()
                .FirstOrDefault(d => d.DungeonId == SelectedDungeonId.ToString());

            if (selectedDungeon == null || CurrentUser.Characters.Count == 0)
            {
                Message = "던전을 선택하거나 용병을 고용해주세요.";
                MessageType = "error";
            }
            else
            {
                var character = CurrentUser.Characters[0];
                var result = await _gameService.ExploreDungeonAsync(character, selectedDungeon);
                LastDungeonResult = result;

                if (result.Success)
                {
                    Message = $"✓ {selectedDungeon.Name} 탐험 성공! " +
                              $"목재 +{result.WoodGained}, 철 +{result.IronGained}, 골드 +{result.GoldGained}";
                    MessageType = "success";
                }
                else
                {
                    Message = $"✗ {selectedDungeon.Name} 탐험 실패! 경험치만 획득했습니다.";
                    MessageType = "error";
                }
            }
        }
        catch (Exception ex)
        {
            Message = $"오류 발생: {ex.Message}";
            MessageType = "error";
            _logger.LogError(ex, "던전 탐험 중 오류");
        }

        LoadGameData();
        return Page();
    }

    private async Task<IActionResult> HandleCraftItem()
    {
        try
        {
            var selectedRecipe = _gameService.GetAvailableRecipes()
                .FirstOrDefault(r => r.RecipeId == SelectedRecipeId);

            if (selectedRecipe == null)
            {
                Message = "레시피를 선택해주세요.";
                MessageType = "error";
            }
            else
            {
                var craftedItem = await _gameService.CraftItemAsync(selectedRecipe);
                if (craftedItem != null)
                {
                    Message = $"✓ {craftedItem.Name}을(를) 제작했습니다! (판매가: {craftedItem.Price}G)";
                    MessageType = "success";
                }
                else
                {
                    Message = $"✗ 자원이 부족합니다. 필요: 목재 {selectedRecipe.RequiredWood}, " +
                              $"철 {selectedRecipe.RequiredIron}, 석재 {selectedRecipe.RequiredStone}";
                    MessageType = "error";
                }
            }
        }
        catch (Exception ex)
        {
            Message = $"오류 발생: {ex.Message}";
            MessageType = "error";
            _logger.LogError(ex, "아이템 제작 중 오류");
        }

        LoadGameData();
        return Page();
    }

    private async Task<IActionResult> HandleBuyFromShop()
    {
        try
        {
            _gameService.GenerateShopItems();

            if (ShopItems.Count == 0 || BuyQuantity <= 0)
            {
                Message = "구매할 아이템과 수량을 선택해주세요.";
                MessageType = "error";
            }
            else
            {
                var shopItem = ShopItems[0];

                if (CurrentUser != null && await _gameService.BuyFromNPCShopAsync(shopItem, BuyQuantity))
                {
                    Message = $"✓ {shopItem.Name} x{BuyQuantity}을(를) {shopItem.Price * BuyQuantity}G에 구매했습니다!";
                    MessageType = "success";
                }
                else
                {
                    Message = $"✗ 골드가 부족합니다. 필요: {shopItem.Price * BuyQuantity}G";
                    MessageType = "error";
                }
            }
        }
        catch (Exception ex)
        {
            Message = $"오류 발생: {ex.Message}";
            MessageType = "error";
            _logger.LogError(ex, "상점 구매 중 오류");
        }

        LoadGameData();
        return Page();
    }

    private IActionResult HandleSellMarketplace()
    {
        try
        {
            if (CurrentUser == null || CurrentUser.Inventory.Items.Count == 0)
            {
                Message = "판매할 아이템이 없습니다.";
                MessageType = "error";
            }
            else
            {
                var itemToSell = CurrentUser.Inventory.Items[0];
                var sellPrice = itemToSell.Price;

                if (_gameService.SellItemToMarketplace(itemToSell, sellPrice))
                {
                    Message = $"✓ {itemToSell.Name}을(를) 거래소에 판매 등록했습니다! (가격: {sellPrice}G)";
                    MessageType = "success";
                }
                else
                {
                    Message = "거래소 판매에 실패했습니다.";
                    MessageType = "error";
                }
            }
        }
        catch (Exception ex)
        {
            Message = $"오류 발생: {ex.Message}";
            MessageType = "error";
            _logger.LogError(ex, "거래소 판매 중 오류");
        }

        LoadGameData();
        return Page();
    }

    private async Task<IActionResult> HandlePaySalaries()
    {
        try
        {
            if (CurrentUser == null)
            {
                Message = "유저 정보가 없습니다.";
                MessageType = "error";
                return Page();
            }

            long totalSalary = CurrentUser.Characters.Sum(c => c.MonthlySalary);

            if (CurrentUser.Gold < totalSalary)
            {
                Message = $"✗ 용병 급여가 부족합니다. 필요: {totalSalary}G, 보유: {CurrentUser.Gold}G";
                MessageType = "error";
            }
            else
            {
                await _gameService.PayCharacterSalariesAsync();
                Message = $"✓ 용병 급여 {totalSalary}G를 지급했습니다!";
                MessageType = "success";
            }
        }
        catch (Exception ex)
        {
            Message = $"오류 발생: {ex.Message}";
            MessageType = "error";
            _logger.LogError(ex, "급여 지급 중 오류");
        }

        LoadGameData();
        return Page();
    }

    private async Task<IActionResult> HandleHireCharacter()
    {
        try
        {
            if (CurrentUser == null)
            {
                Message = "유저 정보가 없습니다.";
                MessageType = "error";
                return Page();
            }

            var newCharacter = _gameService.CreateCharacter("새로운 용병", CharacterJob.Archer, 1, 500);
            long hireCost = newCharacter.MonthlySalary * 2;

            if (CurrentUser.Gold < hireCost)
            {
                Message = $"✗ 용병 고용 비용이 부족합니다. 필요: {hireCost}G";
                MessageType = "error";
            }
            else
            {
                var ok = await _gameService.HireCharacterAsync(newCharacter);
                if (ok)
                {
                    Message = $"✓ 새로운 용병을 고용했습니다! (비용: {hireCost}G)";
                    MessageType = "success";
                }
                else
                {
                    Message = "용병 고용에 실패했습니다.";
                    MessageType = "error";
                }
            }
        }
        catch (Exception ex)
        {
            Message = $"오류 발생: {ex.Message}";
            MessageType = "error";
            _logger.LogError(ex, "용병 고용 중 오류");
        }

        LoadGameData();
        return Page();
    }

    private async Task<IActionResult> HandleLogin()
    {
        try
        {
            var authService = HttpContext.RequestServices.GetRequiredService<AuthService>();
            var (success, message, user) = await authService.LoginOrRegisterAsync(Username, Password);

            if (success && user != null)
            {
                _gameService.SetCurrentUser(user);
                Message = message;
                MessageType = "success";
            }
            else
            {
                Message = message;
                MessageType = "error";
            }
        }
        catch (Exception ex)
        {
            Message = $"오류: {ex.Message}";
            MessageType = "error";
        }

        LoadGameData();
        return Page();
    }

    private IActionResult HandleLogout()
    {
        var user = _gameService.GetCurrentUser();
        if (user != null)
        {
            var authService = HttpContext.RequestServices.GetRequiredService<AuthService>();
            authService.Logout(user);
            _gameService.SetCurrentUser(null);
            Message = "로그아웃되었습니다.";
            MessageType = "success";
        }

        LoadGameData();
        return Page();
    }

    private async Task<IActionResult> HandleUploadToPlayFab()
    {
        try
        {
            var ok = await _gameService.SaveCurrentUserToPlayFabAsync();
            if (ok)
            {
                Message = "PlayFab에 업로드했습니다.";
                MessageType = "success";
            }
            else
            {
                Message = "PlayFab 업로드에 실패했습니다.";
                MessageType = "error";
            }
        }
        catch (Exception ex)
        {
            Message = $"오류 발생: {ex.Message}";
            MessageType = "error";
            _logger.LogError(ex, "PlayFab 업로드 중 오류");
        }

        LoadGameData();
        return Page();
    }

    private async Task<IActionResult> HandleSyncFromPlayFab()
    {
        try
        {
            var ok = await _gameService.OverwriteLocalWithPlayFabAsync();
            if (ok)
            {
                Message = "PlayFab에서 데이터를 가져와 로컬에 덮어썼습니다.";
                MessageType = "success";
            }
            else
            {
                Message = "PlayFab에서 데이터를 불러오지 못했습니다.";
                MessageType = "error";
            }
        }
        catch (Exception ex)
        {
            Message = $"오류 발생: {ex.Message}";
            MessageType = "error";
            _logger.LogError(ex, "PlayFab 동기화 중 오류");
        }

        LoadGameData();
        return Page();
    }

    // ==================== 헬퍼 메서드 ====================

    private void LoadGameData()
    {
        CurrentUser = _gameService.GetCurrentUser();
        if (CurrentUser != null)
        {
            AvailableCharacters = CurrentUser.Characters.ToList();
            AvailableDungeons = _gameService.GetAvailableDungeons();
            AvailableRecipes = _gameService.GetAvailableRecipes();
            _gameService.GenerateShopItems();
            ShopItems = CurrentUser.Shop.Items;
            Statistics = _gameService.GetGameStatistics();
        }
    }
}
