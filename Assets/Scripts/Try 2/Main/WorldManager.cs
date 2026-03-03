using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WorldManager : MonoBehaviour
{
    #region Tile Data
    [Header("Tile Settings")]
    [SerializeField] private List<TileSO> tileTypes;
    #endregion

    #region Grid Data
    [Header("Grid Settings")]
    [Min(1)]
    [SerializeField] public Vector2Int gridSize = new(5, 5);
    [Min(1)]
    [SerializeField] public Vector2Int tileSize = new(2, 2);
    public float cellSize = 1f;
    #endregion

    private GridGenerator gridGenerator;
    private MeshGenerator meshGenerator;

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

    private void GenerateWorld()
    {
        // Grid is tile-level: gridSize.x by gridSize.y tiles, each cellSize wide
        gridGenerator = new GridGenerator(gridSize.x, gridSize.y, cellSize);
        // Mesh subdivides each tile into tileSize quads for detail, but stays within grid bounds
        meshGenerator = new MeshGenerator(gridSize, tileSize, cellSize, noiseScale, heightMultiplier, generateHeight, out Mesh mesh, gridGenerator.GetWorldPosition(0, 0));
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
    }
}
