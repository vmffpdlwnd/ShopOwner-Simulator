// Models/DTOs/MercenaryDTO.cs
namespace ShopOwnerSimulator.Models.DTOs;

public class MercenaryDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public long Experience { get; set; }
    public MercenaryStatsDTO Stats { get; set; }
    public Dictionary<int, string> EquipmentInventory { get; set; }
    public string CurrentDungeonId { get; set; }
    public DateTime? DungeonEndTime { get; set; }
    public TimeSpan? RemainingDungeonTime { get; set; }
    public bool IsActive { get; set; }
}

public class MercenaryStatsDTO
{
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }
}