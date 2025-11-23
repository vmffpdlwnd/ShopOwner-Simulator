// Models/Shop.cs
public class Shop
{
    public string ShopId { get; set; } = Guid.NewGuid().ToString();
    public string OwnerUserId { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public List<ShopItem> Items { get; set; } = new List<ShopItem>();
    public long TotalRevenue { get; set; } = 0;
    public DateTime LastResetAt { get; set; } = DateTime.UtcNow;

    public void ResetShopIfNeeded()
    {
        if ((DateTime.UtcNow - LastResetAt).TotalHours >= 24)
        {
            Items = GenerateNewItems();
            LastResetAt = DateTime.UtcNow;
        }
    }

    private List<ShopItem> GenerateNewItems()
    {
        var random = new Random();
        var items = new List<ShopItem>();

        var possibleItems = new[]
        {
            new ShopItem { Name = "초급 전사", Price = 1000, Type = "Character" },
            new ShopItem { Name = "철 (x10)", Price = 100, Type = "Resource" },
            new ShopItem { Name = "포션 (x5)", Price = 50, Type = "Item" },
            new ShopItem { Name = "나무 검", Price = 300, Type = "Equipment" },
            new ShopItem { Name = "방패", Price = 400, Type = "Equipment" },
            new ShopItem { Name = "초급 궁수", Price = 900, Type = "Character" }
        };

        for (int i = 0; i < 4; i++)
            items.Add(possibleItems[random.Next(possibleItems.Length)]);

        return items;
    }
}

public class ShopItem
{
    public string ItemId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public long Price { get; set; } = 0;
    public string Type { get; set; } = string.Empty;
    public int Stock { get; set; } = 100;
}