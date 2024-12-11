using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MeadoworldMono;

public class Party
{
    public Dictionary<Troop, int> Troops { get; private set; } = new Dictionary<Troop, int>();
    public Inventory Inventory { get; private set; }
    private float _morale = 50f;
    private float _foodConsumptionRate = 1f;

    public void AddTroop(Troop troop, int count = 1)
    {
        if (Troops.ContainsKey(troop))
            Troops[troop] += count;
        else
            Troops[troop] = count;
    }

    public void RemoveTroop(Troop troop, int count = 1)
    {
        if (!Troops.ContainsKey(troop)) return;
        
        Troops[troop] -= count;
        if (Troops[troop] <= 0)
            Troops.Remove(troop);
    }

    public void DismissTroop(Troop troop) => RemoveTroop(troop);

    public int GetTotalTroops() => Troops.Sum(t => t.Value);

    public float GetTotalWages() => Troops.Sum(t => t.Value * t.Key.Strength * 0.1f);

    public float GetAverageMorale() => _morale;

    public float FoodConsumptionRate => _foodConsumptionRate * GetTotalTroops();

    public void PayWages()
    {
        // Implementation depends on your economy system
        float wages = GetTotalWages();
        // Deduct wages from party's money
    }

    public void ConsumeDailyFood()
    {
        // Implementation depends on your food system
        float consumption = FoodConsumptionRate;
        // Deduct food from inventory
    }

    public float CalculateFoodSupply()
    {
        // Implementation depends on your inventory system
        return 0f; // Placeholder
    }

    public void UpdatePlayer(GameTime gameTime)
    {
        ConsumeDailyFood();
        PayWages();
        // Add any other player-specific updates that need to happen each frame
    }

    public void Update()
    {
        // Update party state, morale, etc.
        if (GetTotalTroops() > 0)
        {
            // Perform any necessary updates
        }
    }
} 