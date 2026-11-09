using System.Collections.Generic;
using UnityEngine;

// A list of every item type in the game, so a save file can store just item IDs
// ("Pringles", "Steak") and turn them back into the real ItemData assets when loading.
// Like a lookup table from product ID to product.
// Create it with: right-click > Create > Heist > Item Database.
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Heist/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [Tooltip("Every ItemData in the game. An item missing from this list can't be loaded from a save, so add new items here.")]
    [SerializeField] private List<ItemData> items = new List<ItemData>();

    // Returns null if no item has this ID.
    public ItemData FindById(string id)
    {
        foreach (ItemData item in items)
        {
            if (item != null && item.Id == id)
            {
                return item;
            }
        }
        return null;
    }
}
