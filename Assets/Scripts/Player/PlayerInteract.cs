using UnityEngine;
using UnityEngine.InputSystem;

// Every frame, shoots an invisible line (a raycast) from the center of the camera.
// If it hits a LootItem close enough to the player, that item is highlighted,
// and pressing Interact (E) steals it.
// Because the ray starts at the camera, this works in both first and third person.
[RequireComponent(typeof(PlayerLoot))]
public class PlayerInteract : MonoBehaviour
{
    [Tooltip("The Interact action from InputSystem_Actions (Player/Interact), bound to E.")]
    [SerializeField] private InputActionReference interactAction;

    [Tooltip("The Main Camera (the one with the Cinemachine Brain). The ray goes out from its center.")]
    [SerializeField] private Camera playerCamera;

    [Tooltip("How close the item must be to the player's body to steal it, in meters.")]
    [SerializeField] private float interactRange = 2.5f;

    [Tooltip("How far the ray travels from the camera. Must be longer than interactRange plus the third-person camera distance.")]
    [SerializeField] private float rayLength = 10f;

    [Tooltip("Which layers the ray can hit. Untick the Player layer so the ray doesn't hit your own capsule in third person.")]
    [SerializeField] private LayerMask interactLayers = ~0;

    private PlayerLoot playerLoot;

    // The item currently highlighted, and the collider we last looked at.
    // We only call GetComponent when the ray moves to a different collider, rather than every frame.
    private LootItem currentTarget;
    private Collider lastCollider;

    private void Awake()
    {
        playerLoot = GetComponent<PlayerLoot>();
    }

    private void OnEnable()
    {
        interactAction.action.Enable();
    }

    private void OnDisable()
    {
        interactAction.action.Disable();
    }

    private void Update()
    {
        FindTarget();

        // The Interact action has a "Hold" interaction in the default asset. WasPressedThisFrame
        // ignores interactions, so a normal tap of E still works.
        if (currentTarget != null && interactAction.action.WasPressedThisFrame())
        {
            currentTarget.Steal();
            playerLoot.AddLoot();
            currentTarget = null;
            lastCollider = null;
        }
    }

    private void FindTarget()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        // QueryTriggerInteraction.Ignore: invisible trigger zones (like a future exit zone) won't block the ray.
        Collider hitCollider = null;
        if (Physics.Raycast(ray, out RaycastHit hit, rayLength, interactLayers, QueryTriggerInteraction.Ignore))
        {
            // Measure from the player's body, not the camera, so range feels the same in both views.
            if (Vector3.Distance(transform.position, hit.point) <= interactRange)
            {
                hitCollider = hit.collider;
            }
        }

        if (hitCollider == lastCollider)
        {
            return;
        }
        lastCollider = hitCollider;

        // Hitting a wall or shelf gives null here, which un-highlights the previous item.
        LootItem newTarget = hitCollider != null ? hitCollider.GetComponent<LootItem>() : null;

        if (currentTarget != null)
        {
            currentTarget.SetHighlighted(false);
        }
        currentTarget = newTarget;
        if (currentTarget != null)
        {
            currentTarget.SetHighlighted(true);
        }
    }
}
