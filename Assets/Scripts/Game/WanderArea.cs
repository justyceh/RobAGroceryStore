using UnityEngine;

// Marks the part of a level that guards wander around in. Put one in each level scene and
// size it to fit the store; every guard in that level finds it automatically.
// The area is centered on this object and drawn as a yellow box in the Scene view.
// It ignores rotation, so it always lines up with the world X and Z axes.
public class WanderArea : MonoBehaviour
{
    [Tooltip("Width (X) and depth (Z) of the area in meters, centered on this object.")]
    [SerializeField] private Vector2 size = new Vector2(28f, 18f);

    // A random point somewhere inside the area, at this object's height.
    public Vector3 GetRandomPoint()
    {
        return transform.position + new Vector3(
            Random.Range(-size.x / 2f, size.x / 2f),
            0f,
            Random.Range(-size.y / 2f, size.y / 2f));
    }

    // OnDrawGizmos (not ...Selected) draws even when this object isn't selected,
    // so you can always see the wander area while building a level.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 0.1f, size.y));
    }
}
