using System;
using System.Collections.Generic;

// The list of items the player is carrying. This is a plain C# class, not a MonoBehaviour:
// it isn't attached to any GameObject, it's just data plus a few methods, like a plain
// TypeScript class. GameSession owns the one Backpack so it survives loading a new level.
public class Backpack
{
    private readonly List<ItemData> items = new List<ItemData>();

    public int Capacity { get; }

    // IReadOnlyList lets other scripts loop over the items without being able to add or remove them directly.
    public IReadOnlyList<ItemData> Items => items;

    public bool IsFull => items.Count >= Capacity;

    // Fired whenever an item is added or removed, so UI can redraw.
    public event Action Changed;

    public Backpack(int capacity)
    {
        Capacity = capacity;
    }

    public int TotalValue
    {
        get
        {
            int total = 0;
            foreach (ItemData item in items)
            {
                total += item.SellValue;
            }
            return total;
        }
    }

    // Returns false (and adds nothing) if the backpack is full.
    public bool TryAdd(ItemData item)
    {
        if (IsFull)
        {
            return false;
        }

        items.Add(item);
        Changed?.Invoke();
        return true;
    }

    // Removes one item of this type, if there is one.
    public void Remove(ItemData item)
    {
        if (items.Remove(item))
        {
            Changed?.Invoke();
        }
    }

    // Removes the item in a specific slot (0 = first). Later items shift down one slot.
    public void RemoveAt(int index)
    {
        if (index >= 0 && index < items.Count)
        {
            items.RemoveAt(index);
            Changed?.Invoke();
        }
    }
}
