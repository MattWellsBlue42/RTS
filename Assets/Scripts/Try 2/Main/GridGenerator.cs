using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class GridGenerator : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private float noiseScale = 0.3f;
    [SerializeField] private float heightMultiplier = 2f;
    [SerializeField] private GameObject waterPrefab;
    [SerializeField] private List<TileSO> tileTypes;
    [SerializeField] private bool generateHeight = false;
    private Vector3[] vertices;

    private Mesh mesh;

    private Vector2Int gridSize;
    private Vector2Int tileSize;
    private float meshSize;

    private float maxY;
    private float minY;

    private void OnValidate()
    {
        if (!Application.isPlaying) return;
        if (gridSize.x <= 0 || gridSize.y <= 0 || tileSize.x <= 0 || tileSize.y <= 0 || meshSize <= 0f) return;

        StartGen(gridSize, tileSize, meshSize);
    }

    public void StartGen(Vector2Int gridSize, Vector2Int tileSize, float meshSize)
    {
        this.gridSize = gridSize;
        this.tileSize = tileSize;
        this.meshSize = meshSize;
        mesh = new Mesh();
        mesh.name = "Grid";
        mesh.indexFormat = IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = material;
        GenerateGrid(gridSize, tileSize, meshSize);

        // Update collider so raycasting works for hover detection
        if (mesh.vertexCount >= 3)
        {
            var meshCollider = GetComponent<MeshCollider>();
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }

        // Sync hover detector with current mesh size
        var hoverDetector = GetComponent<TileHoverDetector>();
        if (hoverDetector != null)
            hoverDetector.Refresh(meshSize);
    }

    private float GetMeshY(float wx, float wz)
    {
        if (!generateHeight) return 0f;
        float y = Mathf.PerlinNoise(wx * noiseScale, wz * noiseScale) * heightMultiplier;
        maxY = Mathf.Max(maxY, y);
        minY = Mathf.Min(minY, y);
        return y;
    }

    private WorldTile ProcessTile(int tileX, int tileY)
    {
        int randomIndex = Random.Range(0, tileTypes.Count);
        TileSO tileSO = tileTypes[randomIndex];
        Tile tileData = new Tile(tileSO, tileSize);
        WorldTile worldTile = new WorldTile
        {
            id = System.Guid.NewGuid().ToString(),
            gridPosition = new Vector2Int(tileX, tileY),
            isUnitOccupied = false,
            isWalkable = true,
            isBuildingOccupied = false,
            isResourceOccupied = false,
            tileView = new TileView(tileData)
        };

        return worldTile;
    }

    public void GenerateGrid(Vector2Int gridSize, Vector2Int tileSize, float meshSize)
    {
        maxY = float.MinValue;
        minY = float.MaxValue;

        int totalSubX = gridSize.x * tileSize.x;
        int totalSubY = gridSize.y * tileSize.y;

        float subSizeX = meshSize / tileSize.x;
        float subSizeY = meshSize / tileSize.y;

        // Shared vertex grid: (totalSubX+1) * (totalSubY+1) vertices
        vertices = new Vector3[(totalSubX + 1) * (totalSubY + 1)];
        Color[] colors = new Color[vertices.Length];
        int[] triangles = new int[totalSubX * totalSubY * 6];

        for (int tileX = 0; tileX < gridSize.x; tileX++)
        {
            for (int tileY = 0; tileY < gridSize.y; tileY++)
            {
                WorldTile worldTile = ProcessTile(tileX, tileY);

                for (int subX = 0; subX <= tileSize.x; subX++)
                {
                    for (int subY = 0; subY <= tileSize.y; subY++)
                    {
                        int x = tileX * tileSize.x + subX;
                        int y = tileY * tileSize.y + subY;
                        float wx = x * subSizeX;
                        float wy = y * subSizeY;
                        int v = x * (totalSubY + 1) + y;
                        vertices[v] = new Vector3(wx, GetMeshY(wx, wy), wy);
                    }
                }
            }
        }

        // Second pass: color uses correct minY/maxY now that all heights are known
        for (int v = 0; v < vertices.Length; v++)
        {
            float normalizedY = Mathf.InverseLerp(minY, maxY, vertices[v].y);
            colors[v] = GetVertexColor(normalizedY);
        }

        int t = 0;
        for (int x = 0; x < totalSubX; x++)
        {
            for (int y = 0; y < totalSubY; y++)
            {
                int v = x * (totalSubY + 1) + y;
                int vRight = v + (totalSubY + 1);
                triangles[t] = v;
                triangles[t + 1] = vRight + 1;
                triangles[t + 2] = vRight;
                triangles[t + 3] = v;
                triangles[t + 4] = v + 1;
                triangles[t + 5] = vRight + 1;
                t += 6;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private Color GetVertexColor(float y)
    {
        TileType tileType = TileType.Grass;
        if (y < 0.3f)
            tileType = TileType.Water;
        else if (y > 0.6f)
            tileType = TileType.Stone;

        Color tileColor = Color.white;
        foreach (var tileSO in tileTypes)
        {
            if (tileSO.tileType == tileType)
            {
                tileColor = tileSO.tileColor;
                break;
            }
        }
        return tileColor;
    }
}