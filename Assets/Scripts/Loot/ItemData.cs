using UnityEngine;

// Describes one TYPE of item (e.g. "Cereal", "Steak"), not one item in the scene.
// A ScriptableObject is a data asset you create in the Project window and edit in the
// Inspector, a bit like a JSON config file. Every Cereal box in every level points at the
// same Cereal asset, so changing its price in one place changes it everywhere.
// [CreateAssetMenu] adds it to the right-click menu: Create > Heist > Item Data.
[CreateAssetMenu(fileName = "NewItem", menuName = "Heist/Item Data")]
public class ItemData : ScriptableObject
{
    public enum ItemKind
    {
        Loot, // only worth money at the shop
        Tool  // has a usable effect (effects come in a later step)
    }

    [Tooltip("Name shown in the game, e.g. in the backpack. Leave empty to use the asset's file name.")]
    [SerializeField] private string displayName = "";

    [Tooltip("Loot is sold for money. Tools can be used for an effect.")]
    [SerializeField] private ItemKind kind = ItemKind.Loot;

    [Tooltip("How much money the shop pays for this item.")]
    [SerializeField] private int sellValue = 10;

    [Tooltip("How much money the shop charges to buy this item. Only matters for items in the Shop's Items For Sale list.")]
    [SerializeField] private int buyPrice = 25;

    [Tooltip("How many of this item fit in one slot. 1 = doesn't stack (e.g. a Cash Box).")]
    [SerializeField] private int maxStack = 5;

    [Tooltip("Color of this item in the store, so different item types are easy to tell apart.")]
    [SerializeField] private Color color = Color.green;

    [Tooltip("What happens when you use this item (Tools only). Drag an effect asset here, e.g. RedBullBuff. Leave empty for loot.")]
    [SerializeField] private ItemEffect effect;

    // The ID written into save files: the asset's file name (e.g. "Pringles").
    // Renaming an item asset changes its ID, so old saves won't find that item anymore.
    public string Id => name;

    // Read-only from code: only the Inspector should change item definitions.
    // "name" is the asset's file name (e.g. "Pringles"), used when Display Name is left empty.
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public ItemKind Kind => kind;
    public int SellValue => sellValue;

    // Mathf.Max so a negative number typed in the Inspector can't give the player money for buying.
    public int BuyPrice => Mathf.Max(0, buyPrice);

    // Mathf.Max guards against 0 or a negative number typed in the Inspector.
    public int MaxStack => Mathf.Max(1, maxStack);
    public Color Color => color;
    public ItemEffect Effect => effect;
}
