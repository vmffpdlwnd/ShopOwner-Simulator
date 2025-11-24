namespace ShopOwnerSimulator.Models
{
    // Models/Item.cs
    public class Item
    {
        public string ItemId { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public ItemType Type { get; set; }       // 기존 enum 사용
        public long Price { get; set; }
        public int Quantity { get; set; } = 1;   // 추가
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 추가
    }

    // Models/ItemType.cs
    public enum ItemType
    {
        Material,
        Weapon,
        Armor,
        Consumable
    }
}
