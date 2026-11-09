using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One square showing an inventory slot. Used by the hotbar along the bottom of the screen and
// by the grids on the inventory screen. It only displays things and reports clicks; it doesn't
// know which inventory it belongs to.
public class BackpackSlotUI : MonoBehaviour
{
    [Tooltip("The slot's background image. Its color changes when the slot is selected.")]
    [SerializeField] private Image background;

    [Tooltip("A square in the middle that takes the item's color. Hidden when the slot is empty.")]
    [SerializeField] private Image itemSwatch;

    [Tooltip("Small text in the corner showing the slot number (1-8). Empty for backpack slots.")]
    [SerializeField] private TMP_Text numberText;

    [Tooltip("Text under the swatch showing the item's name.")]
    [SerializeField] private TMP_Text nameText;

    [Tooltip("Small text showing how many are stacked, e.g. 'x3'. Hidden for a single item.")]
    [SerializeField] private TMP_Text countText;

    [Tooltip("The Button on this slot, so it can be clicked on the inventory screen.")]
    [SerializeField] private Button button;

    [Tooltip("Background color when this slot is NOT selected.")]
    [SerializeField] private Color normalColor = new Color(0f, 0f, 0f, 0.5f);

    [Tooltip("Background color when this slot IS selected (the hotbar slot you'd use with F).")]
    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.2f, 0.9f);

    [Tooltip("Background color when this slot has been picked up on the inventory screen, waiting for you to click where it goes.")]
    [SerializeField] private Color pickedColor = new Color(0.3f, 0.7f, 1f, 0.9f);

    // Fired when the slot is clicked, passing itself so the listener knows which one it was.
    public event Action<BackpackSlotUI> Clicked;

    private void Awake()
    {
        // AddListener is the code version of adding an entry to the Button's On Click list in the Inspector.
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        Clicked?.Invoke(this);
    }

    // Pass 0 to show no number.
    public void SetNumber(int number)
    {
        numberText.text = number > 0 ? number.ToString() : "";
    }

    public void ShowSlot(InventorySlot slot)
    {
        bool hasItem = !slot.IsEmpty;
        itemSwatch.enabled = hasItem;
        nameText.text = hasItem ? slot.Item.DisplayName : "";

        // Only show a count for real stacks; "x1" on every single item would just be clutter.
        countText.text = slot.Count > 1 ? "x" + slot.Count : "";

        if (hasItem)
        {
            itemSwatch.color = slot.Item.Color;
        }
    }

    // "Picked" wins over "selected" so you can always see what you're about to move.
    public void SetHighlight(bool selected, bool picked)
    {
        if (picked)
        {
            background.color = pickedColor;
        }
        else
        {
            background.color = selected ? selectedColor : normalColor;
        }
    }
}
