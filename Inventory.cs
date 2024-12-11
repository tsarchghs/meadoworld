using System.Collections.Generic;

namespace MeadoworldMono;

public class Inventory
{
    private Dictionary<Item, int> _items = new Dictionary<Item, int>();
    private Dictionary<Item, float> _itemPrices = new Dictionary<Item, float>();
    public float Gold { get; set; } = 0f;
    
    // Expose items as read-only
    public IReadOnlyDictionary<Item, int> Items => _items;

    public void AddItem(Item item, int count = 1)
    {
        if (_items.ContainsKey(item))
            _items[item] += count;
        else
            _items[item] = count;
    }

    public void RemoveItem(Item item, int count = 1)
    {
        if (!_items.ContainsKey(item)) return;

        _items[item] -= count;
        if (_items[item] <= 0)
            _items.Remove(item);
    }

    public int GetItemCount(Item item)
    {
        return _items.ContainsKey(item) ? _items[item] : 0;
    }

    public float GetItemPrice(Item item)
    {
        return _itemPrices.ContainsKey(item) ? _itemPrices[item] : item.BasePrice;
    }

    public void SetItemPrice(Item item, float price)
    {
        _itemPrices[item] = price;
    }

    public IEnumerable<KeyValuePair<Item, int>> GetAllItems()
    {
        return _items;
    }
} 