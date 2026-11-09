using TMPro;
using UnityEngine;

// Shows what's in the backpack on screen, e.g. "Backpack: 3/8   $45".
// Instead of checking every frame, it listens for the backpack's Changed event and only
// updates the text when something was added or removed.
public class LootCounterUI : MonoBehaviour
{
    [Tooltip("The TextMeshPro text that displays the backpack.")]
    [SerializeField] private TMP_Text label;

    [Tooltip("How the text is laid out. {0} = items carried, {1} = backpack size, {2} = total value.")]
    [SerializeField] private string format = "Backpack: {0}/{1}   ${2}";

    private Backpack backpack;

    // Start, not OnEnable: GameSession sets up the backpack in its Awake, and every Awake
    // is guaranteed to have run before any Start.
    private void Start()
    {
        backpack = GameSession.Instance.Backpack;
        backpack.Changed += Refresh;
        Refresh();
    }

    // The backpack outlives this level but this UI doesn't. If we didn't unsubscribe here,
    // the backpack would keep trying to update a label that's been destroyed, a classic
    // "listener never removed" bug.
    private void OnDestroy()
    {
        if (backpack != null)
        {
            backpack.Changed -= Refresh;
        }
    }

    private void Refresh()
    {
        label.text = string.Format(format, backpack.Items.Count, backpack.Capacity, backpack.TotalValue);
    }
}
