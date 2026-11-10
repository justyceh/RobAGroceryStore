// The shop's rules: what can be sold or bought, what it costs, and saving afterwards.
// Kept separate from ShopScreenUI so the UI only deals with buttons and text, like keeping
// business logic out of a React component. A static class = a module of functions, no object needed.
// Each method returns true if it worked and always gives back a message to show the player.
public static class Shop
{
    // Sells one item (or the whole stack) from a slot in the hotbar or backpack.
    // Loot and tools can both be sold, each for its Sell Value.
    public static bool TrySell(Inventory inventory, int index, bool wholeStack, out string message)
    {
        InventorySlot slot = inventory.GetSlot(index);
        if (slot.IsEmpty)
        {
            message = "That slot is empty.";
            return false;
        }

        // Remember the item before removing it: once the stack is gone, slot.Item becomes null.
        ItemData item = slot.Item;
        int amount = wholeStack ? slot.Count : 1;
        int earned = item.SellValue * amount;

        inventory.RemoveFromSlot(index, amount);
        GameSession.Instance.AddMoney(earned);
        GameSession.Instance.SaveGame();

        message = $"Sold {amount}x {item.DisplayName} for ${earned}.";
        return true;
    }

    // Buys one of an item. It goes into the hotbar if there's room, otherwise the backpack.
    public static bool TryBuy(ItemData item, out string message)
    {
        GameSession session = GameSession.Instance;
        if (session.Money < item.BuyPrice)
        {
            message = $"Not enough money for {item.DisplayName} (costs ${item.BuyPrice}).";
            return false;
        }

        // Add the item first and only charge if it fit. TryAdd changes nothing when it returns false,
        // so a full inventory never costs you money.
        if (!session.Hotbar.TryAdd(item) && !session.Backpack.TryAdd(item))
        {
            message = "No room! Sell something or free up a slot first.";
            return false;
        }

        session.TrySpendMoney(item.BuyPrice); // Can't fail: we checked the money above.
        session.SaveGame();

        message = $"Bought {item.DisplayName} for ${item.BuyPrice}.";
        return true;
    }
}
