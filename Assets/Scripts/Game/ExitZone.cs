using UnityEngine;

// An invisible box at the store's door. Walking into it with enough loot wins the game.
// Its collider must have "Is Trigger" ticked: a trigger detects things passing through
// it instead of blocking them like a wall.
[RequireComponent(typeof(Collider))]
public class ExitZone : MonoBehaviour
{
    [Tooltip("The GameManager in the scene. Drag the GameManager object here.")]
    [SerializeField] private GameManager gameManager;

    [Tooltip("How many items the player must be carrying for the exit to count as an escape.")]
    [SerializeField] private int requiredLoot = 1;

    // A physics callback: Unity calls this once, the moment another collider enters this trigger.
    // The player's CharacterController counts as a collider, so it fires when the player walks in.
    private void OnTriggerEnter(Collider other)
    {
        // Guards can walk through here too, so check it's the player. GetComponent is fine here
        // because this only runs when something enters, not every frame.
        if (!other.TryGetComponent(out PlayerLoot playerLoot))
        {
            return;
        }

        if (playerLoot.LootCount >= requiredLoot)
        {
            gameManager.PlayerEscaped(playerLoot.LootCount);
        }
    }
}
