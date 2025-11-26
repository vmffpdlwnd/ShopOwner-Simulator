// Services/Interfaces/ICraftingService.cs
using ShopOwnerSimulator.Models.DTOs;
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.Services;

public interface ICraftingService
{
    Task<List<Recipe>> GetRecipesAsync();
    Task<CraftingResponse> CraftAsync(CraftingRequest request);
    Task<bool> CanCraftAsync(CraftingRequest request);
    Task<Recipe> GetRecipeAsync(string recipeId);
}