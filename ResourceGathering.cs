using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MeadoworldMono;

public static class ResourceGathering
{
    private static readonly Random random = new();

    public static (Item item, int quantity) Hunt(Vector2 position, Weather weather)
    {
        var terrain = GetTerrainType(position);
        float baseChance = GetTerrainHuntingChance(terrain);
        float weatherMod = weather.GetGatheringModifier();
        
        float successChance = baseChance * weatherMod;
        
        if (random.NextDouble() > successChance)
            return (null, 0);

        // Get terrain-specific items
        var possibleItems = ItemDatabase.GetTerrainSpecificItems(terrain)
            .Where(i => i.Type == ItemType.Food)
            .ToList();
        
        if (possibleItems.Count == 0)
            return (null, 0);

        var item = possibleItems[random.Next(possibleItems.Count)];
        return (item, random.Next(1, 4));
    }

    public static (Item item, int quantity) Forage(Vector2 position, Weather weather)
    {
        var terrain = GetTerrainType(position);
        float baseChance = GetTerrainForagingChance(terrain);
        float weatherMod = weather.GetGatheringModifier();
        
        float successChance = baseChance * weatherMod;
        
        if (random.NextDouble() > successChance)
            return (null, 0);

        // Terrain-specific foraging results
        return terrain switch
        {
            "Forest" => random.NextDouble() < 0.7f
                ? (ItemDatabase.Grain, random.Next(2, 4))
                : (ItemDatabase.Bread, random.Next(1, 3)),
            "Plains" => (ItemDatabase.Grain, random.Next(1, 4)),
            _ => (ItemDatabase.Grain, 1)
        };
    }

    public static string GetTerrainType(Vector2 position)
    {
        // Simple terrain generation based on position
        if (position.Y < 200) return "Mountains";
        if (position.X < 300) return "Forest";
        if (position.Y > 800) return "Swamp";
        return "Plains";
    }

    private static float GetTerrainHuntingChance(string terrain)
    {
        return terrain switch
        {
            "Forest" => 0.6f,
            "Plains" => 0.4f,
            "Mountains" => 0.3f,
            _ => 0.2f
        };
    }

    private static float GetTerrainForagingChance(string terrain)
    {
        return terrain switch
        {
            "Forest" => 0.7f,
            "Plains" => 0.5f,
            "Mountains" => 0.2f,
            _ => 0.3f
        };
    }

    public static Item GatherResource(string terrainType)
    {
        var random = new Random();
        float chance = terrainType switch
        {
            "Forest" => 0.7f,
            "Plains" => 0.5f,
            "Mountains" => 0.3f,
            "Swamp" => 0.4f,
            _ => 0.2f
        };

        if (random.NextDouble() > chance)
            return null;

        // Return terrain-specific resources
        return terrainType switch
        {
            "Forest" => random.NextDouble() < 0.6f ? ItemDatabase.Wood : ItemDatabase.Herbs,
            "Plains" => random.NextDouble() < 0.7f ? ItemDatabase.Grain : ItemDatabase.Herbs,
            "Mountains" => random.NextDouble() < 0.8f ? ItemDatabase.Stone : ItemDatabase.Ore,
            "Swamp" => random.NextDouble() < 0.5f ? ItemDatabase.Herbs : ItemDatabase.Wood,
            _ => ItemDatabase.Herbs
        };
    }
} 