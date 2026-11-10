using System;
using UnityEngine;

// A fixed row of slots where items stack, e.g. [Cereal x3] [empty] [Red Bull x2] ...
// Slot positions matter (slot 3 stays slot 3), which the hotbar and, later, swapping need.
// This is a plain C# class, not a MonoBehaviour: just data plus methods, like a TypeScript class.
// GameSession owns two of these, the hotbar and the backpack, so they survive loading a new level.
public class Inventory
{
    private readonly InventorySlot[] slots;

    public int SlotCount => slots.Length;

    // Fired whenever anything in any slot changes, so UI can redraw.
    public event Action Changed;

    public Inventory(int slotCount)
    {
        slots = new InventorySlot[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            slots[i] = new InventorySlot();
        }
    }

    public InventorySlot GetSlot(int index)
    {
        return slots[index];
    }

    public int UsedSlotCount
    {
        get
        {
            int used = 0;
            foreach (InventorySlot slot in slots)
            {
                if (!slot.IsEmpty)
                {
                    used++;
                }
            }
            return used;
        }
    }

    // Total number of items, counting every item in every stack.
    public int TotalItemCount
    {
        get
        {
            int total = 0;
            foreach (InventorySlot slot in slots)
            {
                total += slot.Count;
            }
            return total;
        }
    }

    public int TotalValue
    {
        get
        {
            int total = 0;
            foreach (InventorySlot slot in slots)
            {
                if (!slot.IsEmpty)
                {
                    total += slot.Item.SellValue * slot.Count;
                }
            }
            return total;
        }
    }

    // Adds one item: onto an existing stack of the same item if one has room,
    // otherwise into the first empty slot. Returns false (and adds nothing) if neither exists.
    public bool TryAdd(ItemData item)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.HasRoomFor(item))
            {
                slot.Set(item, slot.Count + 1);
                Changed?.Invoke();
                return true;
            }
        }

        foreach (InventorySlot slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.Set(item, 1);
                Changed?.Invoke();
                return true;
            }
        }

        return false;
    }

    // Removes one of this item from wherever it is, starting from the last slot.
    // Returns false if there wasn't one.
    public bool RemoveOne(ItemData item)
    {
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            if (slots[i].Item == item)
            {
                RemoveOneAt(i);
                return true;
            }
        }
        return false;
    }

    // Removes one item from a specific slot's stack (e.g. using one Red Bull out of three).
    public void RemoveOneAt(int index)
    {
        RemoveFromSlot(index, 1);
    }

    // Removes some items from a specific slot's stack (e.g. selling 3 Pringles at the shop).
    // Asking for more than the stack has just empties the slot.
    public void RemoveFromSlot(int index, int amount)
    {
        InventorySlot slot = slots[index];
        if (slot.IsEmpty || amount <= 0)
        {
            return;
        }

        slot.Set(slot.Item, slot.Count - amount);
        Changed?.Invoke();
    }

    // Puts a whole stack straight into a slot. Used when loading a save.
    public void SetSlot(int index, ItemData item, int count)
    {
        slots[index].Set(item, count);
        Changed?.Invoke();
    }

    // Moves the stack in one slot onto another slot, which can be in a different inventory
    // (e.g. backpack slot 5 -> hotbar slot 2):
    // - same item on both: as many as fit are merged onto the target stack,
    // - otherwise the two slots simply swap (an empty slot swaps too, which works as a move).
    // "static" because it works on two inventories rather than belonging to one of them.
    public static void MoveOrSwap(Inventory from, int fromIndex, Inventory to, int toIndex)
    {
        InventorySlot source = from.slots[fromIndex];
        InventorySlot target = to.slots[toIndex];
        if (source == target || (source.IsEmpty && target.IsEmpty))
        {
            return;
        }

        bool merged = false;
        if (!source.IsEmpty && source.Item == target.Item)
        {
            int room = target.Item.MaxStack - target.Count;
            int amount = Mathf.Min(room, source.Count);
            if (amount > 0)
            {
                target.Set(target.Item, target.Count + amount);
                source.Set(source.Item, source.Count - amount);
                merged = true;
            }
        }

        if (!merged)
        {
            ItemData item = source.Item;
            int count = source.Count;
            source.Set(target.Item, target.Count);
            target.Set(item, count);
        }

        // Events can only be fired from inside the class that declares them, which is why this lives here.
        from.Changed?.Invoke();
        if (to != from)
        {
            to.Changed?.Invoke();
        }
    }
}
