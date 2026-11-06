using UnityEngine;

// Answers one question for the guard: "Can I see the player right now?"
// For now the guard sees in every direction (360 degrees) as long as the player is in range
// and nothing solid is in between. The next step narrows this to a vision cone.
public class GuardSight : MonoBehaviour
{
    [Tooltip("The player to look for. Drag the Player from the Hierarchy here.")]
    [SerializeField] private Transform target;

    [Tooltip("How far the guard can see, in meters. Shown as a cyan circle in the Scene view when the guard is selected.")]
    [SerializeField] private float sightRange = 15f;

    [Tooltip("How high the guard's eyes are above its center, in meters. The capsule is 2 m tall, so 0.6 is about head height.")]
    [SerializeField] private float eyeHeight = 0.6f;

    [Tooltip("Layers that block sight, like walls and shelves. Leave Player, Guard and Loot unticked so they don't block the view.")]
    [SerializeField] private LayerMask obstacleLayers = 1; // 1 = only the "Default" layer

    public bool CanSeePlayer()
    {
        Vector3 eyes = transform.position + Vector3.up * eyeHeight;
        Vector3 playerCenter = target.position;

        if (Vector3.Distance(eyes, playerCenter) > sightRange)
        {
            return false;
        }

        // Draw an invisible line from the guard's eyes to the player. If it hits a wall or shelf
        // on the way, the player is hidden. Only obstacle layers are checked, so the line passes
        // through the guard's own body and the player without "hitting" them.
        bool blocked = Physics.Linecast(eyes, playerCenter, obstacleLayers, QueryTriggerInteraction.Ignore);
        return !blocked;
    }

    // Editor-only: draws the sight range when the guard is selected.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * eyeHeight, sightRange);
    }
}
