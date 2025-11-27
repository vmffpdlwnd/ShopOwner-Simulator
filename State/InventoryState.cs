// State/InventoryState.cs
using ShopOwnerSimulator.Models.Entities;

namespace ShopOwnerSimulator.State;

public class InventoryState
{
    public event Action? OnInventoryChanged;

    public List<InventoryItem> Items { get; private set; } = new();
    public int TotalItems => Items.Sum(i => i.Quantity);

    public void UpdateInventory(List<InventoryItem> items)
    {
        Items = new List<InventoryItem>(items);
        OnInventoryChanged?.Invoke();
    }

    public void AddItem(InventoryItem item)
    {
        Items.Add(item);
        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(string itemId)
    {
        Items.RemoveAll(i => i.Id == itemId);
        OnInventoryChanged?.Invoke();
    }

    public InventoryItem? GetItem(string itemId)
    {
        return Items.FirstOrDefault(i => i.Id == itemId);
    }

    public void Clear()
    {
        Items.Clear();
        OnInventoryChanged?.Invoke();
    }
}