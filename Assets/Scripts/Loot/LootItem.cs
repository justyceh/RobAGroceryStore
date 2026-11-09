using UnityEngine;

// Put this on anything the player can steal. It doesn't detect the player itself:
// PlayerInteract finds it with a raycast, then calls SetHighlighted and Steal.
// The object also needs a collider so the raycast has something to hit.
// Which kind of item it is (name, value, color) comes from its ItemData asset.
[RequireComponent(typeof(Renderer))]
public class LootItem : MonoBehaviour
{
    [Tooltip("What kind of item this is. Drag an ItemData asset (e.g. Cereal) here from the Project window.")]
    [SerializeField] private ItemData item;

    [Tooltip("Color the item turns while the player is aiming at it and close enough to steal it.")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.2f);

    public ItemData Item => item;

    // URP Lit stores its main color in a property called "_BaseColor". Looking up the ID once
    // is faster than passing the string every time.
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private Renderer itemRenderer;
    private Color normalColor;

    // A MaterialPropertyBlock overrides a material's values for this one object only.
    // That's how items sharing one material can each show their own type's color.
    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        itemRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();

        if (item == null)
        {
            Debug.LogError($"LootItem '{name}' has no Item set. Drag an ItemData asset into its Item slot.", this);
            normalColor = itemRenderer.sharedMaterial.GetColor(BaseColorId);
        }
        else
        {
            normalColor = item.Color;
        }

        SetHighlighted(false);
    }

    public void SetHighlighted(bool highlighted)
    {
        itemRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(BaseColorId, highlighted ? highlightColor : normalColor);
        itemRenderer.SetPropertyBlock(propertyBlock);
    }

    public void Steal()
    {
        // Destroy removes the GameObject from the scene at the end of this frame.
        Destroy(gameObject);
    }
}
