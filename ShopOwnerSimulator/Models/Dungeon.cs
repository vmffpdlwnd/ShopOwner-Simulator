namespace ShopOwnerSimulator.Models
{
    // Models/Dungeon.cs
    public class Dungeon
    {
        public string DungeonId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Difficulty { get; set; }
        public string MainReward { get; set; } = string.Empty;
        public int Wood { get; set; }
        public int Iron { get; set; }
        public int Stone { get; set; }
        public int Crystal { get; set; }
        public int Gold { get; set; }
        public int Experience { get; set; }

        public Dungeon()
        {
            DungeonId = Guid.NewGuid().ToString();
        }

        public static List<Dungeon> GetDefaultDungeons()
        {
            return new List<Dungeon>
            {
                new Dungeon
                {
                    Name = "숲",
                    Difficulty = 1,
                    MainReward = "목재",
                    Wood = 8,
                    Iron = 1,
                    Stone = 0,
                    Crystal = 0,
                    Gold = 200,
                    Experience = 50
                },
                new Dungeon
                {
                    Name = "광산",
                    Difficulty = 2,
                    MainReward = "철",
                    Wood = 2,
                    Iron = 5,
                    Stone = 2,
                    Crystal = 0,
                    Gold = 400,
                    Experience = 100
                },
                new Dungeon
                {
                    Name = "동굴",
                    Difficulty = 3,
                    MainReward = "석재",
                    Wood = 1,
                    Iron = 3,
                    Stone = 6,
                    Crystal = 1,
                    Gold = 600,
                    Experience = 150
                }
            };
        }
    }
}