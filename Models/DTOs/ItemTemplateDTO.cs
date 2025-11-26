// Models/DTOs/ItemTemplateDTO.cs
namespace ShopOwnerSimulator.Models.DTOs;

public class ItemTemplateDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Rarity { get; set; }
    public long BasePrice { get; set; }
    public long CurrentPrice { get; set; }
    public string Description { get; set; }
    public ItemStatsDTO Stats { get; set; }
}

public class ItemStatsDTO
{
    public int AttackBonus { get; set; }
    public int DefenseBonus { get; set; }
    public int SpeedBonus { get; set; }
}