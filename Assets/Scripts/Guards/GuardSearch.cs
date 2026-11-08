using UnityEngine;
using UnityEngine.AI;

// After losing sight of the player, the guard walks to where it last saw them and slowly
// turns on the spot so its vision cone sweeps the area. GuardAI gives it the point to search,
// switches this on, and goes back to wandering once IsFinished becomes true.
[RequireComponent(typeof(NavMeshAgent))]
public class GuardSearch : MonoBehaviour
{
    [Tooltip("Walking speed on the way to the last known position, in meters per second.")]
    [SerializeField] private float searchSpeed = 3.5f;

    [Tooltip("How long the guard looks around after arriving, in seconds.")]
    [SerializeField] private float lookAroundTime = 4f;

    [Tooltip("How fast the guard turns while looking around, in degrees per second. 90 = a full circle in 4 seconds.")]
    [SerializeField] private float turnSpeed = 90f;

    public bool IsFinished { get; private set; }

    private NavMeshAgent agent;
    private Vector3 searchPoint;
    private bool hasArrived;
    private float lookTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // GuardAI calls this just before switching the Search state on.
    public void SetSearchPoint(Vector3 point)
    {
        searchPoint = point;
    }

    private void OnEnable()
    {
        agent.speed = searchSpeed;
        hasArrived = false;
        IsFinished = false;
        lookTimer = 0f;

        if (agent.isOnNavMesh)
        {
            agent.SetDestination(searchPoint);
        }
    }

    private void OnDisable()
    {
        if (agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
    }

    private void Update()
    {
        if (IsFinished)
        {
            return;
        }

        if (!hasArrived)
        {
            if (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                return;
            }
            hasArrived = true;
        }

        // Look around. The agent only turns the guard while walking, so we can rotate it by hand once it has stopped.
        transform.Rotate(0f, turnSpeed * Time.deltaTime, 0f);

        lookTimer += Time.deltaTime;
        if (lookTimer >= lookAroundTime)
        {
            IsFinished = true;
        }
    }
}
