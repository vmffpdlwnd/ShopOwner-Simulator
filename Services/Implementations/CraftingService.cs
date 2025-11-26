// Services/Implementations/CraftingService.cs
using ShopOwnerSimulator.Models.DTOs;
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services.Implementations;

public class CraftingService : ICraftingService
{
    private readonly IStateService _stateService;
    private readonly IStorageService _storage;
    private readonly IInventoryService _inventoryService;

    private readonly List<Recipe> _recipes = new()
    {
        new Recipe
        {
            Id = "rec_001",
            Name = "Iron Sword",
            RequiredItems = new() { new("material_ore", 5) },
            OutputItem = "equipment_sword",
            OutputQuantity = 1
        },
        new Recipe
        {
            Id = "rec_002",
            Name = "Leather Armor",
            RequiredItems = new() { new("material_wood", 3), new("material_ore", 2) },
            OutputItem = "equipment_armor",
            OutputQuantity = 1
        },
        new Recipe
        {
            Id = "rec_003",
            Name = "Health Potion",
            RequiredItems = new() { new("material_herb", 2) },
            OutputItem = "consumable_potion",
            OutputQuantity = 5
        }
    };

    public CraftingService(
        IStateService stateService,
        IStorageService storage,
        IInventoryService inventoryService)
    {
        _stateService = stateService;
        _storage = storage;
        _inventoryService = inventoryService;
    }

    public async Task<List<Recipe>> GetRecipesAsync()
    {
        return await Task.FromResult(_recipes);
    }

    public async Task<CraftingResponse> CraftAsync(CraftingRequest request)
    {
        if (!await CanCraftAsync(request))
            throw new Exception("Cannot craft: insufficient materials");

        var recipe = _recipes.FirstOrDefault(r => r.Id == request.RecipeId);
        if (recipe == null)
            throw new Exception("Recipe not found");

        // Consume required items
        foreach (var required in recipe.RequiredItems)
        {
            await _inventoryService.RemoveItemAsync(
                _stateService.CurrentPlayer.Id,
                required.Key,
                required.Value);
        }

        // Add output item
        await _inventoryService.AddItemAsync(
            _stateService.CurrentPlayer.Id,
            recipe.OutputItem,
            recipe.OutputQuantity);

        _stateService.NotifyStateChanged();

        return new CraftingResponse
        {
            Success = true,
            ProducedItemId = recipe.OutputItem,
            ProducedQuantity = recipe.OutputQuantity,
            RecipeId = request.RecipeId
        };
    }

    public async Task<bool> CanCraftAsync(CraftingRequest request)
    {
        var recipe = _recipes.FirstOrDefault(r => r.Id == request.RecipeId);
        if (recipe == null)
            return false;

        foreach (var required in recipe.RequiredItems)
        {
            var item = _stateService.Inventory.FirstOrDefault(i => i.ItemTemplateId == required.Key);
            if (item == null || item.Quantity < required.Value)
                return false;
        }

        return true;
    }

    public async Task<Recipe> GetRecipeAsync(string recipeId)
    {
        return _recipes.FirstOrDefault(r => r.Id == recipeId);
    }
}

public class Recipe
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Dictionary<string, int> RequiredItems { get; set; } = new();
    public string OutputItem { get; set; }
    public int OutputQuantity { get; set; }
}