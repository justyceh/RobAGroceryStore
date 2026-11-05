using TMPro;
using UnityEngine;

// Shows the player's loot count on screen, e.g. "Loot: 3".
// Instead of checking the count every frame, it listens for PlayerLoot's LootChanged event
// and only updates the text when something was actually stolen.
public class LootCounterUI : MonoBehaviour
{
    [Tooltip("The PlayerLoot component on the Player. Drag the Player object here.")]
    [SerializeField] private PlayerLoot playerLoot;

    [Tooltip("The TextMeshPro text that displays the count.")]
    [SerializeField] private TMP_Text label;

    [Tooltip("Text shown before the number.")]
    [SerializeField] private string prefix = "Loot: ";

    // Subscribe when enabled and unsubscribe when disabled, like adding an event listener
    // in a useEffect and removing it in the cleanup. Forgetting to unsubscribe can cause
    // errors if this UI is destroyed while the player still exists.
    private void OnEnable()
    {
        playerLoot.LootChanged += UpdateLabel;
    }

    private void OnDisable()
    {
        playerLoot.LootChanged -= UpdateLabel;
    }

    private void Start()
    {
        // Show the starting count (0) right away instead of the placeholder text.
        UpdateLabel(playerLoot.LootCount);
    }

    private void UpdateLabel(int count)
    {
        label.text = prefix + count;
    }
}
