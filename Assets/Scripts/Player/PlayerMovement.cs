using UnityEngine;
using UnityEngine.InputSystem;

// Moves the player with WASD / left stick using a CharacterController.
// Movement is relative to the way the player is facing, so once mouse-look turns
// the body (next step), W will always mean "walk where I'm looking".
// RequireComponent makes Unity add a CharacterController automatically when this script is added.
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Tooltip("The Move action from InputSystem_Actions (Player/Move). It gives a Vector2: x = left/right, y = forward/back.")]
    [SerializeField] private InputActionReference moveAction;

    [Tooltip("The Jump action from InputSystem_Actions (Player/Jump), bound to Space.")]
    [SerializeField] private InputActionReference jumpAction;

    [Tooltip("The Sprint action from InputSystem_Actions (Player/Sprint), bound to Left Shift.")]
    [SerializeField] private InputActionReference sprintAction;

    [Tooltip("Walking speed in meters per second (1 Unity unit = 1 meter).")]
    [SerializeField] private float moveSpeed = 4f;

    [Tooltip("Speed in meters per second while holding Sprint.")]
    [SerializeField] private float sprintSpeed = 7f;

    [Tooltip("How high a jump goes, in meters. The capsule is 2 m tall, so 1.2 clears a waist-high box.")]
    [SerializeField] private float jumpHeight = 1.2f;

    [Tooltip("Downward acceleration in meters per second squared. Real gravity is -9.81; games often use more so falling feels snappy.")]
    [SerializeField] private float gravity = -20f;

    private CharacterController controller;

    // Optional: if the Player has PlayerBuffs, Red Bull / Jordans boosts are applied. Without it, no boosts.
    private PlayerBuffs buffs;

    // CharacterController doesn't do gravity for us (it isn't a Rigidbody), so we track fall speed ourselves.
    private float verticalVelocity;

    // The world direction WASD is pushing this frame (length 0 to 1). PlayerDash reads it to know which way to dash.
    public Vector3 MoveDirection { get; private set; }

    private void Awake()
    {
        // Cache the components once instead of looking them up every frame.
        controller = GetComponent<CharacterController>();
        TryGetComponent(out buffs);
    }

    // Input actions only report values while enabled. OnEnable/OnDisable run when this
    // component is turned on/off, similar to subscribing/unsubscribing in a React useEffect.
    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        sprintAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
        sprintAction.action.Disable();
    }

    // Update runs once per rendered frame. We use it (not FixedUpdate) because
    // CharacterController.Move isn't driven by the physics engine.
    private void Update()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        // Turn the 2D input into a 3D direction based on the player's facing.
        // ClampMagnitude stops diagonals (W+D) from being faster than straight lines.
        Vector3 direction = transform.right * input.x + transform.forward * input.y;
        direction = Vector3.ClampMagnitude(direction, 1f);
        MoveDirection = direction;

        // 1 when no buff is active, e.g. 1.5 while Red Bull is active.
        float speedMultiplier = buffs != null ? buffs.SpeedMultiplier : 1f;
        float jumpMultiplier = buffs != null ? buffs.JumpMultiplier : 1f;

        // A small downward push while grounded keeps isGrounded reliable on slopes and steps.
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        // Only jump when standing on something, otherwise you could jump forever in mid-air.
        // WasPressedThisFrame fires once per key press, so holding Space doesn't bunny-hop.
        if (controller.isGrounded && jumpAction.action.WasPressedThisFrame())
        {
            // Physics formula: the upward speed needed to reach jumpHeight under this gravity.
            // Tuning height in meters is easier to reason about than tuning a raw launch speed.
            verticalVelocity = Mathf.Sqrt(jumpHeight * jumpMultiplier * -2f * gravity);
        }
        verticalVelocity += gravity * Time.deltaTime;

        // IsPressed is true every frame the key is held, unlike WasPressedThisFrame.
        float speed = (sprintAction.action.IsPressed() ? sprintSpeed : moveSpeed) * speedMultiplier;

        Vector3 velocity = direction * speed;
        velocity.y = verticalVelocity;

        // Multiply by Time.deltaTime so speed is "per second" instead of "per frame";
        // otherwise the player would move faster on high-FPS machines.
        controller.Move(velocity * Time.deltaTime);
    }
}
