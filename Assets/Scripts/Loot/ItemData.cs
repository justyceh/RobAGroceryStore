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

    [Tooltip("Color of this item in the store, so different item types are easy to tell apart.")]
    [SerializeField] private Color color = Color.green;

    // Read-only from code: only the Inspector should change item definitions.
    // "name" is the asset's file name (e.g. "Pringles"), used when Display Name is left empty.
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public ItemKind Kind => kind;
    public int SellValue => sellValue;
    public Color Color => color;
}
