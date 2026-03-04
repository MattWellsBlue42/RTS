using UnityEngine;

public class MeshGenerator
{

    private float maxY = float.MinValue;
    private float minY = float.MaxValue;
    private Vector3[] vertices;
    private int[] triangles;
    private Color[] colors;

    private MeshCollider meshCollider;

    private Mesh generatedMesh;

    // Stored for per-tile vertex lookups
    private Vector2Int storedGridSize;
    private Vector2Int storedTileSize;
    private int vertsPerTile;

    public Vector3[] GetVertices() => vertices;
    public int[] GetTriangles() => triangles;


    public MeshGenerator(Vector2Int gridSize, Vector2Int tileSize, float cellSize, float noiseScale, float heightMultiplier, bool generateHeight, MeshCollider meshCollider, out Mesh generatedMesh, Vector3 originPosition = default(Vector3))
    {
        this.meshCollider = meshCollider;
        this.storedGridSize = gridSize;
        this.storedTileSize = tileSize;

        // Total world size
        float worldSizeX = gridSize.x * cellSize;
        float worldSizeZ = gridSize.y * cellSize;

        // Total quads across the mesh (gridSize tiles * tileSize subdivisions per tile)
        int totalQuadsX = gridSize.x * tileSize.x;
        int totalQuadsZ = gridSize.y * tileSize.y;

        // Size of each individual quad (subdivision)
        float quadSizeX = cellSize / tileSize.x;
        float quadSizeZ = cellSize / tileSize.y;

        Debug.Log($"Generating mesh: {gridSize} tiles, {tileSize} subs/tile, cellSize={cellSize}, world={worldSizeX}x{worldSizeZ}, quads={totalQuadsX}x{totalQuadsZ}, quadSize={quadSizeX}x{quadSizeZ}");

        this.generatedMesh = new Mesh
        {
            name = "Grid"
        };
        generatedMesh = this.generatedMesh;

        // Each tile gets its own (tileSize.x+1) * (tileSize.y+1) vertices — no sharing between tiles
        vertsPerTile = (tileSize.x + 1) * (tileSize.y + 1);
        int totalTiles = gridSize.x * gridSize.y;
        int quadsPerTile = tileSize.x * tileSize.y;

        vertices = new Vector3[totalTiles * vertsPerTile];
        colors = new Color[vertices.Length];
        triangles = new int[totalTiles * quadsPerTile * 6];

        int t = 0;

        for (int tileX = 0; tileX < gridSize.x; tileX++)
        {
            for (int tileY = 0; tileY < gridSize.y; tileY++)
            {
                int tileIndex = tileX * gridSize.y + tileY;
                int baseVertex = tileIndex * vertsPerTile;

                // Generate vertices for this tile
                for (int subX = 0; subX <= tileSize.x; subX++)
                {
                    for (int subY = 0; subY <= tileSize.y; subY++)
                    {
                        int globalX = tileX * tileSize.x + subX;
                        int globalY = tileY * tileSize.y + subY;
                        float wx = globalX * quadSizeX + originPosition.x;
                        float wy = globalY * quadSizeZ + originPosition.z;

                        int v = baseVertex + subX * (tileSize.y + 1) + subY;
                        colors[v] = Color.black;
                        vertices[v] = new Vector3(wx, GetMeshY(wx, wy, noiseScale, heightMultiplier, generateHeight), wy);
                    }
                }

                // Generate triangles for this tile
                for (int subX = 0; subX < tileSize.x; subX++)
                {
                    for (int subY = 0; subY < tileSize.y; subY++)
                    {
                        int v = baseVertex + subX * (tileSize.y + 1) + subY;
                        int vRight = v + (tileSize.y + 1);

                        triangles[t]     = v;
                        triangles[t + 1] = vRight + 1;
                        triangles[t + 2] = vRight;
                        triangles[t + 3] = v;
                        triangles[t + 4] = v + 1;
                        triangles[t + 5] = vRight + 1;
                        t += 6;
                    }
                }
            }
        }

        RecalculateMesh();
        Debug.Log($"Mesh generated with {vertices.Length} vertices and {triangles.Length / 3} triangles. Height range: {minY} to {maxY}");
    }

    public void ChangeMeshColorForXY(int tileX, int tileY, Color color, Vector2Int gridSize, Vector2Int tileSize)
    {
        int tileIndex = tileX * gridSize.y + tileY;
        int baseVertex = tileIndex * vertsPerTile;

        for (int i = 0; i < vertsPerTile; i++)
        {
            int vertexIndex = baseVertex + i;
            if (vertexIndex >= 0 && vertexIndex < colors.Length)
            {
                colors[vertexIndex] = color;
            }
        }

        RecalculateMesh();
    }

    private float GetMeshY(float wx, float wz, float noiseScale, float heightMultiplier, bool generateHeight)
    {
        if (!generateHeight) return 0f;
        float y = Mathf.PerlinNoise(wx * noiseScale, wz * noiseScale) * heightMultiplier;
        maxY = Mathf.Max(maxY, y);
        minY = Mathf.Min(minY, y);
        return y;
    }

    private void RecalculateMesh()
    {
        if (generatedMesh == null)
        {
            generatedMesh = new Mesh
            {
                name = "Grid"
            };
        }
        
        generatedMesh.Clear();
        generatedMesh.vertices = vertices;
        generatedMesh.colors = colors;
        generatedMesh.triangles = triangles;
        generatedMesh.RecalculateNormals();
        generatedMesh.RecalculateBounds();
        meshCollider.sharedMesh = generatedMesh;
    }
}