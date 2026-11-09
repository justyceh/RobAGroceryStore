using System;
using System.Collections.Generic;
using UnityEngine;

// Shows any Inventory as a grid of clickable slots. Reusable: the bottom hotbar bar and the
// backpack on the inventory screen each use one, and the shop will too. Like a React component
// that takes the inventory as a prop and reports clicks through a callback.
// The container's Grid/Horizontal Layout Group decides how the slots are arranged.
public class InventoryGridUI : MonoBehaviour
{
    [Tooltip("The slot prefab to copy for each slot (Assets/Prefabs/BackpackSlot).")]
    [SerializeField] private BackpackSlotUI slotPrefab;

    [Tooltip("The object the slots are created inside. Its layout group arranges them.")]
    [SerializeField] private Transform slotContainer;

    [Tooltip("Show 1, 2, 3... in each slot's corner. On for the hotbar, off for the backpack.")]
    [SerializeField] private bool showSlotNumbers;

    // Fired when a slot is clicked: which grid, and which slot index in it.
    public event Action<InventoryGridUI, int> SlotClicked;

    public Inventory Inventory { get; private set; }

    // The first slot's object, so the inventory screen can select it for controller navigation.
    public GameObject FirstSlotObject => slots.Count > 0 ? slots[0].gameObject : null;

    private readonly List<BackpackSlotUI> slots = new List<BackpackSlotUI>();

    // Two separate markers, so they never fight over the same slot:
    // selected = the hotbar slot you'd use with F (yellow), picked = waiting to be moved (blue).
    private int selectedIndex = -1;
    private int pickedIndex = -1;

    // Builds the slots for an inventory and keeps them up to date. Call once.
    public void Bind(Inventory inventory)
    {
        Inventory = inventory;

        for (int i = 0; i < inventory.SlotCount; i++)
        {
            BackpackSlotUI slot = Instantiate(slotPrefab, slotContainer);
            slot.SetNumber(showSlotNumbers ? i + 1 : 0);

            // Copy i into a new variable for each slot so each click handler remembers its own
            // index. Same reason you'd use "let" instead of "var" in a JS loop with callbacks.
            int index = i;
            slot.Clicked += clickedSlot => SlotClicked?.Invoke(this, index);
            slots.Add(slot);
        }

        inventory.Changed += Refresh;
        Refresh();
    }

    // Marks the currently selected slot (the hotbar uses this). Pass -1 for none.
    public void SetSelected(int index)
    {
        selectedIndex = index;
        Refresh();
    }

    // Marks the slot picked up on the inventory screen. Pass -1 for none.
    public void SetPicked(int index)
    {
        pickedIndex = index;
        Refresh();
    }

    // The inventory outlives this UI, so unsubscribe when it's destroyed.
    private void OnDestroy()
    {
        if (Inventory != null)
        {
            Inventory.Changed -= Refresh;
        }
    }

    private void Refresh()
    {
        // SetSelected can be called before Bind, depending on which Start runs first.
        // Nothing to draw yet in that case; Bind will call Refresh itself.
        if (Inventory == null)
        {
            return;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].ShowSlot(Inventory.GetSlot(i));
            slots[i].SetHighlight(i == selectedIndex, i == pickedIndex);
        }
    }
}
