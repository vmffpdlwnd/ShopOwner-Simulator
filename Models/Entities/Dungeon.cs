// Models/Entities/Dungeon.cs
namespace ShopOwnerSimulator.Models.Entities;

public class Dungeon
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public int BaseRewardTime { get; set; }
}
