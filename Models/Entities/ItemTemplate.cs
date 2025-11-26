// Models/Entities/ItemTemplate.cs
namespace ShopOwnerSimulator.Models.Entities;

public class ItemTemplate
{
    public string Id { get; set; }
    public string Name { get; set; }
    public ItemType Type { get; set; }
    public ItemRarity Rarity { get; set; }
    public long BasePrice { get; set; }
    public string Description { get; set; }
    public ItemStats Stats { get; set; }
}

public class ItemStats
{
    public int AttackBonus { get; set; }
    public int DefenseBonus { get; set; }
    public int SpeedBonus { get; set; }
}