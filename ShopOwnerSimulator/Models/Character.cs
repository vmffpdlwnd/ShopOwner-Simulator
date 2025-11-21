// Models/Character.cs
public class Character
{
    public string CharacterId { get; set; }
    public string OwnerUserId { get; set; }
    public string Name { get; set; }
    public CharacterJob Job { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }
    public long MonthlySalary { get; set; }
    public bool IsRentable { get; set; }
    public int TimesRented { get; set; }
    public double AverageRating { get; set; }
    public DateTime CreatedAt { get; set; }

    public Character()
    {
        CharacterId = Guid.NewGuid().ToString();
        HP = MaxHP;
        TimesRented = 0;
        AverageRating = 5.0;
        CreatedAt = DateTime.UtcNow;
    }

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