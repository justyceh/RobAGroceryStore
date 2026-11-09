using UnityEngine;

// Answers one question for the guard: "Can I see the player right now?"
// The guard sees in a cone in front of it: the player must be within range, inside the
// viewing angle, and not hidden behind a wall or shelf. Sneaking up from behind is safe.
public class GuardSight : MonoBehaviour
{
    [Tooltip("How far the guard can see, in meters.")]
    [SerializeField] private float sightRange = 15f;

    [Tooltip("Total width of the vision cone in degrees, e.g. 100 = 50 degrees to each side of straight ahead.")]
    [SerializeField] private float viewAngle = 100f;

    [Tooltip("How high the guard's eyes are above its center, in meters. The capsule is 2 m tall, so 0.6 is about head height.")]
    [SerializeField] private float eyeHeight = 0.6f;

    [Tooltip("Layers that block sight, like walls and shelves. Leave Player, Guard and Loot unticked so they don't block the view.")]
    [SerializeField] private LayerMask obstacleLayers = 1; // 1 = only the "Default" layer

    // Read-only access for GuardVisionCone, so the drawn cone always matches what the guard really sees.
    public float SightRange => sightRange;
    public float ViewAngle => viewAngle;
    public LayerMask ObstacleLayers => obstacleLayers;
    public Vector3 EyePosition => transform.position + Vector3.up * eyeHeight;

    // The player to look for. GuardAI hands this over in its Awake.
    private Transform target;

    public void Init(Transform player)
    {
        target = player;
    }

    public bool CanSeePlayer()
    {
        Vector3 eyes = EyePosition;
        Vector3 toPlayer = target.position - eyes;

        // 1. Range
        if (toPlayer.magnitude > sightRange)
        {
            return false;
        }

        // 2. Angle. Flatten to the ground first so jumping doesn't change whether you're "in front".
        // Vector3.Angle gives the angle between where the guard faces and where the player is.
        Vector3 flatToPlayer = new Vector3(toPlayer.x, 0f, toPlayer.z);
        if (Vector3.Angle(transform.forward, flatToPlayer) > viewAngle / 2f)
        {
            return false;
        }

        // 3. Line of sight: if the line from the eyes to the player hits a wall or shelf, the player is hidden.
        if (Physics.Linecast(eyes, target.position, out RaycastHit hit, obstacleLayers, QueryTriggerInteraction.Ignore))
        {
            // Character models often come with their own colliders (hat, belt...). If the first thing
            // the line hits is part of the player, nothing is in the way, so the player is visible.
            // IsChildOf is also true for the player object itself.
            return hit.transform.IsChildOf(target);
        }
        return true;
    }

    // Editor-only: draws the cone's two edges in cyan when the guard is selected.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 eyes = EyePosition;
        Vector3 leftEdge = Quaternion.Euler(0f, -viewAngle / 2f, 0f) * transform.forward;
        Vector3 rightEdge = Quaternion.Euler(0f, viewAngle / 2f, 0f) * transform.forward;
        Gizmos.DrawLine(eyes, eyes + leftEdge * sightRange);
        Gizmos.DrawLine(eyes, eyes + rightEdge * sightRange);
    }
}
