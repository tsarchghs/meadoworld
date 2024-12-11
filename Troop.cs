using Microsoft.Xna.Framework;

namespace MeadoworldMono
{
    public enum TroopType
    {
        Infantry,
        Cavalry,
        Archer,
        Elite,
        Recruit
    }

    public class Troop
    {
        public string Name { get; set; }
        public float MaxHealth { get; set; }
        public float AttackRange { get; set; }
        public float Speed { get; set; }
        public float AttackSpeed { get; set; }
        public float Damage { get; set; }
        public float Armor { get; set; }
        public float BaseMorale { get; set; }
        public float Strength { get; set; }
        
        // Additional properties needed
        public int Level { get; set; } = 1;
        public float Experience { get; set; } = 0f;
        public float ExperienceToNextLevel { get; set; } = 100f;
        public float Morale { get; set; }
        public float Wage { get; set; }
        public float RecruitmentCost { get; set; }
        public TroopType Type { get; set; }

        public Troop(string name, float maxHealth = 100f, float attackRange = 50f, 
            float speed = 100f, float attackSpeed = 1f, float damage = 10f, 
            float armor = 10f, float baseMorale = 50f, float strength = 10f,
            float wage = 5f, float recruitmentCost = 100f, TroopType type = TroopType.Infantry)
        {
            Name = name;
            MaxHealth = maxHealth;
            AttackRange = attackRange;
            Speed = speed;
            AttackSpeed = attackSpeed;
            Damage = damage;
            Armor = armor;
            BaseMorale = baseMorale;
            Strength = strength;
            Wage = wage;
            RecruitmentCost = recruitmentCost;
            Type = type;
            Morale = baseMorale;
        }
    }
} 