// Models/Entities/Player.cs
namespace ShopOwnerSimulator.Models.Entities;

public class Player
{
    public string Id { get; set; }
    public string Username { get; set; }
    public int Level { get; set; }
    public long Experience { get; set; }
    public long Gold { get; set; }
    public long Crystal { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
}