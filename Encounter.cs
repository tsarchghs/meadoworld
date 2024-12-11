using System;
using System.Collections.Generic;

namespace MeadoworldMono;


public class Encounter
{
    public string Name { get; set; }
    public string Description { get; private set; }
    public Action<Game1> ExecuteAction { get; set; }
    public Func<Game1, bool> Condition { get; set; }

    public Encounter(string name, string description, Action<Game1> executeAction, Func<Game1, bool> condition = null)
    {
        Name = name;
        Description = description;
        ExecuteAction = executeAction;
        Condition = condition ?? (_ => true);
    }

    public void Effect(Party party, Inventory inventory)
    {
        // Implementation of encounter effects
    }

    public static List<Encounter> GetTerrainEncounters(TerrainType terrain)
    {
        return new List<Encounter>();
    }
} 