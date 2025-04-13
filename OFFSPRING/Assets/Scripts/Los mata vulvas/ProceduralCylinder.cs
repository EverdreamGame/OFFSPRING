using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralCylinder : MonoBehaviour
{
    [Header("Cylinder Settings")]
    public float height = 2f;
    public float radius = 1f;
    public int radialSegments = 24;
    public float segmentHeight = 1f;

    private Mesh mesh;
    private Vector3[] vertices;
    private Vector3[] normals;
    private Vector2[] uv;
    private int[] triangles;

    private int lastRadialSegments;
    private int lastHeightSegments;
    private float lastHeight;
    private float lastRadius;
    [Space]
    public bool isSameDirection;

    private void Start()
    {
        int heightSegments = Mathf.Max(1, Mathf.RoundToInt(height / segmentHeight));

        GenerateCylinder(radialSegments, heightSegments, radius, height);
        lastRadialSegments = radialSegments;
        lastHeightSegments = heightSegments;
        lastHeight = height;
        lastRadius = radius;
    }

    public void UpdateMeshIfNeeded(float height, float maxHeight)
    {
        this.height = isSameDirection ? height : maxHeight - height;
        int heightSegments = Mathf.Max(1, Mathf.RoundToInt(height / segmentHeight));

        // Only regenerate if values have changed
        if (mesh == null ||
            radialSegments != lastRadialSegments ||
            heightSegments != lastHeightSegments ||
            height != lastHeight ||
            radius != lastRadius)
        {
            GenerateCylinder(radialSegments, heightSegments, radius, height);
            lastRadialSegments = radialSegments;
            lastHeightSegments = heightSegments;
            lastHeight = height;
            lastRadius = radius;
        }
    }

    public void GenerateCylinder(int radialSegments, int heightSegments, float radius, float height)
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "ProceduralCylinder";
            meshFilter.sharedMesh = mesh;
        }
        else
        {
            mesh.Clear(); // Reuse the existing mesh
        }

        int vertCount = (radialSegments + 1) * (heightSegments + 1);
        int triCount = radialSegments * heightSegments * 6;

        // Resize buffers only if needed
        if (vertices == null || vertices.Length != vertCount)
        {
            vertices = new Vector3[vertCount];
            normals = new Vector3[vertCount];
            uv = new Vector2[vertCount];
        }

        if (triangles == null || triangles.Length != triCount)
        {
            triangles = new int[triCount];
        }

        // Vertices
        for (int y = 0; y <= heightSegments; y++)
        {
            float v = (float)y / heightSegments;
            float yPos = v * height;

            for (int i = 0; i <= radialSegments; i++)
            {
                float u = (float)i / radialSegments;
                float angle = u * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;

                int index = y * (radialSegments + 1) + i;
                vertices[index] = new Vector3(x, yPos, z);
                normals[index] = new Vector3(x, 0f, z).normalized;
                uv[index] = new Vector2(u, v);
            }
        }

        // Triangles
        int triIndex = 0;
        for (int y = 0; y < heightSegments; y++)
        {
            for (int i = 0; i < radialSegments; i++)
            {
                int a = y * (radialSegments + 1) + i;
                int b = a + radialSegments + 1;
                int c = b + 1;
                int d = a + 1;

                triangles[triIndex++] = a;
                triangles[triIndex++] = b;
                triangles[triIndex++] = c;

                triangles[triIndex++] = a;
                triangles[triIndex++] = c;
                triangles[triIndex++] = d;
            }
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }
}