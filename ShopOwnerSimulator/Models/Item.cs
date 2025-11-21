// Models/Item.cs
public class Item
{
    public string ItemId { get; set; }
    public string Name { get; set; }
    public ItemType Type { get; set; }
    public int Price { get; set; }
    public string Description { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum ItemType
{
    Weapon,
    Armor,
    Potion,
    Resource,
    Material
}