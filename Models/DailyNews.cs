using System;

namespace ShopOwnerSimulator.Models
{
    public class DailyNews
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
    }
}
