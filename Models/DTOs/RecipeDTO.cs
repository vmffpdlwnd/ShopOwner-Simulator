// Models/DTOs/RecipeDTO.cs
namespace ShopOwnerSimulator.Models.DTOs;

public class RecipeDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Dictionary<string, int> RequiredItems { get; set; }
    public string OutputItem { get; set; }
    public int OutputQuantity { get; set; }
    public bool CanCraft { get; set; }
}