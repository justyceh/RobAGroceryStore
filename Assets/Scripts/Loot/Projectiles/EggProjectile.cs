using UnityEngine;

// A thrown egg. It breaks on the first thing it hits. If that's a guard (or a security
// camera, since they use GuardSight too), it gets egg in its eyes and goes blind for a while.
// A blinded guard that was chasing you loses you.
[RequireComponent(typeof(Rigidbody))]
public class EggProjectile : MonoBehaviour
{
    [Tooltip("How long the guard or camera can't see, in seconds.")]
    [SerializeField] private float blindDuration = 6f;

    [Tooltip("Seconds before the egg disappears if it somehow never hits anything (e.g. thrown out of the level).")]
    [SerializeField] private float lifetime = 10f;

    private void Awake()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GuardSight sight = collision.collider.GetComponentInParent<GuardSight>();
        if (sight != null)
        {
            sight.Blind(blindDuration);
        }

        // Eggs break on anything: floor, shelf or guard.
        Destroy(gameObject);
    }
}
