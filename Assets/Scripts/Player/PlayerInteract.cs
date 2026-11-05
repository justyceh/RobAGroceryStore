using UnityEngine;
using UnityEngine.InputSystem;

// Every frame, shoots an invisible line (a raycast) from the center of the camera.
// If it hits a LootItem close enough to the player, that item is highlighted,
// and pressing Interact (E) steals it. If the thin line just misses, a fatter "sphere cast"
// gives some aim assist so you don't have to hit small items pixel-perfectly.
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

    [Tooltip("The layer(s) loot items are on. Only objects on these layers can be highlighted and stolen.")]
    [SerializeField] private LayerMask lootLayers;

    [Tooltip("How far off-center you can aim and still grab an item, in meters. 0 = exact aim only.")]
    [SerializeField] private float aimAssistRadius = 0.35f;

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
        Collider hitCollider = FindLootCollider();

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

    // Returns the collider of the loot item being aimed at, or null if there isn't one.
    private Collider FindLootCollider()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        // 1. Exact aim: a thin ray against everything, so walls and shelves block it.
        // QueryTriggerInteraction.Ignore: invisible trigger zones (like a future exit zone) won't block the ray.
        if (Physics.Raycast(ray, out RaycastHit hit, rayLength, interactLayers, QueryTriggerInteraction.Ignore)
            && IsLoot(hit.collider) && IsInRange(hit.point))
        {
            return hit.collider;
        }

        // 2. Aim assist: sweep a ball of aimAssistRadius along the same line, but only against loot.
        // Ignoring shelves here means an item sitting against a shelf can't be "hidden" by the shelf
        // when you aim just beside it. The line-of-sight check stops you grabbing through walls.
        if (Physics.SphereCast(ray, aimAssistRadius, out hit, rayLength, lootLayers, QueryTriggerInteraction.Ignore)
            && IsInRange(hit.point) && HasLineOfSight(hit.collider))
        {
            return hit.collider;
        }

        return null;
    }

    private bool IsLoot(Collider other)
    {
        // A LayerMask is a set of on/off bits, one per layer. This checks whether the
        // collider's layer bit is switched on, which is much cheaper than GetComponent.
        return (lootLayers.value & (1 << other.gameObject.layer)) != 0;
    }

    private bool IsInRange(Vector3 point)
    {
        // Measure from the player's body, not the camera, so range feels the same in both views.
        return Vector3.Distance(transform.position, point) <= interactRange;
    }

    private bool HasLineOfSight(Collider loot)
    {
        // Draw a line from the camera to the item's center. If the first thing it hits is
        // something else (a wall or shelf), the item is hidden behind it.
        Vector3 from = playerCamera.transform.position;
        Vector3 to = loot.bounds.center;
        if (Physics.Linecast(from, to, out RaycastHit hit, interactLayers, QueryTriggerInteraction.Ignore))
        {
            return hit.collider == loot;
        }
        return true;
    }
}
