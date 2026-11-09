using UnityEngine;

// The hotbar row along the bottom of the screen.
// The InventoryGridUI on the same object does the real work (making the slots, showing items,
// reporting clicks). This script just connects it to the hotbar and keeps the yellow
// "selected slot" marker in sync with the BackpackSelector.
// Because it's a normal InventoryGridUI, the inventory screen can use this same bar
// for moving items in and out, so there's only ever one hotbar on screen.
public class BackpackBarUI : MonoBehaviour
{
    [Tooltip("The BackpackSelector on the Player. Drag the Player here.")]
    [SerializeField] private BackpackSelector selector;

    [Tooltip("The InventoryGridUI that draws the hotbar slots. Usually on this same object.")]
    [SerializeField] private InventoryGridUI grid;

    private void Start()
    {
        grid.Bind(GameSession.Instance.Hotbar);
        selector.SelectionChanged += OnSelectionChanged;
        grid.SetSelected(selector.SelectedIndex);
    }

    // The selector lives on the Player, which may be destroyed at a different moment than
    // this UI, so always unsubscribe.
    private void OnDestroy()
    {
        if (selector != null)
        {
            selector.SelectionChanged -= OnSelectionChanged;
        }
    }

    private void OnSelectionChanged(int index)
    {
        grid.SetSelected(index);
    }
}
