using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// The inventory screen opened with Tab (or the controller's Select/View button).
// It shows the backpack; the normal hotbar bar at the bottom of the screen stays visible on top
// and becomes clickable. Click a slot to pick it up, then click another slot (backpack or hotbar)
// to move or swap it there. Same items stack together when there's room.
// The game keeps running while it's open (guards still move!), but looking around,
// stealing and using items are paused so the mouse can be used for clicking.
public class InventoryScreenUI : MonoBehaviour
{
    [Tooltip("The ToggleInventory action (Player/ToggleInventory), bound to Tab and the controller's Select/View button.")]
    [SerializeField] private InputActionReference toggleAction;

    [Tooltip("The panel to show and hide (InventoryScreen).")]
    [SerializeField] private GameObject screen;

    [Tooltip("The grid that shows the backpack.")]
    [SerializeField] private InventoryGridUI backpackGrid;

    [Tooltip("The hotbar bar at the bottom of the screen (the object with BackpackBarUI and InventoryGridUI). It binds itself; this screen only listens to its clicks.")]
    [SerializeField] private InventoryGridUI hotbarGrid;

    [Tooltip("The GameManager, so the screen can't be opened after the game ends.")]
    [SerializeField] private GameManager gameManager;

    [Tooltip("These player scripts are switched off while the screen is open. Drag the Player into each slot.")]
    [SerializeField] private PlayerLook playerLook;
    [SerializeField] private PlayerInteract playerInteract;
    [SerializeField] private BackpackSelector backpackSelector;

    // The slot picked up with the first click, waiting for the second click. null = nothing picked.
    private InventoryGridUI pickedGrid;
    private int pickedIndex = -1;

    private bool IsOpen => screen.activeSelf;

    private void OnEnable()
    {
        toggleAction.action.Enable();
    }

    private void OnDisable()
    {
        toggleAction.action.Disable();
    }

    private void Start()
    {
        // Only the backpack is bound here. The hotbar grid is the bottom bar, which
        // BackpackBarUI already binds; binding it twice would create a second row of slots.
        backpackGrid.Bind(GameSession.Instance.Backpack);
        backpackGrid.SlotClicked += OnSlotClicked;
        hotbarGrid.SlotClicked += OnSlotClicked;
        screen.SetActive(false);
    }

    private void Update()
    {
        // After a win or loss the player's scripts are switched off for good,
        // so never open (and never re-enable them by closing).
        if (gameManager.IsGameOver)
        {
            if (IsOpen)
            {
                ClearPick();
                screen.SetActive(false);
            }
            return;
        }

        if (toggleAction.action.WasPressedThisFrame())
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }
    }

    private void Open()
    {
        screen.SetActive(true);
        SetPlayerControls(false);

        // Give a controller something to start from: it moves between slots with the D-pad/stick
        // and "clicks" with the A / Cross button.
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(hotbarGrid.FirstSlotObject);
        }
    }

    private void Close()
    {
        ClearPick();
        screen.SetActive(false);
        SetPlayerControls(true);
        ClearUISelection();
    }

    // The bottom bar's slots are buttons that stay on screen during gameplay. If a controller left
    // one selected, pressing A (which is also Jump) would "click" it. Clearing the selection stops that.
    private void ClearUISelection()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void SetPlayerControls(bool on)
    {
        // Switching PlayerLook off also unlocks the cursor (its OnDisable), and back on locks it again.
        playerLook.enabled = on;
        playerInteract.enabled = on;
        backpackSelector.enabled = on;
    }

    private void OnSlotClicked(InventoryGridUI grid, int index)
    {
        // The bottom bar is clickable all the time, but rearranging is only allowed with the screen open.
        if (!IsOpen)
        {
            return;
        }

        // First click: pick up a slot (an empty slot has nothing to pick up).
        if (pickedGrid == null)
        {
            if (grid.Inventory.GetSlot(index).IsEmpty)
            {
                return;
            }

            pickedGrid = grid;
            pickedIndex = index;
            grid.SetPicked(index);
            return;
        }

        // Second click: move or swap there. Clicking the same slot again just cancels.
        if (grid != pickedGrid || index != pickedIndex)
        {
            Inventory.MoveOrSwap(pickedGrid.Inventory, pickedIndex, grid.Inventory, index);
        }
        ClearPick();
    }

    private void ClearPick()
    {
        if (pickedGrid != null)
        {
            pickedGrid.SetPicked(-1);
        }
        pickedGrid = null;
        pickedIndex = -1;
    }
}
