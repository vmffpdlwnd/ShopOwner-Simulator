// Models/Shop.cs
public class Shop
{
    public string ShopId { get; set; }
    public string OwnerUserId { get; set; }
    public string ShopName { get; set; }
    public List<ShopItem> Items { get; set; }
    public long TotalRevenue { get; set; }
    public DateTime LastResetAt { get; set; }

    public Shop()
    {
        ShopId = Guid.NewGuid().ToString();
        Items = new List<ShopItem>();
        TotalRevenue = 0;
        LastResetAt = DateTime.UtcNow;
    }

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
        // 무작위로 상점 아이템 생성
        var random = new Random();
        var items = new List<ShopItem>();

        // 기본 아이템들
        var possibleItems = new[]
        {
            new ShopItem { Name = "초급 전사", Price = 1000, Type = "Character" },
            new ShopItem { Name = "철 (x10)", Price = 100, Type = "Resource" },
            new ShopItem { Name = "포션 (x5)", Price = 50, Type = "Item" },
            new ShopItem { Name = "나무 검", Price = 300, Type = "Equipment" },
            new ShopItem { Name = "방패", Price = 400, Type = "Equipment" },
            new ShopItem { Name = "초급 궁수", Price = 900, Type = "Character" }
        };

        // 랜덤하게 4개 선택
        for (int i = 0; i < 4; i++)
        {
            items.Add(possibleItems[random.Next(possibleItems.Length)]);
        }

        return items;
    }
}

public class ShopItem
{
    public string ItemId { get; set; }
    public string Name { get; set; }
    public long Price { get; set; }
    public string Type { get; set; }
    public int Stock { get; set; }

    public ShopItem()
    {
        ItemId = Guid.NewGuid().ToString();
        Stock = 100;
    }
}