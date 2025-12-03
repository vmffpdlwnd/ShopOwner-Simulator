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
            RequiredItems = new() { { "material_ore", 5 } },
            OutputItem = "equipment_sword",
            OutputQuantity = 1
        },
        new Recipe
        {
            Id = "rec_002",
            Name = "Leather Armor",
            RequiredItems = new() { { "material_wood", 3 }, { "material_ore", 2 } },
            OutputItem = "equipment_armor",
            OutputQuantity = 1
        },
        new Recipe
        {
            Id = "rec_003",
            Name = "Health Potion",
            RequiredItems = new() { { "material_herb", 2 } },
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

        // Consume required items (find actual inventory item IDs by template id)
        foreach (var required in recipe.RequiredItems)
        {
            var invItem = _stateService.Inventory.FirstOrDefault(i => i.ItemTemplateId == required.Key && i.PlayerId == _stateService.CurrentPlayer.Id);
            if (invItem == null)
            {
                throw new Exception($"Cannot craft: missing required item {required.Key}");
            }

            await _inventoryService.RemoveItemAsync(
                invItem.PlayerId,
                invItem.Id,
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
        await Task.CompletedTask;
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
        await Task.CompletedTask;
        return _recipes.FirstOrDefault(r => r.Id == recipeId)!;
    }
}

// Recipe moved to Models/Entities/Recipe.cs