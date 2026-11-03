using UnityEngine;
using UnityEngine.InputSystem;

// Turns the player with the mouse.
// Left/right (yaw) rotates the whole body, so PlayerMovement's "forward" follows where you look.
// Up/down (pitch) only tilts the head-height CameraTarget, so the body never leans over.
// Both the first-person and third-person cameras follow CameraTarget, which keeps the two views in sync.
public class PlayerLook : MonoBehaviour
{
    [Tooltip("The Look action from InputSystem_Actions (Player/Look). With a mouse it gives how many pixels the mouse moved this frame.")]
    [SerializeField] private InputActionReference lookAction;

    [Tooltip("Child object at head height that the cameras follow. It tilts up/down; the body turns left/right.")]
    [SerializeField] private Transform cameraTarget;

    [Tooltip("Degrees turned per pixel of mouse movement. Raise it if turning feels slow.")]
    [SerializeField] private float mouseSensitivity = 0.1f;

    [Tooltip("How far you can look up or down, in degrees. Kept below 90 so the view can't flip upside down.")]
    [SerializeField] private float maxPitch = 80f;

    // We store pitch ourselves instead of reading it back from the rotation, because
    // Unity reports angles as 0-360 (looking up 10 degrees reads as 350), which makes clamping messy.
    private float pitch;

    private void OnEnable()
    {
        lookAction.action.Enable();

        // Hide the cursor and keep it in the middle of the Game view so it can't wander off-screen.
        // In the Editor, press Esc to get the cursor back.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        lookAction.action.Disable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        // No Time.deltaTime here: mouse delta is already "how far the mouse moved this frame",
        // so multiplying by frame time would make turning depend on FPS.
        Vector2 delta = lookAction.action.ReadValue<Vector2>() * mouseSensitivity;

        // Yaw: spin the body around the vertical (Y) axis.
        transform.Rotate(0f, delta.x, 0f);

        // Pitch: moving the mouse up gives positive y, but a positive X rotation looks down, so subtract.
        pitch = Mathf.Clamp(pitch - delta.y, -maxPitch, maxPitch);
        cameraTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
