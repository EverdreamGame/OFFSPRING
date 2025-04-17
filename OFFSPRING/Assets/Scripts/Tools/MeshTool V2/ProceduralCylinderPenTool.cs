using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[ExecuteAlways]
public class ProceduralCylinderPenTool : MonoBehaviour
{
    [Header("Bezier Path Reference")]
    public BezierPath bezierPath;

    [Header("Tube Settings")]
    public float radius = 0.5f;
    public int radialSegments = 16;
    public int pathResolution = 20; // Segments per Bezier segment

    private Mesh mesh;
    private Vector3[] vertices;
    private Vector3[] normals;
    private Vector2[] uv;
    private int[] triangles;

    private void Start()
    {
        GenerateTubeAlongBezier();
    }

    private void OnValidate()
    {
        GenerateTubeAlongBezier();
    }

    private void OnEnable()
    {
        if (bezierPath != null)
        {
            bezierPath.OnPathChanged += GenerateTubeAlongBezier;
        }
    }

    private void OnDisable()
    {
        if (bezierPath != null)
        {
            bezierPath.OnPathChanged -= GenerateTubeAlongBezier;
        }
    }

    private void GenerateTubeAlongBezier()
    {
        if (bezierPath == null || bezierPath.segments.Count == 0)
        {
            Debug.LogWarning("BezierPath is not assigned or empty.");
            return;
        }

        List<Vector3> pathPoints = new List<Vector3>();
        List<Vector3> pathTangents = new List<Vector3>();
        List<float> pathRadii = new List<float>();

        foreach (var segment in bezierPath.segments)
        {
            for (int i = 0; i <= pathResolution; i++)
            {
                float t = (float)i / pathResolution;
                pathPoints.Add(segment.GetPoint(t));
                pathTangents.Add(segment.GetTangent(t).normalized);
                pathRadii.Add(segment.GetWidth(t));
            }
        }

        GenerateTubeMesh(pathPoints, pathTangents, pathRadii);
    }

    private void GenerateTubeMesh(List<Vector3> centers, List<Vector3> tangents, List<float> radii)
    {
        int ringCount = centers.Count;
        int vertexCount = ringCount * (radialSegments + 1);
        int triangleCount = (ringCount - 1) * radialSegments * 6;

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "BezierTube";
            meshFilter.sharedMesh = mesh;
        }
        else
        {
            mesh.Clear();
        }

        vertices = new Vector3[vertexCount];
        normals = new Vector3[vertexCount];
        uv = new Vector2[vertexCount];
        triangles = new int[triangleCount];

        float[] curveLengths = new float[ringCount];
        float totalLength = 0f;
        curveLengths[0] = 0f;

        for (int i = 1; i < ringCount; i++)
        {
            float dist = Vector3.Distance(centers[i - 1], centers[i]);
            totalLength += dist;
            curveLengths[i] = totalLength;
        }

        for (int i = 0; i < ringCount; i++)
        {
            Vector3 forward = tangents[i];
            Vector3 normal = Vector3.up;

            if (Vector3.Dot(forward, normal) > 0.99f)
                normal = Vector3.right;

            Vector3 side = Vector3.Cross(forward, normal).normalized;
            normal = Vector3.Cross(side, forward).normalized;

            for (int j = 0; j <= radialSegments; j++)
            {
                float angle = (float)j / radialSegments * Mathf.PI * 2f;
                Vector3 radialDir = Mathf.Cos(angle) * side + Mathf.Sin(angle) * normal;
                float ringRadius = radii[i];
                Vector3 vertex = centers[i] + radialDir * ringRadius;

                int index = i * (radialSegments + 1) + j;
                vertices[index] = vertex;
                normals[index] = radialDir;

                float u = (float)j / radialSegments;
                float v = curveLengths[i] / totalLength; 

                uv[index] = new Vector2(u, v); 
            }
        }

        int triIndex = 0;
        for (int i = 0; i < ringCount - 1; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
                int a = i * (radialSegments + 1) + j;
                int b = a + radialSegments + 1;

                triangles[triIndex++] = a;
                triangles[triIndex++] = b;
                triangles[triIndex++] = b + 1;

                triangles[triIndex++] = a;
                triangles[triIndex++] = b + 1;
                triangles[triIndex++] = a + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }
}