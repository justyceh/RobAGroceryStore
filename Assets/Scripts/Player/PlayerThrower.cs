using UnityEngine;

// Launches thrown or kicked items (banana, egg, soccer ball) as real physics objects.
// It spawns the item's prefab and gives its Rigidbody a starting velocity; after that,
// Unity's physics engine handles gravity, which is what makes the curved "throw arc".
//
// Throws go where the camera aims (CameraTarget tilts up/down with the mouse), so you can lob
// a banana far by looking up. Kicks start at your feet and go straight ahead along the floor.
public class PlayerThrower : MonoBehaviour
{
    [Tooltip("The head-height CameraTarget child of the Player (the same one PlayerLook tilts). Throws start here and fly where it points.")]
    [SerializeField] private Transform cameraTarget;

    [Tooltip("Where a thrown item appears, relative to CameraTarget: a little right (x), down (y) and in front (z), like it's leaving your hand.")]
    [SerializeField] private Vector3 throwOffset = new Vector3(0.25f, -0.2f, 0.7f);

    [Tooltip("Where a kicked item appears, relative to the Player's center: low (y, the feet are at -1) and in front (z).")]
    [SerializeField] private Vector3 kickOffset = new Vector3(0f, -0.7f, 0.8f);

    // Every collider on the player (the CharacterController plus any on the model).
    // Thrown items ignore these so they don't bounce off your own body the moment they spawn.
    private Collider[] playerColliders;

    private void Awake()
    {
        // "true" also includes colliders on inactive children.
        playerColliders = GetComponentsInChildren<Collider>(true);
    }

    // Spawns the prefab and sends it flying. Returns false (so the item isn't used up) if it can't.
    // forwardSpeed / upwardSpeed are in meters per second.
    public bool Launch(GameObject prefab, float forwardSpeed, float upwardSpeed, bool kick)
    {
        if (cameraTarget == null)
        {
            Debug.LogError("PlayerThrower has no Camera Target set.");
            return false;
        }

        // TransformPoint turns an offset in local space ("0.7 m in front of my face")
        // into a world position, taking the current rotation into account.
        Vector3 origin = kick ? transform.TransformPoint(kickOffset) : cameraTarget.TransformPoint(throwOffset);
        Vector3 direction = kick ? transform.forward : cameraTarget.forward;

        // Instantiate = spawn a copy of a prefab into the scene, like creating a new DOM element from a template.
        GameObject projectile = Instantiate(prefab, origin, Quaternion.LookRotation(direction));

        if (!projectile.TryGetComponent(out Rigidbody body))
        {
            Debug.LogError($"{prefab.name} needs a Rigidbody to be thrown.");
            Destroy(projectile);
            return false;
        }

        foreach (Collider projectileCollider in projectile.GetComponentsInChildren<Collider>())
        {
            foreach (Collider playerCollider in playerColliders)
            {
                Physics.IgnoreCollision(projectileCollider, playerCollider);
            }
        }

        // linearVelocity is Unity 6's name for what older tutorials call "velocity".
        body.linearVelocity = direction * forwardSpeed + Vector3.up * upwardSpeed;
        return true;
    }
}
