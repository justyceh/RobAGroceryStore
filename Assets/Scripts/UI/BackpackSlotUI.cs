using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One square in the slot bar. BackpackBarUI creates eight of these from a prefab and
// tells each one what to show. It only displays things; it doesn't know about input.
public class BackpackSlotUI : MonoBehaviour
{
    [Tooltip("The slot's background image. Its color changes when the slot is selected.")]
    [SerializeField] private Image background;

    [Tooltip("A square in the middle that takes the item's color. Hidden when the slot is empty.")]
    [SerializeField] private Image itemSwatch;

    [Tooltip("Small text in the corner showing the slot number (1-8).")]
    [SerializeField] private TMP_Text numberText;

    [Tooltip("Text under the swatch showing the item's name.")]
    [SerializeField] private TMP_Text nameText;

    [Tooltip("Background color when this slot is NOT selected.")]
    [SerializeField] private Color normalColor = new Color(0f, 0f, 0f, 0.5f);

    [Tooltip("Background color when this slot IS selected.")]
    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.2f, 0.9f);

    public void SetNumber(int number)
    {
        numberText.text = number.ToString();
    }

    // Pass null for an empty slot.
    public void ShowItem(ItemData item)
    {
        bool hasItem = item != null;
        itemSwatch.enabled = hasItem;
        nameText.text = hasItem ? item.DisplayName : "";

        if (hasItem)
        {
            itemSwatch.color = item.Color;
        }
    }

    public void SetSelected(bool selected)
    {
        background.color = selected ? selectedColor : normalColor;
    }
}
