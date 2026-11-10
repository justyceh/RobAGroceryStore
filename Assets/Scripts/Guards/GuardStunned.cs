using UnityEngine;
using UnityEngine.AI;

// The guard's "Stunned" behaviour: it gets shoved back a little, tips over onto the floor,
// lies there for a few seconds, then stands back up. Used for slipping on a banana and
// being hit by a soccer ball. Like GuardSearch, GuardAI switches it on and watches IsFinished.
[RequireComponent(typeof(NavMeshAgent))]
public class GuardStunned : MonoBehaviour
{
    [Tooltip("The guard's visible model (the child object with the character mesh, e.g. Cop). It tips over when knocked down. Leave empty to skip the falling animation.")]
    [SerializeField] private Transform body;

    [Tooltip("How far the guard tips over, in degrees. 90 = flat on its back.")]
    [SerializeField] private float fallAngle = 80f;

    [Tooltip("Seconds it takes to fall over (and to get back up).")]
    [SerializeField] private float fallTime = 0.25f;

    [Tooltip("Where the guard's feet are, measured from the Guard's center. The 2 m capsule's feet are 1 m below its center, so -1. The body tips over around this point.")]
    [SerializeField] private float feetHeight = -1f;

    [Tooltip("Seconds the knockback shove lasts.")]
    [SerializeField] private float knockbackTime = 0.3f;

    public bool IsFinished { get; private set; }

    private NavMeshAgent agent;
    private Vector3 bodyStartPosition;
    private Quaternion bodyStartRotation;

    private float timeLeft;
    private float knockbackTimeLeft;
    private Vector3 knockbackVelocity;

    // 0 = standing, 1 = fully fallen over. Animated between the two.
    private float fallAmount;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (body != null)
        {
            bodyStartPosition = body.localPosition;
            bodyStartRotation = body.localRotation;
        }
    }

    // GuardAI calls this just before switching the Stunned state on.
    // "knockback" is how far (and which way) the guard is shoved, in meters.
    public void Setup(float duration, Vector3 knockback)
    {
        timeLeft = duration;
        knockbackTime = Mathf.Max(0.01f, knockbackTime);
        knockbackVelocity = knockback / knockbackTime;
    }

    private void OnEnable()
    {
        IsFinished = false;
        knockbackTimeLeft = knockbackTime;

        // isStopped freezes the agent on its path without forgetting it. A stunned guard can't walk.
        if (agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
    }

    private void OnDisable()
    {
        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }

        // Snap back to standing in case we were switched off mid-fall (e.g. knocked down again).
        fallAmount = 0f;
        ApplyFall();
    }

    private void Update()
    {
        // The shove. agent.Move slides the guard but still respects the NavMesh,
        // so it can't be knocked through a wall or off the edge of the walkable area.
        if (knockbackTimeLeft > 0f && agent.isOnNavMesh)
        {
            float step = Mathf.Min(Time.deltaTime, knockbackTimeLeft);
            agent.Move(knockbackVelocity * step);
            knockbackTimeLeft -= step;
        }

        timeLeft -= Time.deltaTime;

        // Fall over at the start, and start getting back up when there's just fallTime left,
        // so the guard is fully standing at the exact moment the stun ends.
        float target = timeLeft > fallTime ? 1f : 0f;
        fallAmount = Mathf.MoveTowards(fallAmount, target, Time.deltaTime / Mathf.Max(0.01f, fallTime));
        ApplyFall();

        if (timeLeft <= 0f)
        {
            IsFinished = true;
        }
    }

    // Tips the body backwards around the guard's feet (not its middle, or it would sink into the floor).
    // Rotating around a point = rotate the offset from that point, then add the point back.
    private void ApplyFall()
    {
        if (body == null)
        {
            return;
        }

        // A negative rotation around the X axis tips the top of the body backwards.
        Quaternion tilt = Quaternion.Euler(-fallAngle * fallAmount, 0f, 0f);
        Vector3 feet = new Vector3(0f, feetHeight, 0f);

        body.localPosition = feet + tilt * (bodyStartPosition - feet);
        body.localRotation = tilt * bodyStartRotation;
    }
}
