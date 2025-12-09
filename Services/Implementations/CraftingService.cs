// Services/Implementations/CraftingService.cs
using ShopOwnerSimulator.Models.DTOs;
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services.Implementations;

public class CraftingService : ICraftingService
{
    private readonly IStateService _stateService;
    private readonly IStorageService _storage;
    private readonly IInventoryService _inventoryService;
    private readonly IDynamoDBService _dynamoDB;

    public CraftingService(
        IStateService stateService,
        IStorageService storage,
        IInventoryService inventoryService,
        IDynamoDBService dynamoDB)
    {
        _stateService = stateService;
        _storage = storage;
        _inventoryService = inventoryService;
        _dynamoDB = dynamoDB;
    }

    public async Task<List<Recipe>> GetRecipesAsync()
    {
        var dbRecipes = await _dynamoDB.GetAllRecipesAsync();
        var recipes = new List<Recipe>();

        foreach (dynamic r in dbRecipes)
        {
            recipes.Add(new Recipe
            {
                Id = r.Id,
                Name = r.Name,
                RequiredItems = r.RequiredItems,
                OutputItem = r.OutputItem,
                OutputQuantity = r.OutputQuantity
            });
        }

        return recipes;
    }

    public async Task<CraftingResponse> CraftAsync(CraftingRequest request)
    {
        if (!await CanCraftAsync(request))
            throw new Exception("Cannot craft: insufficient materials");

        var dbRecipes = await _dynamoDB.GetAllRecipesAsync();
        Recipe recipe = null;
        foreach (dynamic r in dbRecipes)
        {
            if (r.Id == request.RecipeId)
            {
                recipe = new Recipe
                {
                    Id = r.Id,
                    Name = r.Name,
                    RequiredItems = r.RequiredItems,
                    OutputItem = r.OutputItem,
                    OutputQuantity = r.OutputQuantity
                };
                break;
            }
        }
        
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
        var dbRecipes = await _dynamoDB.GetAllRecipesAsync();
        Recipe recipe = null;
        foreach (dynamic r in dbRecipes)
        {
            if (r.Id == request.RecipeId)
            {
                recipe = new Recipe
                {
                    Id = r.Id,
                    Name = r.Name,
                    RequiredItems = r.RequiredItems,
                    OutputItem = r.OutputItem,
                    OutputQuantity = r.OutputQuantity
                };
                break;
            }
        }
        
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
        var dbRecipes = await _dynamoDB.GetAllRecipesAsync();
        foreach (dynamic r in dbRecipes)
        {
            if (r.Id == recipeId)
            {
                return new Recipe
                {
                    Id = r.Id,
                    Name = r.Name,
                    RequiredItems = r.RequiredItems,
                    OutputItem = r.OutputItem,
                    OutputQuantity = r.OutputQuantity
                };
            }
        }
        
        return null!;
    }
}

// Recipe moved to Models/Entities/Recipe.cs