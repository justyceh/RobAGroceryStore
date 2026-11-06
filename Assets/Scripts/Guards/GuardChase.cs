using UnityEngine;
using UnityEngine.AI;

// Makes the guard chase a target (the player) around the store using the baked NavMesh.
// The NavMeshAgent component does the actual pathfinding and walking; this script only
// tells it where to go and how fast. Turning and acceleration are set on the NavMeshAgent itself.
// GuardAI switches this on and off.
[RequireComponent(typeof(NavMeshAgent))]
public class GuardChase : MonoBehaviour
{
    [Tooltip("What the guard chases. Drag the Player from the Hierarchy here.")]
    [SerializeField] private Transform target;

    [Tooltip("Seconds between path updates. Recalculating every frame is wasteful; a few times per second looks the same.")]
    [SerializeField] private float repathInterval = 0.2f;

    [Tooltip("Running speed while chasing, in meters per second. The player walks at 4 and sprints at 7.")]
    [SerializeField] private float chaseSpeed = 5f;

    private NavMeshAgent agent;
    private float repathTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        // The agent has one speed setting shared by every behaviour, so each one sets its own when it starts.
        agent.speed = chaseSpeed;

        // Path to the player on the very next Update instead of waiting out the timer.
        repathTimer = 0f;
    }

    private void Update()
    {
        // Count down, and when the timer runs out, give the agent the player's latest position.
        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = repathInterval;
            agent.SetDestination(target.position);
        }
    }

    // When this script is turned off, the agent would otherwise keep walking its last path.
    // Clearing it makes the guard stand still, so later features (stun, distraction) can stop
    // the chase just by disabling this component.
    private void OnDisable()
    {
        if (agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
    }
}
