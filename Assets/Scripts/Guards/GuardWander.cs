using UnityEngine;
using UnityEngine.AI;

// Makes the guard stroll between random points inside a rectangular area (the store),
// pausing for a moment at each one. GuardAI switches this on and off.
[RequireComponent(typeof(NavMeshAgent))]
public class GuardWander : MonoBehaviour
{
    [Tooltip("Center of the area the guard wanders in, in world space. Shown as a yellow box in the Scene view when the guard is selected.")]
    [SerializeField] private Vector3 areaCenter = new Vector3(0f, 0f, 0f);

    [Tooltip("Width (X) and depth (Z) of the wander area in meters. Y is ignored. The store's inside is about 29 x 19.")]
    [SerializeField] private Vector3 areaSize = new Vector3(28f, 0f, 18f);

    [Tooltip("Walking speed while wandering, in meters per second. Slower than chasing so it reads as a calm patrol.")]
    [SerializeField] private float wanderSpeed = 2f;

    [Tooltip("Shortest pause at each point, in seconds.")]
    [SerializeField] private float minWaitTime = 1f;

    [Tooltip("Longest pause at each point, in seconds.")]
    [SerializeField] private float maxWaitTime = 3f;

    private NavMeshAgent agent;
    private float waitTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
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
        if (!agent.isOnNavMesh)
        {
            return;
        }

        // A random point might land inside a shelf, so try a few times. SamplePosition finds
        // the nearest walkable spot within 1.5 m of the point, or fails if there isn't one.
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector3 randomPoint = areaCenter + new Vector3(
                Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
                0f,
                Random.Range(-areaSize.z / 2f, areaSize.z / 2f));

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }
    }

    // Editor-only drawing: shows the wander area as a yellow box when the guard is selected.
    // It never appears in the game itself.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(areaCenter, new Vector3(areaSize.x, 0.1f, areaSize.z));
    }
}
