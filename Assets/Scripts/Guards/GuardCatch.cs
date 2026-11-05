using UnityEngine;

// Ends the game when the guard gets close enough to the player.
// Kept separate from GuardChase so chasing and catching can be switched off independently
// (e.g. a stunned guard might stop chasing but a later feature could still let it catch).
public class GuardCatch : MonoBehaviour
{
    [Tooltip("The player to catch. Drag the Player from the Hierarchy here.")]
    [SerializeField] private Transform target;

    [Tooltip("The GameManager in the scene. Drag the GameManager object here.")]
    [SerializeField] private GameManager gameManager;

    [Tooltip("Distance between the guard's and player's centers that counts as caught, in meters. The two capsules touch at 1.0.")]
    [SerializeField] private float catchDistance = 1.3f;

    private void Update()
    {
        // A plain distance check instead of physics collisions: the NavMeshAgent stops the guard
        // just short of the player, so their colliders might never actually touch.
        if (Vector3.Distance(transform.position, target.position) <= catchDistance)
        {
            gameManager.PlayerCaught();
        }
    }
}
