namespace MeadoworldMono;

public enum ItemType
{
    Food,
    RawMaterial,
    TradeGood,
    Equipment,
    Weapon,
    Armor
}

public class Item
{
    public string Name { get; set; }
    public ItemType Type { get; set; }
    public float BasePrice { get; set; }
    public float Weight { get; set; }
    public string Description { get; set; }

    public Item(string name, ItemType type, float basePrice, float weight = 1f, string description = "")
    {
        Name = name;
        Type = type;
        BasePrice = basePrice;
        Weight = weight;
        Description = description;
    }
} 