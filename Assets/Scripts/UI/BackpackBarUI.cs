using System.Collections.Generic;
using UnityEngine;

// The row of backpack slots along the bottom of the screen.
// It makes one slot per backpack space from a prefab (like rendering a list with .map() in React),
// then redraws whenever the backpack's contents or the selected slot change.
public class BackpackBarUI : MonoBehaviour
{
    [Tooltip("The BackpackSelector on the Player. Drag the Player here.")]
    [SerializeField] private BackpackSelector selector;

    [Tooltip("The slot prefab to copy for each backpack space (Assets/Prefabs/BackpackSlot).")]
    [SerializeField] private BackpackSlotUI slotPrefab;

    [Tooltip("The object the slots are created inside. Its Horizontal Layout Group lines them up in a row.")]
    [SerializeField] private Transform slotContainer;

    private readonly List<BackpackSlotUI> slots = new List<BackpackSlotUI>();
    private Backpack backpack;

    private void Start()
    {
        backpack = GameSession.Instance.Backpack;

        // Instantiate makes a copy of the prefab at runtime. Passing the container makes each
        // copy its child, so the layout group arranges them automatically.
        for (int i = 0; i < backpack.Capacity; i++)
        {
            BackpackSlotUI slot = Instantiate(slotPrefab, slotContainer);
            slot.SetNumber(i + 1);
            slots.Add(slot);
        }

        backpack.Changed += Refresh;
        selector.SelectionChanged += OnSelectionChanged;
        Refresh();
    }

    // The backpack outlives this level, so always unsubscribe from it when this UI is destroyed.
    private void OnDestroy()
    {
        if (backpack != null)
        {
            backpack.Changed -= Refresh;
        }
        if (selector != null)
        {
            selector.SelectionChanged -= OnSelectionChanged;
        }
    }

    private void OnSelectionChanged(int index)
    {
        Refresh();
    }

    private void Refresh()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            ItemData item = i < backpack.Items.Count ? backpack.Items[i] : null;
            slots[i].ShowItem(item);
            slots[i].SetSelected(i == selector.SelectedIndex);
        }
    }
}
