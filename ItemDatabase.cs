using System.Collections.Generic;

namespace MeadoworldMono;

public static class ItemDatabase
{
    public static readonly Item Food = new Item("Food", ItemType.Food, 10f, 1f, "Basic food supplies");
    public static readonly Item Grain = new Item("Grain", ItemType.Food, 8f, 1f, "Raw grain");
    public static readonly Item Bread = new Item("Bread", ItemType.Food, 12f, 0.5f, "Baked bread");
    public static readonly Item Fish = new Item("Fish", ItemType.Food, 15f, 1f, "Fresh fish");
    public static readonly Item Water = new Item("Water", ItemType.Food, 5f, 2f, "Fresh water");
    
    public static readonly Item Iron = new Item("Iron", ItemType.RawMaterial, 30f, 5f, "Raw iron ore");
    public static readonly Item Wood = new Item("Wood", ItemType.RawMaterial, 15f, 3f, "Lumber");
    public static readonly Item RawMaterial = new Item("Wood", ItemType.RawMaterial, 15f, 3f, "Lumber");
public static readonly Item Herbs = new Item("Herbs", ItemType.RawMaterial, 20f, 0.5f, "Medicinal herbs");
    public static readonly Item Stone = new Item("Stone", ItemType.RawMaterial, 25f, 6f, "Raw stone");
    public static readonly Item Ore = new Item("Ore", ItemType.RawMaterial, 35f, 4f, "Raw ore");
    public static readonly List<Item> TradeGoods = new List<Item>
    {
        new Item("Cloth", ItemType.TradeGood, 20f, 2f, "Basic cloth material"),
        Iron,
        Wood,
        new Item("Spices", ItemType.TradeGood, 50f, 1f, "Exotic spices"),
        new Item("Wine", ItemType.TradeGood, 40f, 2f, "Fine wine"),
        new Item("Salt", ItemType.TradeGood, 25f, 1f, "Preserved salt")
    };

    public static readonly Dictionary<ItemType, List<Item>> ItemsByType = new Dictionary<ItemType, List<Item>>();

    static ItemDatabase()
    {
        // Initialize ItemsByType
        foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
        {
            ItemsByType[type] = new List<Item>();
        }

        // Add Food items
        ItemsByType[ItemType.Food].Add(Food);
        ItemsByType[ItemType.Food].Add(Grain);
        ItemsByType[ItemType.Food].Add(Bread);
        ItemsByType[ItemType.Food].Add(Fish);
        ItemsByType[ItemType.Food].Add(Water);

        // Add TradeGoods
        foreach (var item in TradeGoods)
        {
            ItemsByType[item.Type].Add(item);
        }
    }

    public static void InitializeLocationInventory(Location location)
    {
        switch (location.Type)
        {
            case LocationType.City:
                location.Inventory.AddItem(Food, 50);
                location.Inventory.AddItem(TradeGoods[0], 20);
                location.Inventory.AddItem(TradeGoods[1], 5);
                location.Inventory.AddItem(TradeGoods[2], 10);
                break;
            case LocationType.Village:
                location.Inventory.AddItem(Food, 20);
                location.Inventory.AddItem(TradeGoods[0], 5);
                break;
            case LocationType.Castle:
                location.Inventory.AddItem(TradeGoods[1], 10);
                location.Inventory.AddItem(TradeGoods[2], 15);
                break;
        }
    }

    public static List<Item> GetFoodItems()
    {
        return new List<Item> { Food };
    }

    public static List<Item> GetTerrainSpecificItems(string terrain)
    {
        return terrain switch
        {
            "Forest" => new List<Item> { ItemsByType[ItemType.Food][0], ItemsByType[ItemType.Food][1] },
            "Mountains" => new List<Item> { ItemsByType[ItemType.Food][2] },
            "Swamp" => new List<Item> { ItemsByType[ItemType.Food][3], ItemsByType[ItemType.Food][0] },
            "Plains" => new List<Item> { ItemsByType[ItemType.Food][1], ItemsByType[ItemType.Food][4] },
            _ => new List<Item> { Food }
        };
    }

    public static List<Item> GetAllItems()
    {
        return new List<Item> { Food };
    }
} 