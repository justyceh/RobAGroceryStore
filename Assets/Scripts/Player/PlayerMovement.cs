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

    [Tooltip("Walking speed in meters per second (1 Unity unit = 1 meter).")]
    [SerializeField] private float moveSpeed = 4f;

    [Tooltip("Downward acceleration in meters per second squared. Real gravity is -9.81; games often use more so falling feels snappy.")]
    [SerializeField] private float gravity = -20f;

    private CharacterController controller;

    // CharacterController doesn't do gravity for us (it isn't a Rigidbody), so we track fall speed ourselves.
    private float verticalVelocity;

    private void Awake()
    {
        // Cache the component once instead of looking it up every frame.
        controller = GetComponent<CharacterController>();
    }

    // Input actions only report values while enabled. OnEnable/OnDisable run when this
    // component is turned on/off, similar to subscribing/unsubscribing in a React useEffect.
    private void OnEnable()
    {
        moveAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
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

        // A small downward push while grounded keeps isGrounded reliable on slopes and steps.
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = direction * moveSpeed;
        velocity.y = verticalVelocity;

        // Multiply by Time.deltaTime so speed is "per second" instead of "per frame";
        // otherwise the player would move faster on high-FPS machines.
        controller.Move(velocity * Time.deltaTime);
    }
}
