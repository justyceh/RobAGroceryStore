using UnityEngine;
using UnityEngine.AI;

// Makes the guard chase a target (the player) around the store using the baked NavMesh.
// The NavMeshAgent component does the actual pathfinding and walking; this script only
// tells it where to go. Speed, turning and acceleration are set on the NavMeshAgent itself.
[RequireComponent(typeof(NavMeshAgent))]
public class GuardChase : MonoBehaviour
{
    [Tooltip("What the guard chases. Drag the Player from the Hierarchy here.")]
    [SerializeField] private Transform target;

    [Tooltip("Seconds between path updates. Recalculating every frame is wasteful; a few times per second looks the same.")]
    [SerializeField] private float repathInterval = 0.2f;

    private NavMeshAgent agent;
    private float repathTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
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
