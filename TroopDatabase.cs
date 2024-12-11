using System.Collections.Generic;

namespace MeadoworldMono;

public static class TroopDatabase
{
    public static readonly Troop Infantry = new Troop(
        "Infantry",
        maxHealth: 100f,
        attackRange: 2f,
        speed: 5f,
        attackSpeed: 1f,
        damage: 10f,
        armor: 5f,
        baseMorale: 50f,
        strength: 8f,
        wage: 2f,
        recruitmentCost: 100f,
        type: TroopType.Infantry
    );

    public static readonly Troop Archer = new Troop(
        "Archer",
        maxHealth: 80f,
        attackRange: 20f,
        speed: 4f,
        attackSpeed: 0.8f,
        damage: 15f,
        armor: 3f,
        baseMorale: 45f,
        strength: 6f,
        wage: 3f,
        recruitmentCost: 150f,
        type: TroopType.Archer
    );

    public static readonly Troop Cavalry = new Troop(
        "Cavalry",
        maxHealth: 120f,
        attackRange: 3f,
        speed: 8f,
        attackSpeed: 1.2f,
        damage: 12f,
        armor: 6f,
        baseMorale: 60f,
        strength: 10f,
        wage: 5f,
        recruitmentCost: 200f,
        type: TroopType.Cavalry
    );

    public static List<Troop> GetAvailableTroops(LocationType locationType)
    {
        return locationType switch
        {
            LocationType.City => new List<Troop> { Infantry, Archer, Cavalry },
            LocationType.Castle => new List<Troop> { Infantry, Archer },
            LocationType.Village => new List<Troop> { Infantry },
            _ => new List<Troop>()
        };
    }

    public static Troop Recruit(TroopType type)
    {
        return type switch
        {
            TroopType.Infantry => Infantry,
            TroopType.Archer => Archer,
            TroopType.Cavalry => Cavalry,
            _ => null
        };
    }

    public static Troop GetTroop(TroopType type)
    {
        return type switch
        {
            TroopType.Infantry => Infantry,
            TroopType.Archer => Archer,
            TroopType.Cavalry => Cavalry,
            _ => null
        };
    }
} 