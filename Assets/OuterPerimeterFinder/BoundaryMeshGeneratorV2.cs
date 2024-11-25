using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class BoundaryMeshGeneratorV2 : MonoBehaviour
{
    [Header("Boundary Settings")]
    public Vector3[] boundaryPoints;

    [Header("Mesh Settings")]
    [Range(1, 20)]
    public int innerRings = 5;
    [Range(0f, 1f)]
    public float smoothingFactor = 0.5f;

    private Mesh mesh;
    private List<Vector3> vertices;
    private List<int> triangles;
    private List<Vector2> uvs;

    void Start()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        if (boundaryPoints == null || boundaryPoints.Length < 3)
        {
            Debug.LogError("Need at least 3 boundary points to generate mesh!");
            return;
        }

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();

        // Important: Use the exact boundary points without any transformation
        vertices.AddRange(boundaryPoints);

        Vector3 centroid = CalculateCentroid();

        // Generate inner rings
        for (int ring = 1; ring <= innerRings; ring++)
        {
            float t = (float)ring / (innerRings + 1);
            t = Mathf.Pow(t, smoothingFactor);

            for (int i = 0; i < boundaryPoints.Length; i++)
            {
                Vector3 currentPoint = boundaryPoints[i];
                Vector3 nextPoint = boundaryPoints[(i + 1) % boundaryPoints.Length];
                Vector3 prevPoint = boundaryPoints[(i - 1 + boundaryPoints.Length) % boundaryPoints.Length];

                // Calculate internal points while maintaining exact positions
                Vector3 ringPoint = Vector3.Lerp(currentPoint, centroid, t);
                
                // Add smoothing while preserving position
                Vector3 neighborEffect = (nextPoint + prevPoint - 2 * currentPoint) * smoothingFactor * (1 - t) * 0.25f;
                ringPoint += neighborEffect;

                vertices.Add(ringPoint);
            }
        }

        // Add centroid as final vertex
        vertices.Add(centroid);

        GenerateTriangles();
        GenerateUVs();
        UpdateMesh();
    }

    Vector3 CalculateCentroid()
    {
        Vector3 centroid = Vector3.zero;
        foreach (Vector3 point in boundaryPoints)
        {
            centroid += point;
        }
        return centroid / boundaryPoints.Length;
    }

 void GenerateTriangles()
{
    int numBoundaryPoints = boundaryPoints.Length;

    // Connect outer boundary to first inner ring
    for (int i = 0; i < numBoundaryPoints; i++)
    {
        int nextI = (i + 1) % numBoundaryPoints;
        int ringStartIdx = numBoundaryPoints;

        // Reversed order of vertices
        triangles.Add(i);
        triangles.Add(nextI);
        triangles.Add(ringStartIdx + i);

        triangles.Add(ringStartIdx + i);
        triangles.Add(nextI);
        triangles.Add(ringStartIdx + nextI);
    }

    // Connect inner rings
    for (int ring = 0; ring < innerRings - 1; ring++)
    {
        int ringStartIdx = numBoundaryPoints + (ring * numBoundaryPoints);
        int nextRingStartIdx = ringStartIdx + numBoundaryPoints;

        for (int i = 0; i < numBoundaryPoints; i++)
        {
            int nextI = (i + 1) % numBoundaryPoints;

            // Reversed order of vertices
            triangles.Add(ringStartIdx + i);
            triangles.Add(ringStartIdx + nextI);
            triangles.Add(nextRingStartIdx + i);

            triangles.Add(nextRingStartIdx + i);
            triangles.Add(ringStartIdx + nextI);
            triangles.Add(nextRingStartIdx + nextI);
        }
    }

    // Connect innermost ring to centroid
    int lastRingStartIdx = numBoundaryPoints + ((innerRings - 1) * numBoundaryPoints);
    int centroidIdx = vertices.Count - 1;

    for (int i = 0; i < numBoundaryPoints; i++)
    {
        int nextI = (i + 1) % numBoundaryPoints;
        // Reversed order of vertices
        triangles.Add(lastRingStartIdx + i);
        triangles.Add(lastRingStartIdx + nextI);
        triangles.Add(centroidIdx);
    }
}

    void GenerateUVs()
    {
        Vector3 bounds = CalculateBounds();
        foreach (Vector3 vertex in vertices)
        {
            float u = Mathf.InverseLerp(-bounds.x, bounds.x, vertex.x);
            float v = Mathf.InverseLerp(-bounds.z, bounds.z, vertex.z);
            uvs.Add(new Vector2(u, v));
        }
    }

    Vector3 CalculateBounds()
    {
        Vector3 min = boundaryPoints[0];
        Vector3 max = boundaryPoints[0];
        
        foreach (Vector3 point in boundaryPoints)
        {
            min = Vector3.Min(min, point);
            max = Vector3.Max(max, point);
        }
        
        return max - min;
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
    }

    // Utility method to update mesh at runtime
    public void UpdateBoundaryPoints(Vector3[] newPoints)
    {
        boundaryPoints = newPoints;
        GenerateMesh();
    }

    // Visualize points in Scene view
    void OnDrawGizmosSelected()
    {
        if (boundaryPoints == null || boundaryPoints.Length < 2) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < boundaryPoints.Length; i++)
        {
            Vector3 current = boundaryPoints[i];
            Vector3 next = boundaryPoints[(i + 1) % boundaryPoints.Length];
            Gizmos.DrawLine(current, next);
            Gizmos.DrawSphere(current, 0.1f);
        }
    }
}