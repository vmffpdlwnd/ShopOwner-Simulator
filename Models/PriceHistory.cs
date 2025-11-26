using System;

namespace ShopOwnerSimulator.Models
{
    public class PriceHistory
    {
        public int Id { get; set; }
        public int MarketItemId { get; set; }
        public decimal Price { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
