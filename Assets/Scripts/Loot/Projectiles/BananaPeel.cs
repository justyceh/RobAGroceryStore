using UnityEngine;

// A thrown banana peel. It flies and bounces like a normal object until it lands on a floor
// (or shelf top), then lies flat and waits as a trap. The first guard to step on it slips
// and falls over. Hitting a guard directly while flying also makes them slip.
[RequireComponent(typeof(Rigidbody))]
public class BananaPeel : MonoBehaviour
{
    [Tooltip("How long the guard lies on the floor after slipping, in seconds.")]
    [SerializeField] private float slipDuration = 2.5f;

    [Tooltip("How far the guard slides forward when it slips, in meters.")]
    [SerializeField] private float slideDistance = 1.5f;

    [Tooltip("Seconds before an unused peel disappears, so the store doesn't fill up with them.")]
    [SerializeField] private float lifetime = 60f;

    private Rigidbody body;
    private Collider peelCollider;
    private bool hasLanded;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        peelCollider = GetComponent<Collider>();

        // Destroy with a delay: Unity removes it automatically after "lifetime" seconds, like setTimeout.
        Destroy(gameObject, lifetime);
    }

    // Physics callback: runs when this solid collider bumps into another one (while flying).
    private void OnCollisionEnter(Collision collision)
    {
        if (TrySlip(collision.collider) || hasLanded)
        {
            return;
        }

        // The contact "normal" points away from the surface we hit. Pointing mostly up (y > 0.5)
        // means we landed on top of something (floor, shelf top), not against a wall.
        if (collision.GetContact(0).normal.y > 0.5f)
        {
            Land();
        }
    }

    // Physics callback: runs when something walks into this trigger (after landing).
    private void OnTriggerEnter(Collider other)
    {
        TrySlip(other);
    }

    private void Land()
    {
        hasLanded = true;

        // Kinematic = physics stops moving it, so it stays exactly where it fell.
        // Trigger = guards walk through it instead of bumping into it, but we still get OnTriggerEnter.
        body.isKinematic = true;
        peelCollider.isTrigger = true;

        // Lie flat, keeping only the left/right facing.
        transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
    }

    private bool TrySlip(Collider other)
    {
        // The guard's collider might be on a child object (like the model), so search upward for GuardAI.
        GuardAI guard = other.GetComponentInParent<GuardAI>();
        if (guard == null)
        {
            return false;
        }

        guard.KnockDown(slipDuration, guard.transform.forward * slideDistance);
        Destroy(gameObject);
        return true;
    }
}
