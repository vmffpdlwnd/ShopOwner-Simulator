// Models/Entities/Recipe.cs
namespace ShopOwnerSimulator.Models.Entities;

public class Recipe
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Dictionary<string, int> RequiredItems { get; set; } = new();
    public string OutputItem { get; set; }
    public int OutputQuantity { get; set; }
}
