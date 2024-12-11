using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MeadoworldMono;

public enum LocationType
{
    City,
    Village,
    Castle,
    Hideout
}

public class Location
{
    public string Name { get; set; }
    public Vector2 Position { get; set; }
    public LocationType Type { get; set; }
    public Texture2D Icon { get; set; }
    public float InteractionRadius { get; set; } = 32f;
    public Inventory Inventory { get; set; } = new();
    private Dictionary<Item, float> _priceMultipliers = new Dictionary<Item, float>();
    public Faction ControllingFaction { get; set; }
    public float Prosperity { get; private set; } = 100f;

    public Location(string name, LocationType type, Vector2 position, Texture2D icon = null, float interactionRadius = 32f)
    {
        Name = name;
        Type = type;
        Position = position;
        Icon = icon;
        InteractionRadius = interactionRadius;
        Inventory = new Inventory();
        _priceMultipliers = new Dictionary<Item, float>();
        Prosperity = 100f;
    }

    public bool IsPlayerInRange(Vector2 playerPosition)
    {
        return Vector2.Distance(Position, playerPosition) <= InteractionRadius;
    }

    public int GetBuyPrice(Item item)
    {
        float priceMultiplier = _priceMultipliers.ContainsKey(item) ? _priceMultipliers[item] : 1.0f;
        return (int)(item.BasePrice * priceMultiplier * 1.2f); // 20% markup when buying
    }
    
    public int GetSellPrice(Item item)
    {
        float priceMultiplier = _priceMultipliers.ContainsKey(item) ? _priceMultipliers[item] : 1.0f;
        return (int)(item.BasePrice * priceMultiplier * 0.8f); // 20% markdown when selling
    }

    public void Improve()
    {
        Prosperity += 10f;
        // Increase inventory capacity
        // Improve trade goods variety
        // etc.
    }

    public float GetItemPrice(Item item)
    {
        float basePrice = item.BasePrice;
        if (_priceMultipliers.TryGetValue(item, out float multiplier))
        {
            basePrice *= multiplier;
        }
        return basePrice;
    }

    public void SetPriceMultiplier(Item item, float multiplier)
    {
        _priceMultipliers[item] = multiplier;
    }
} 