// Models/User.cs
public class User
{
    public string UserId { get; set; }
    public string Username { get; set; }
    public int Level { get; set; }
    public long Gold { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    public Inventory Inventory { get; set; }
    public List<Character> Characters { get; set; }
    public Shop Shop { get; set; }
}