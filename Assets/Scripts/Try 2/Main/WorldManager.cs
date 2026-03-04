using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class WorldManager : MonoBehaviour
{
    #region Tile Data
    [Header("Tile Settings")]
    [SerializeField] private List<TileSO> tileTypes;
    #endregion

    #region Grid Data
    [Header("Grid Settings")]
    [SerializeField] private GameObject tileHighlighterPrefab;
    private bool highlighterSpawned = false;
    private GameObject currentHighlighter;
    [Min(1)]
    [SerializeField] public Vector2Int gridSize = new(5, 5);
    [Min(1)]
    [SerializeField] public Vector2Int tileSize = new(2, 2);
    public float cellSize = 1f;
    #endregion

    private GridGenerator gridGenerator;
    private MeshGenerator meshGenerator;
    private TileHoverDetector hoverDetector;
    private MeshCollider meshCollider;
    private Camera mainCamera;
    private Mouse mouse;

    private Vector2Int? lastHoveredTile = null;

    #region Mesh Data
    private Mesh mesh;
    [Header("Mesh Data")]
    [SerializeField] private Material material;
    [SerializeField] private float noiseScale = 0.3f;
    [SerializeField] private float heightMultiplier = 2f;
    [SerializeField] private bool generateHeight = false;
    #endregion 

    private void OnValidate()
    {
        if (!Application.isPlaying) return;

        GenerateWorld();
    }

    private void HandleHighlight(Vector3 location)
    {
        // if (highlighterSpawned)
        // {
        //     currentHighlighter.transform.position = location + new Vector3(cellSize * 0.5f, 0.01f, cellSize * 0.5f);
        // }
        // else
        // {
        //     currentHighlighter = Instantiate(tileHighlighterPrefab, location + new Vector3(cellSize * 0.5f, 0.01f, cellSize * 0.5f), Quaternion.Euler(90, 0, 0));
        //     highlighterSpawned = true;
        // }
    }

    private void Update()
    {
        if (hoverDetector == null) return;

        hoverDetector.SendRaycast(out Vector3? hitLocation);

        if (hitLocation == null)
        {
            if (lastHoveredTile.HasValue)
            {
                meshGenerator.ChangeMeshColorForXY(lastHoveredTile.Value.x, lastHoveredTile.Value.y, Color.black, gridSize, tileSize);
                lastHoveredTile = null;
            }
            return;
        }

        gridGenerator.GetXY((Vector3)hitLocation, out int x, out int y);

        if (gridGenerator.IsValidCell(x, y))
        {
            Vector2Int currentTile = new Vector2Int(x, y);

            if (lastHoveredTile.HasValue && lastHoveredTile.Value != currentTile)
            {
                meshGenerator.ChangeMeshColorForXY(lastHoveredTile.Value.x, lastHoveredTile.Value.y, Color.black, gridSize, tileSize);
            }

            meshGenerator.ChangeMeshColorForXY(x, y, Color.red, gridSize, tileSize);
            lastHoveredTile = currentTile;
        }
    }

    private void GenerateWorld()
    {
        mainCamera = Camera.main;
        mouse = Mouse.current;
        meshCollider = GetComponent<MeshCollider>();

        hoverDetector = new TileHoverDetector(mainCamera, mouse);

        // Grid is tile-level: gridSize.x by gridSize.y tiles, each cellSize wide
        gridGenerator = new GridGenerator(gridSize.x, gridSize.y, cellSize);
        // Mesh subdivides each tile into tileSize quads for detail, but stays within grid bounds
        meshGenerator = new MeshGenerator(gridSize, tileSize, cellSize, noiseScale, heightMultiplier, generateHeight, meshCollider, out Mesh mesh, gridGenerator.GetWorldPosition(0, 0));
        this.mesh = mesh;
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = material;
    }

    void OnDrawGizmos()
    {
        if (gridGenerator == null) return;

        for (int x = 0; x < gridGenerator.GridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridGenerator.GridArray.GetLength(1); y++)
            {
                Gizmos.color = Color.white;
                // Get the world position of the cell
                Vector3 pos = gridGenerator.GetWorldPosition(x, y);
                // Move the gizmo to the center of the cell
                Vector3 center = pos + new Vector3(cellSize * 0.5f, 0, cellSize * 0.5f);

                // Draw a wire cube at the center of the cell (it draws from the center, so we need to offset it by half the cell size)
                Gizmos.DrawWireCube(center, Vector3.one * cellSize);

                // Draw the cell coordinates as a label at the center of the cell
                Handles.Label(center, gridGenerator.PositionToString(x, y), EditorStyles.boldLabel);
            }
        }

        if (hoverDetector == null) return;

        // Draw a ray from the camera to the mouse position
        Ray ray = mainCamera.ScreenPointToRay(hoverDetector.LastMousePosition);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(ray.origin, ray.direction * 100f);
    }
}
