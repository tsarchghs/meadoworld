using System;
using System.Collections.Generic;

namespace MeadoworldMono;

public class BattleRewards
{
    public float Gold { get; private set; }
    public Dictionary<Item, int> Items { get; private set; }
    public List<Troop> Prisoners { get; private set; }

    public BattleRewards(float gold, Dictionary<Item, int> items, List<Troop> prisoners)
    {
        Gold = gold;
        Items = items;
        Prisoners = prisoners;
    }

    public void Apply(Party party, Inventory inventory)
    {
        inventory.Gold += Gold;
        foreach (var item in Items)
        {
            inventory.AddItem(item.Key, item.Value);
        }
        foreach (var prisoner in Prisoners)
        {
            party.AddTroop(prisoner);
        }
    }

    public void Claim(Party party, Inventory inventory)
    {
        Apply(party, inventory);
    }
} 