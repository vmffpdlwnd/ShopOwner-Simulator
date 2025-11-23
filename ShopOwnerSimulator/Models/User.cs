// Models/User.cs
public class User
{
    public string UserId { get; set; } = string.Empty;
    public string? PlayFabId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public bool IsLoggedIn { get; set; } = false;
    public int Level { get; set; } = 1;
    public long Gold { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
    public Inventory Inventory { get; set; } = new Inventory();
    public List<Character> Characters { get; set; } = new List<Character>();
    public Shop Shop { get; set; } = new Shop();
}