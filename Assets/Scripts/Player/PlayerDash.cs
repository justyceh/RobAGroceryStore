using UnityEngine;
using UnityEngine.InputSystem;

// Press Dash (Q) to burst a few meters in the direction you're walking
// (or straight ahead if you're standing still). Works in the air too.
// A dash is just a very fast move for a split second, then a cooldown before the next one.
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerDash : MonoBehaviour
{
    [Tooltip("The Dash action from InputSystem_Actions (Player/Dash), bound to Q.")]
    [SerializeField] private InputActionReference dashAction;

    [Tooltip("How far one dash moves you, in meters (before buffs like Red Bull).")]
    [SerializeField] private float dashDistance = 5f;

    [Tooltip("How many seconds the dash takes. Shorter = snappier. 0.15 is a quick burst.")]
    [SerializeField] private float dashDuration = 0.15f;

    [Tooltip("Seconds you must wait after a dash before you can dash again.")]
    [SerializeField] private float cooldown = 1f;

    private CharacterController controller;
    private PlayerMovement movement;
    private PlayerBuffs buffs; // optional: without it, dashes are never boosted

    private float dashTimeLeft;
    private float cooldownLeft;
    private Vector3 dashVelocity;

    public bool IsDashing => dashTimeLeft > 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        movement = GetComponent<PlayerMovement>();
        TryGetComponent(out buffs);
    }

    private void OnEnable()
    {
        dashAction.action.Enable();
    }

    private void OnDisable()
    {
        dashAction.action.Disable();
        dashTimeLeft = 0f; // e.g. caught mid-dash: don't finish the dash later
    }

    private void Update()
    {
        if (cooldownLeft > 0f)
        {
            cooldownLeft -= Time.deltaTime;
        }

        if (dashAction.action.WasPressedThisFrame() && !IsDashing && cooldownLeft <= 0f)
        {
            StartDash();
        }

        if (IsDashing)
        {
            // On the last frame only move for the time that's actually left,
            // so the dash covers exactly dashDistance instead of overshooting a bit.
            float step = Mathf.Min(Time.deltaTime, dashTimeLeft);

            // PlayerMovement also calls Move this frame (for walking and gravity). Two Move calls
            // per frame are fine: CharacterController just adds them up, and still stops at walls.
            controller.Move(dashVelocity * step);
            dashTimeLeft -= step;
        }
    }

    private void StartDash()
    {
        // Dash where you're walking; if you aren't pressing WASD, dash forward.
        Vector3 direction = movement.MoveDirection;
        if (direction.sqrMagnitude < 0.01f)
        {
            direction = transform.forward;
        }
        direction.y = 0f;
        direction.Normalize();

        float distance = dashDistance * (buffs != null ? buffs.DashMultiplier : 1f);
        float duration = Mathf.Max(0.01f, dashDuration);

        // speed = distance / time, so the dash always covers the full distance in dashDuration seconds.
        dashVelocity = direction * (distance / duration);
        dashTimeLeft = duration;
        cooldownLeft = cooldown;
    }
}
