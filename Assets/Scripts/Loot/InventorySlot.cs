// One slot in an Inventory: an item type plus how many of it are stacked there,
// e.g. Cereal x3. An empty slot has no item and a count of 0.
// Only Inventory should change slots (through Set), so it can fire its Changed event;
// everything else just reads Item and Count.
public class InventorySlot
{
    public ItemData Item { get; private set; }
    public int Count { get; private set; }

    public bool IsEmpty => Item == null;

    // True if one more of this item fits on this slot's stack.
    public bool HasRoomFor(ItemData item)
    {
        return Item == item && Count < item.MaxStack;
    }

    public void Set(ItemData item, int count)
    {
        // A count of 0 (or no item) always means an empty slot, so the two can't disagree.
        if (item == null || count <= 0)
        {
            Item = null;
            Count = 0;
            return;
        }

        Item = item;
        Count = count;
    }
}
