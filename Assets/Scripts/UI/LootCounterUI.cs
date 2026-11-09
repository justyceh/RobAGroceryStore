using TMPro;
using UnityEngine;

// Shows a summary of everything you're carrying (hotbar + backpack), e.g. "Backpack: 5/24   $45".
// Instead of checking every frame, it listens for both inventories' Changed events and only
// updates the text when something was added, removed or moved.
public class LootCounterUI : MonoBehaviour
{
    [Tooltip("The TextMeshPro text that displays the summary.")]
    [SerializeField] private TMP_Text label;

    [Tooltip("How the text is laid out. {0} = slots used, {1} = total slots, {2} = total value, {3} = total items counting stacks. Hotbar and backpack are added together.")]
    [SerializeField] private string format = "Backpack: {0}/{1}   ${2}";

    private Inventory hotbar;
    private Inventory backpack;

    // Start, not OnEnable: GameSession sets up the inventories in its Awake, and every Awake
    // is guaranteed to have run before any Start.
    private void Start()
    {
        hotbar = GameSession.Instance.Hotbar;
        backpack = GameSession.Instance.Backpack;
        hotbar.Changed += Refresh;
        backpack.Changed += Refresh;
        Refresh();
    }

    // The inventories outlive this level but this UI doesn't. If we didn't unsubscribe here,
    // they would keep trying to update a label that's been destroyed, a classic
    // "listener never removed" bug.
    private void OnDestroy()
    {
        if (hotbar != null)
        {
            hotbar.Changed -= Refresh;
        }
        if (backpack != null)
        {
            backpack.Changed -= Refresh;
        }
    }

    private void Refresh()
    {
        label.text = string.Format(format,
            hotbar.UsedSlotCount + backpack.UsedSlotCount,
            hotbar.SlotCount + backpack.SlotCount,
            hotbar.TotalValue + backpack.TotalValue,
            hotbar.TotalItemCount + backpack.TotalItemCount);
    }
}
