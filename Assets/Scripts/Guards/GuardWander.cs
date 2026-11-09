using UnityEngine;
using UnityEngine.AI;

// Makes the guard stroll between random points inside the level's WanderArea,
// pausing for a moment at each one. GuardAI switches this on and off.
[RequireComponent(typeof(NavMeshAgent))]
public class GuardWander : MonoBehaviour
{
    [Tooltip("Walking speed while wandering, in meters per second. Slower than chasing so it reads as a calm patrol.")]
    [SerializeField] private float wanderSpeed = 2f;

    [Tooltip("Shortest pause at each point, in seconds.")]
    [SerializeField] private float minWaitTime = 1f;

    [Tooltip("Longest pause at each point, in seconds.")]
    [SerializeField] private float maxWaitTime = 3f;

    private NavMeshAgent agent;
    private WanderArea area;
    private float waitTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // The level owns the wander area, so a guard prefab works in any level without retyping sizes.
        // Looked up once here, never in Update.
        area = FindAnyObjectByType<WanderArea>();
        if (area == null)
        {
            Debug.LogError("GuardWander: this level has no WanderArea. Create an empty object and add the WanderArea component.");
        }
    }

    private void OnEnable()
    {
        agent.speed = wanderSpeed;

        // Pick a first point on the next Update instead of here. When the scene loads, the
        // NavMeshAgent may not be placed on the NavMesh yet at the moment OnEnable runs.
        waitTimer = 0f;
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
        // Still walking toward the current point? Nothing to do yet.
        if (agent.pathPending)
        {
            return;
        }
        if (agent.hasPath && agent.remainingDistance > agent.stoppingDistance)
        {
            return;
        }

        // Arrived (or no point yet): wait, then head somewhere new.
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0f)
        {
            PickNewDestination();
            waitTimer = Random.Range(minWaitTime, maxWaitTime);
        }
    }

    private void PickNewDestination()
    {
        if (area == null || !agent.isOnNavMesh)
        {
            return;
        }

        // A random point might land inside a shelf, so try a few times. SamplePosition finds
        // the nearest walkable spot within 1.5 m of the point, or fails if there isn't one.
        for (int attempt = 0; attempt < 10; attempt++)
        {
            if (NavMesh.SamplePosition(area.GetRandomPoint(), out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }
    }
}
