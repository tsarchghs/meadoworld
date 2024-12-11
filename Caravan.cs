using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MeadoworldMono;

public class Caravan
{
    public string Name { get; private set; }
    public Vector2 Position { get; set; }
    public Vector2 Destination { get; private set; }
    public float Speed { get; private set; }
    public Inventory Inventory { get; private set; }
    public Location HomeLocation { get; private set; }
    public Location TargetLocation { get; private set; }
    private Random _random;

    public Caravan(string name, Location home, Location target, Random random)
    {
        Name = name;
        HomeLocation = home;
        TargetLocation = target;
        Position = home.Position;
        Destination = target.Position;
        Speed = 100f; // Base speed in pixels per second
        Inventory = new Inventory();
        _random = random;
        InitializeInventory();
    }

    private void InitializeInventory()
    {
        // Add random trade goods based on home location
        foreach (var item in ItemDatabase.TradeGoods)
        {
            if (_random.NextDouble() < 0.6f)
            {
                int quantity = _random.Next(5, 20);
                Inventory.AddItem(item, quantity);
            }
        }
        Inventory.Gold = _random.Next(500, 2000);
    }

    public void Update(float deltaTime, Weather weather, Season season)
    {
        // Calculate direction to destination
        Vector2 direction = Destination - Position;
        float distance = direction.Length();
        if (distance > 0)
        {
            direction /= distance;
        }

        // Apply weather and season modifiers to speed
        float speedMultiplier = weather.GetTravelSpeedModifier() * 
                              season.GetTravelSpeedModifier();

        // Move caravan
        Position += direction * Speed * speedMultiplier * deltaTime;

        // Check if reached destination
        if (Vector2.Distance(Position, Destination) < 5f)
        {
            // Swap home and target
            var temp = HomeLocation;
            HomeLocation = TargetLocation;
            TargetLocation = temp;
            Destination = TargetLocation.Position;

            // Trade at location
            TradeAtLocation();
        }
    }

    private void TradeAtLocation()
    {
        // Simulate trading at location
        foreach (var itemEntry in TargetLocation.Inventory.Items.ToList())
        {
            var item = itemEntry.Key;
            var quantity = itemEntry.Value;

            // Buy low, sell high
            if (Inventory.GetItemPrice(item) < TargetLocation.GetItemPrice(item))
            {
                // Sell to location
                int sellAmount = Math.Min(
                    Inventory.GetItemCount(item),
                    _random.Next(1, 5)
                );
                if (sellAmount > 0)
                {
                    Inventory.RemoveItem(item, sellAmount);
                    TargetLocation.Inventory.AddItem(item, sellAmount);
                }
            }
            else
            {
                // Buy from location
                int buyAmount = Math.Min(
                    quantity,
                    _random.Next(1, 5)
                );
                if (buyAmount > 0)
                {
                    TargetLocation.Inventory.RemoveItem(item, buyAmount);
                    Inventory.AddItem(item, buyAmount);
                }
            }
        }
    }
} 