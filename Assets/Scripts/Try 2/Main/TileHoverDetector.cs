using UnityEngine;

/// <summary>
/// Raycasts from the camera through the mouse cursor to detect which tile
/// the player is looking at, then sets _HoveredTile on the grid material
/// so the shader can draw the highlight outline.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class TileHoverDetector : MonoBehaviour
{
    [Tooltip("The mesh size value used in WorldManager / GridGenerator")]
    [SerializeField] private float meshSize = 1f;

    [Tooltip("Maximum raycast distance")]
    [SerializeField] private float maxRayDistance = 200f;

    private Material mat;
    private MeshCollider meshCollider;

    // Shader property IDs (cached for performance)
    private static readonly int HoveredTileProp = Shader.PropertyToID("_HoveredTile");
    private static readonly int MeshSizeProp    = Shader.PropertyToID("_MeshSize");

    private void Awake()
    {
        mat = GetComponent<MeshRenderer>().material;
        meshCollider = GetComponent<MeshCollider>();

        // Push meshSize to the shader
        if (mat != null)
            mat.SetFloat(MeshSizeProp, meshSize);
    }

    /// <summary>
    /// Call this after the mesh is regenerated so the collider and shader stay in sync.
    /// </summary>
    public void Refresh(float newMeshSize)
    {
        meshSize = newMeshSize;

        // Update mesh collider with the current mesh
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null; // force refresh
            meshCollider.sharedMesh = GetComponent<MeshFilter>().sharedMesh;
        }

        if (mat != null)
            mat.SetFloat(MeshSizeProp, meshSize);
    }

    private void Update()
    {
        if (mat == null) return;

        Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance) && hit.collider == meshCollider)
        {
            // Convert hit point to object space
            Vector3 localHit = transform.InverseTransformPoint(hit.point);

            // Determine tile grid coordinate
            int tileX = Mathf.FloorToInt(localHit.x / meshSize);
            int tileZ = Mathf.FloorToInt(localHit.z / meshSize);

            // xy = tile coord, z = 1 means active
            mat.SetVector(HoveredTileProp, new Vector4(tileX, tileZ, 1f, 0f));
        }
        else
        {
            // Nothing hovered — deactivate
            mat.SetVector(HoveredTileProp, new Vector4(-1, -1, 0f, 0f));
        }
    }

    private void OnDestroy()
    {
        // Clean up instanced material
        if (mat != null)
            Destroy(mat);
    }
}
