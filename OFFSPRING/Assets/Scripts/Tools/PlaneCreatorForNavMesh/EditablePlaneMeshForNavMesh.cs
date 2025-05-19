using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EditablePlaneMeshForNavMesh : MonoBehaviour
{
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();

    private Mesh mesh;
    private MeshFilter meshFilter;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (!meshFilter)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        var meshRenderer = GetComponent<MeshRenderer>();
        if (!meshRenderer)
            gameObject.AddComponent<MeshRenderer>();

        if (vertices.Count < 4)
            CreateInitialSquare();

        RecalculateMesh();
    }

    void CreateInitialSquare()
    {
        vertices.Clear();
        vertices.Add(new Vector3(-0.5f, 0, -0.5f));
        vertices.Add(new Vector3(0.5f, 0, -0.5f));
        vertices.Add(new Vector3(0.5f, 0, 0.5f));
        vertices.Add(new Vector3(-0.5f, 0, 0.5f));

        triangles = new List<int> { 0, 1, 2, 2, 3, 0 };
    }

    public void RecalculateMesh()
    {
        mesh = new Mesh();
        mesh.name = "NavMesh";
        mesh.vertices = vertices.ToArray();

        if (vertices.Count >= 3)
        {
            // Basic naive triangle fan for now
            triangles.Clear();
            for (int i = 1; i < vertices.Count - 1; i++)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }
        }

        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
}
