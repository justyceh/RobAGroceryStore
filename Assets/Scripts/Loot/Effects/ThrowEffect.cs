using UnityEngine;

// Throws (or kicks) a projectile prefab: Banana, Eggs, Soccer ball...
// This asset holds HOW it's thrown; the prefab's own script decides what happens when it hits
// (BananaPeel, EggProjectile, KnockdownBall).
// Create one with right-click in the Project window > Create > Heist > Effects > Throw.
[CreateAssetMenu(fileName = "NewThrow", menuName = "Heist/Effects/Throw")]
public class ThrowEffect : ItemEffect
{
    [Tooltip("The prefab to spawn and launch. It must have a Rigidbody and a Collider.")]
    [SerializeField] private GameObject projectilePrefab;

    [Tooltip("How fast it leaves your hand (or foot), in meters per second, in the direction you're aiming.")]
    [SerializeField] private float forwardSpeed = 12f;

    [Tooltip("Extra upward speed in meters per second. More = a higher, loopier arc.")]
    [SerializeField] private float upwardSpeed = 3f;

    [Tooltip("Ticked = kicked from your feet straight ahead (soccer ball). Unticked = thrown from your hand where you're looking.")]
    [SerializeField] private bool kick = false;

    public override bool Use(GameObject user)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{name} has no Projectile Prefab set.");
            return false;
        }

        if (!user.TryGetComponent(out PlayerThrower thrower))
        {
            Debug.LogWarning($"{name} needs a PlayerThrower component on the Player.");
            return false;
        }

        return thrower.Launch(projectilePrefab, forwardSpeed, upwardSpeed, kick);
    }
}
