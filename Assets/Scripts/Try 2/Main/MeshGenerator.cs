using UnityEngine;

public class MeshGenerator
{

    private float maxY = float.MinValue;
    private float minY = float.MaxValue;
    private Vector3[] vertices;
    private int[] triangles;


    public Vector3[] GetVertices() => vertices;
    public int[] GetTriangles() => triangles;


    /// <summary>
    /// Generates a mesh that fits exactly within gridSize * cellSize world units.
    /// tileSize controls the number of quads per grid square (detail level).
    /// </summary>
    public MeshGenerator(Vector2Int gridSize, Vector2Int tileSize, float cellSize, float noiseScale, float heightMultiplier, bool generateHeight, out Mesh generatedMesh, Vector3 originPosition = default(Vector3))
    {
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

        generatedMesh = new Mesh
        {
            name = "Grid"
        };

        // Shared vertex grid: (totalQuadsX+1) * (totalQuadsZ+1) vertices
        vertices = new Vector3[(totalQuadsX + 1) * (totalQuadsZ + 1)];
        Color[] colors = new Color[vertices.Length];
        triangles = new int[totalQuadsX * totalQuadsZ * 6];

        for (int tileX = 0; tileX < gridSize.x; tileX++)
        {
            for (int tileY = 0; tileY < gridSize.y; tileY++)
            {
                // Here is where I want to do something for tile data possibly

                for (int subX = 0; subX <= tileSize.x; subX++)
                {
                    for (int subY = 0; subY <= tileSize.y; subY++)
                    {
                        int x = tileX * tileSize.x + subX;
                        int y = tileY * tileSize.y + subY;
                        float wx = x * quadSizeX + originPosition.x;
                        float wy = y * quadSizeZ + originPosition.z;
                        int v = x * (totalQuadsZ + 1) + y;
                        vertices[v] = new Vector3(wx, GetMeshY(wx, wy, noiseScale, heightMultiplier, generateHeight), wy);
                    }
                }
            }
        }

        int t = 0;
        for (int x = 0; x < totalQuadsX; x++)
        {
            for (int y = 0; y < totalQuadsZ; y++)
            {
                int v = x * (totalQuadsZ + 1) + y;
                int vRight = v + (totalQuadsZ + 1);
                triangles[t] = v;
                triangles[t + 1] = vRight + 1;
                triangles[t + 2] = vRight;
                triangles[t + 3] = v;
                triangles[t + 4] = v + 1;
                triangles[t + 5] = vRight + 1;
                t += 6;
            }
        }

        generatedMesh.Clear();
        generatedMesh.vertices = vertices;
        generatedMesh.colors = colors;
        generatedMesh.triangles = triangles;
        generatedMesh.RecalculateNormals();
        generatedMesh.RecalculateBounds();
        Debug.Log($"Mesh generated with {vertices.Length} vertices and {triangles.Length / 3} triangles. Height range: {minY} to {maxY}");
    }

    private float GetMeshY(float wx, float wz, float noiseScale, float heightMultiplier, bool generateHeight)
    {
        if (!generateHeight) return 0f;
        float y = Mathf.PerlinNoise(wx * noiseScale, wz * noiseScale) * heightMultiplier;
        maxY = Mathf.Max(maxY, y);
        minY = Mathf.Min(minY, y);
        return y;
    }
}