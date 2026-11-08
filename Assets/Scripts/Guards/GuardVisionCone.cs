using UnityEngine;

// Draws the guard's vision cone as a see-through shape on the floor, so the player can see
// where it's safe to steal. The shape is cut short wherever a wall or shelf blocks the view,
// so it matches what GuardSight can really see.
// Put this on a child object of the guard, just above the floor.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GuardVisionCone : MonoBehaviour
{
    [Tooltip("The GuardSight on the guard. The cone copies its range, angle and obstacle layers. Drag the Guard (the parent) here.")]
    [SerializeField] private GuardSight sight;

    [Tooltip("How many rays build the cone's edge. More = smoother edges around shelves, but more work each frame.")]
    [SerializeField] private int rayCount = 30;

    private Mesh mesh;
    private Vector3[] vertices;

    private void Awake()
    {
        // A Mesh is the raw shape data the renderer draws: a list of corner points (vertices)
        // plus triangles that connect them. We build a "fan": one point at the guard,
        // and a row of points along the cone's far edge.
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];
        for (int i = 0; i < rayCount; i++)
        {
            // Each triangle joins the center point (0) to two neighbouring edge points.
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        // Triangles must point at vertices that exist, so set the (all-zero) vertices first.
        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }

    // LateUpdate runs after every Update this frame, so the guard has already moved and
    // turned before we redraw its cone. Otherwise the cone would lag one frame behind.
    private void LateUpdate()
    {
        float halfAngle = sight.ViewAngle / 2f;
        Vector3 eyes = sight.EyePosition;

        vertices[0] = Vector3.zero;
        for (int i = 0; i <= rayCount; i++)
        {
            // Sweep from the left edge of the cone to the right edge.
            float angle = -halfAngle + sight.ViewAngle * i / rayCount;
            Vector3 localDirection = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            Vector3 worldDirection = transform.TransformDirection(localDirection);

            // Rays go out at eye height (so low counters don't cut the cone, same as real sight),
            // but the points are placed at this object's height, on the floor.
            float distance = sight.SightRange;
            if (Physics.Raycast(eyes, worldDirection, out RaycastHit hit, sight.SightRange, sight.ObstacleLayers, QueryTriggerInteraction.Ignore))
            {
                distance = hit.distance;
            }

            // Mesh points are relative to this object, like position: relative in CSS.
            vertices[i + 1] = localDirection * distance;
        }

        mesh.vertices = vertices;

        // Bounds tell Unity where the shape is, so it isn't skipped when it's actually on screen.
        mesh.RecalculateBounds();
    }
}
