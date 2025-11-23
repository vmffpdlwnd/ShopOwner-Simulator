// Models/Inventory.cs
public class Inventory
{
    public string UserId { get; set; } = string.Empty;
    public int Wood { get; set; }
    public int Iron { get; set; }
    public int Stone { get; set; }
    public int Crystal { get; set; }
    public int Potion { get; set; }
    public List<Item> Items { get; set; }

    public Inventory()
    {
        Wood = 10;
        Iron = 5;
        Stone = 3;
        Crystal = 0;
        Potion = 5;
        Items = new List<Item>();
    }

    public bool HasResources(int wood, int iron, int stone, int crystal = 0)
    {
        return Wood >= wood && Iron >= iron && Stone >= stone && Crystal >= crystal;
    }

    public void ConsumeResources(int wood, int iron, int stone, int crystal = 0)
    {
        Wood -= wood;
        Iron -= iron;
        Stone -= stone;
        Crystal -= crystal;
    }

    public void AddResources(int wood, int iron, int stone, int crystal = 0)
    {
        Wood += wood;
        Iron += iron;
        Stone += stone;
        Crystal += crystal;
    }
}