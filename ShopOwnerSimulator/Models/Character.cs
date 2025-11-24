namespace ShopOwnerSimulator.Models
{
    public class Character
    {
        public string CharacterId { get; set; } = Guid.NewGuid().ToString();
        public string OwnerUserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public CharacterJob Job { get; set; } = CharacterJob.Warrior;
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public int HP { get; set; } = 100;
        public int MaxHP { get; set; } = 100;
        public int Attack { get; set; } = 10;
        public int Defense { get; set; } = 5;
        public int Speed { get; set; } = 5;
        public long MonthlySalary { get; set; } = 0;
        public bool IsRentable { get; set; } = false;
        public int TimesRented { get; set; } = 0;
        public double AverageRating { get; set; } = 5.0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public void TakeDamage(int damage)
        {
            HP -= damage;
            if (HP < 0) HP = 0;
        }

        public void Heal(int amount)
        {
            HP += amount;
            if (HP > MaxHP) HP = MaxHP;
        }

        public void GainExperience(int exp)
        {
            Experience += exp;
            if (Experience >= 100)
            {
                LevelUp();
                Experience = 0;
            }
        }

        public void LevelUp()
        {
            Level++;
            Attack += 5;
            Defense += 3;
            Speed += 2;
            MaxHP += 10;
            HP = MaxHP;
        }
    }

    public enum CharacterJob
    {
        Warrior,
        Archer,
        Mage,
        Priest
    }
}