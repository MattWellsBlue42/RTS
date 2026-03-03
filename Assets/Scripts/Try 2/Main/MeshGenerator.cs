using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public Mesh GenerateMesh(Vector3[] vertices, int[] triangles)
    {
        Mesh mesh = new()
        {
            vertices = vertices,
            triangles = triangles
        };
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }
}