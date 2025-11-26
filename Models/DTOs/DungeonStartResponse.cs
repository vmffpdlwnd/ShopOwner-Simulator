// Models/DTOs/DungeonStartResponse.cs
namespace ShopOwnerSimulator.Models.DTOs;

public class DungeonStartResponse
{
    public string ProgressId { get; set; }
    public DateTime EndTime { get; set; }
    public List<KeyValuePair<string, int>> EstimatedRewardItems { get; set; }
}