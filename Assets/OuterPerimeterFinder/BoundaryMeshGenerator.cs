using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class BoundaryMeshGenerator : MonoBehaviour
{
    [Header("Boundary Settings")]
    public Vector3[] boundaryPoints; // The loop of points defining the boundary
    
    [Header("Mesh Settings")]
    [Range(1, 20)]
    public int innerRings = 5; // Number of internal rings for mesh density
    [Range(0f, 1f)]
    public float smoothingFactor = 0.5f; // Controls how smooth the internal points are

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

        GenerateVertices();
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

    void GenerateVertices()
    {
        // Add boundary points first
        vertices.AddRange(boundaryPoints);

        Vector3 centroid = CalculateCentroid();
        
        // Generate inner rings
        for (int ring = 1; ring <= innerRings; ring++)
        {
            float t = (float)ring / (innerRings + 1);
            t = Mathf.Pow(t, smoothingFactor); // Apply smoothing to ring distribution
            
            // Generate points for this ring
            for (int i = 0; i < boundaryPoints.Length; i++)
            {
                Vector3 currentPoint = boundaryPoints[i];
                Vector3 nextPoint = boundaryPoints[(i + 1) % boundaryPoints.Length];
                Vector3 prevPoint = boundaryPoints[(i - 1 + boundaryPoints.Length) % boundaryPoints.Length];
                
                // Calculate smooth internal point
                Vector3 direction = (nextPoint - prevPoint).normalized;
                Vector3 normal = Vector3.Cross(direction, Vector3.up).normalized;
                
                Vector3 ringPoint = Vector3.Lerp(currentPoint, centroid, t);
                // Add some smoothing based on neighboring points
                ringPoint += (nextPoint + prevPoint - 2 * currentPoint) * smoothingFactor * (1 - t) * 0.25f;
                
                vertices.Add(ringPoint);
            }
        }

        // Add centroid as final point
        vertices.Add(centroid);
    }

    void GenerateTriangles()
    {
        int numBoundaryPoints = boundaryPoints.Length;
        
        // Generate triangles for outer ring
        for (int i = 0; i < numBoundaryPoints; i++)
        {
            int nextI = (i + 1) % numBoundaryPoints;
            int ringStartIdx = numBoundaryPoints;
            
            // Connect boundary to first inner ring
            triangles.Add(i);
            triangles.Add(ringStartIdx + i);
            triangles.Add(nextI);

            triangles.Add(ringStartIdx + i);
            triangles.Add(ringStartIdx + nextI);
            triangles.Add(nextI);
        }

        // Generate triangles between inner rings
        for (int ring = 0; ring < innerRings - 1; ring++)
        {
            int ringStartIdx = numBoundaryPoints + (ring * numBoundaryPoints);
            int nextRingStartIdx = ringStartIdx + numBoundaryPoints;

            for (int i = 0; i < numBoundaryPoints; i++)
            {
                int nextI = (i + 1) % numBoundaryPoints;
                
                triangles.Add(ringStartIdx + i);
                triangles.Add(nextRingStartIdx + i);
                triangles.Add(ringStartIdx + nextI);

                triangles.Add(nextRingStartIdx + i);
                triangles.Add(nextRingStartIdx + nextI);
                triangles.Add(ringStartIdx + nextI);
            }
        }

        // Connect innermost ring to centroid
        int lastRingStartIdx = numBoundaryPoints + ((innerRings - 1) * numBoundaryPoints);
        int centroidIdx = vertices.Count - 1;

        for (int i = 0; i < numBoundaryPoints; i++)
        {
            int nextI = (i + 1) % numBoundaryPoints;
            
            triangles.Add(lastRingStartIdx + i);
            triangles.Add(centroidIdx);
            triangles.Add(lastRingStartIdx + nextI);
        }
    }

    void GenerateUVs()
    {
        // Calculate UVs based on point positions
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

    // Helper method to set boundary points at runtime
    public void SetBoundaryPoints(Vector3[] points)
    {
        boundaryPoints = points;
        GenerateMesh();
    }

    // Optional: Visualize the boundary points in the editor
    void OnDrawGizmos()
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