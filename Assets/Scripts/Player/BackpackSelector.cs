using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

// Chooses which hotbar slot is selected and uses the item in it.
// Keyboard: 1-8 jump to a slot, the scroll wheel cycles, F uses.
// Controller: D-pad left/right or L1/R1 (LB/RB) cycle, right trigger uses.
// Loot can't be used (it's for selling). Using a tool runs its ItemEffect and uses up one from its stack.
[RequireComponent(typeof(PlayerLoot))]
public class BackpackSelector : MonoBehaviour
{
    [Tooltip("The SelectSlot action (Player/SelectSlot), bound to the number keys 1-8.")]
    [SerializeField] private InputActionReference selectSlotAction;

    [Tooltip("The Previous action (Player/Previous): scroll up, D-pad left, L1/LB.")]
    [SerializeField] private InputActionReference previousAction;

    [Tooltip("The Next action (Player/Next): scroll down, D-pad right, R1/RB.")]
    [SerializeField] private InputActionReference nextAction;

    [Tooltip("The UseItem action (Player/UseItem): F, or the right trigger.")]
    [SerializeField] private InputActionReference useAction;

    // Which slot is selected, 0-based (slot "1" on screen is index 0), like an array index.
    public int SelectedIndex { get; private set; }

    // Fired when the selection moves, passing the new index, so the slot bar can redraw.
    public event Action<int> SelectionChanged;

    private PlayerLoot playerLoot;
    private Inventory hotbar;

    private void Awake()
    {
        playerLoot = GetComponent<PlayerLoot>();
    }

    private void OnEnable()
    {
        selectSlotAction.action.Enable();
        previousAction.action.Enable();
        nextAction.action.Enable();
        useAction.action.Enable();
    }

    private void OnDisable()
    {
        selectSlotAction.action.Disable();
        previousAction.action.Disable();
        nextAction.action.Disable();
        useAction.action.Disable();
    }

    // Start, not Awake: GameSession creates the hotbar in its own Awake.
    private void Start()
    {
        hotbar = GameSession.Instance.Hotbar;
    }

    private void Update()
    {
        if (selectSlotAction.action.WasPressedThisFrame())
        {
            // All eight number keys share one action, so ask which key did it.
            // activeControl is the control that triggered the action; for keys it's a KeyControl.
            // Key.Digit1 ... Key.Digit8 are numbered in order, so subtracting gives 0 ... 7.
            if (selectSlotAction.action.activeControl is KeyControl key)
            {
                Select(key.keyCode - Key.Digit1);
            }
        }

        if (nextAction.action.WasPressedThisFrame())
        {
            // % (remainder) wraps around: after the last slot comes the first.
            Select((SelectedIndex + 1) % hotbar.SlotCount);
        }

        if (previousAction.action.WasPressedThisFrame())
        {
            // Adding SlotCount before % keeps the result positive when going left from slot 0.
            Select((SelectedIndex - 1 + hotbar.SlotCount) % hotbar.SlotCount);
        }

        if (useAction.action.WasPressedThisFrame())
        {
            UseSelected();
        }
    }

    private void Select(int index)
    {
        if (index < 0 || index >= hotbar.SlotCount || index == SelectedIndex)
        {
            return;
        }

        SelectedIndex = index;
        SelectionChanged?.Invoke(SelectedIndex);
    }

    private void UseSelected()
    {
        // Empty slot: nothing to use.
        InventorySlot slot = hotbar.GetSlot(SelectedIndex);
        if (slot.IsEmpty)
        {
            return;
        }

        ItemData item = slot.Item;
        if (item.Kind != ItemData.ItemKind.Tool)
        {
            Debug.Log($"{item.DisplayName} is loot. Sell it at the shop instead.");
            return;
        }

        // Tools whose effect isn't built yet stay in the slot instead of being wasted.
        if (item.Effect == null)
        {
            Debug.LogWarning($"{item.DisplayName} has no Effect set on its Item Data asset yet.");
            return;
        }

        // The effect decides if it worked (e.g. a buff needs PlayerBuffs on the Player).
        // If it didn't, keep the item.
        if (!item.Effect.Use(gameObject))
        {
            return;
        }
        Debug.Log($"Used {item.DisplayName}!");

        // Takes one off the stack. The slot only empties when the last one is used.
        hotbar.RemoveOneAt(SelectedIndex);
        playerLoot.ForgetItem(item);
    }
}
