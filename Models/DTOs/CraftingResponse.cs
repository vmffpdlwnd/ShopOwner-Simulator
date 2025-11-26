// Models/DTOs/CraftingResponse.cs
namespace ShopOwnerSimulator.Models.DTOs;

public class CraftingResponse
{
    public bool Success { get; set; }
    public string ProducedItemId { get; set; }
    public int ProducedQuantity { get; set; }
    public string RecipeId { get; set; }
}