// Models/DTOs/DungeonProgressDTO.cs
namespace ShopOwnerSimulator.Models.DTOs;

public class DungeonProgressDTO
{
    public string Id { get; set; }
    public string MercenaryId { get; set; }
    public string DungeonId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; }
    public TimeSpan RemainingTime { get; set; }
}