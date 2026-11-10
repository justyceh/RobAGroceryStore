using UnityEngine;

// A kicked soccer ball. It rolls and bounces with normal physics; if it hits a guard hard enough,
// the guard is shoved back and knocked down. One ball knocks down one guard, then it's just a ball.
[RequireComponent(typeof(Rigidbody))]
public class KnockdownBall : MonoBehaviour
{
    [Tooltip("How long the guard stays down, in seconds.")]
    [SerializeField] private float knockdownDuration = 3f;

    [Tooltip("How far the guard is shoved backwards, in meters.")]
    [SerializeField] private float knockbackDistance = 2f;

    [Tooltip("The ball must hit at least this fast (meters per second) to knock a guard down, so a slowly rolling ball just bumps them.")]
    [SerializeField] private float minImpactSpeed = 4f;

    [Tooltip("Seconds before the ball disappears.")]
    [SerializeField] private float lifetime = 15f;

    private bool hasKnockedSomeoneDown;

    private void Awake()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // relativeVelocity = how fast the two things were moving toward each other when they hit.
        if (hasKnockedSomeoneDown || collision.relativeVelocity.magnitude < minImpactSpeed)
        {
            return;
        }

        GuardAI guard = collision.collider.GetComponentInParent<GuardAI>();
        if (guard == null)
        {
            return;
        }

        // Shove the guard directly away from the ball, flat along the floor.
        Vector3 away = guard.transform.position - transform.position;
        away.y = 0f;
        guard.KnockDown(knockdownDuration, away.normalized * knockbackDistance);
        hasKnockedSomeoneDown = true;
    }
}
