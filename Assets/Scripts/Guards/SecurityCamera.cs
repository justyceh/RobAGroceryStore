using UnityEngine;

// A wall-mounted camera that sweeps side to side. It can't chase, but it watches:
// - If it sees you steal, you become Wanted and it radios the guards to that spot.
// - While you're Wanted and in view, it keeps reporting your position every few seconds.
// It reuses GuardSight for "can I see the player?", so a camera follows exactly the same
// vision rules as a guard. Keep this object level (rotation X and Z at 0): it turns itself.
[RequireComponent(typeof(GuardSight))]
public class SecurityCamera : MonoBehaviour
{
    [Tooltip("Total sweep in degrees, e.g. 80 = turns 40 degrees each way from where it starts facing.")]
    [SerializeField] private float sweepAngle = 80f;

    [Tooltip("How fast the camera turns while sweeping, in degrees per second.")]
    [SerializeField] private float sweepSpeed = 20f;

    [Tooltip("Guards within this many meters of the camera hear its alerts.")]
    [SerializeField] private float radioRange = 30f;

    [Tooltip("While you're Wanted and in view, how often the camera re-reports your position to guards, in seconds.")]
    [SerializeField] private float alertInterval = 3f;

    private GuardSight sight;
    private Transform player;
    private PlayerLoot playerLoot;
    private PlayerWanted playerWanted;

    private float startYaw;
    private float sweepTimer;
    private float alertTimer;

    private void Awake()
    {
        sight = GetComponent<GuardSight>();

        // Same one-time lookup as GuardAI, so a camera prefab works without wiring anything up.
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("SecurityCamera: no object is tagged 'Player'. Select the Player and set its Tag to Player.");
            enabled = false;
            return;
        }
        player = playerObject.transform;
        playerLoot = playerObject.GetComponent<PlayerLoot>();
        playerWanted = playerObject.GetComponent<PlayerWanted>();
        sight.Init(player);

        // The sweep centers on whichever way you rotated the camera in the Editor.
        startYaw = transform.eulerAngles.y;
    }

    private void OnEnable()
    {
        playerLoot.ItemStolen += OnItemStolen;
    }

    private void OnDisable()
    {
        playerLoot.ItemStolen -= OnItemStolen;
    }

    private void Update()
    {
        Sweep();

        if (playerWanted.IsWanted && sight.CanSeePlayer())
        {
            alertTimer -= Time.deltaTime;
            if (alertTimer <= 0f)
            {
                alertTimer = alertInterval;
                GuardAI.RadioGuards(transform.position, radioRange, player.position, null);
            }
        }
        else
        {
            // Out of view: reset so the camera reports you the instant it spots you again.
            alertTimer = 0f;
        }
    }

    private void Sweep()
    {
        // PingPong counts up to sweepAngle and back down to 0, over and over, like a bouncing
        // value. Subtracting half centers the sweep: -40 ... +40 degrees around startYaw.
        sweepTimer += Time.deltaTime;
        float offset = Mathf.PingPong(sweepTimer * sweepSpeed, sweepAngle) - sweepAngle / 2f;
        transform.rotation = Quaternion.Euler(0f, startYaw + offset, 0f);
    }

    private void OnItemStolen()
    {
        if (sight.CanSeePlayer())
        {
            playerWanted.MakeWanted();
            GuardAI.RadioGuards(transform.position, radioRange, player.position, null);

            // We just reported, so wait a full interval before the repeat reports start.
            alertTimer = alertInterval;
        }
    }
}
