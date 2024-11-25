using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class BoundaryMeshGeneratorV3 : MonoBehaviour
{
    [Header("Boundary Settings")]
    public Vector3[] boundaryPoints;

    [Header("Mesh Settings")]
    [Range(1, 20)]
    public int innerRings = 5;
    [Range(0f, 1f)]
    public float smoothingFactor = 0.5f;
    [Range(0f, 1f)]
    public float holeSize = 0.5f; // Controls the size of the inner hole

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

    Vector3 centroid = CalculateCentroid();

    // Add outer boundary points first
    vertices.AddRange(boundaryPoints);

    // Generate inner rings
    for (int ring = 1; ring <= innerRings; ring++)
    {
        float t = (float)ring / innerRings;
        // Adjust t to create a hole (won't go all the way to center)
        float adjustedT = Mathf.Lerp(0f, holeSize, t);
        
        for (int i = 0; i < boundaryPoints.Length; i++)
        {
            Vector3 currentPoint = boundaryPoints[i];
            Vector3 nextPoint = boundaryPoints[(i + 1) % boundaryPoints.Length];
            Vector3 prevPoint = boundaryPoints[(i - 1 + boundaryPoints.Length) % boundaryPoints.Length];

            // Calculate direction from centroid to current point
            Vector3 direction = (currentPoint - centroid).normalized;
            float distanceToCentroid = Vector3.Distance(currentPoint, centroid);
            
            // Calculate ring point position
            Vector3 ringPoint = Vector3.Lerp(currentPoint, 
                centroid + direction * (distanceToCentroid * holeSize), 
                adjustedT);

            // Add smoothing
            if (smoothingFactor > 0)
            {
                Vector3 neighborEffect = (nextPoint + prevPoint - 2 * currentPoint) * 
                                       smoothingFactor * (1 - t) * 0.25f;
                ringPoint += neighborEffect;
            }

            vertices.Add(ringPoint);
        }
    }

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

    // Connect rings
    for (int ring = 0; ring < innerRings; ring++)
    {
        int currentRingStart = ring * numBoundaryPoints;
        int nextRingStart = (ring + 1) * numBoundaryPoints;

        for (int i = 0; i < numBoundaryPoints; i++)
        {
            int nextI = (i + 1) % numBoundaryPoints;

            if (ring == 0)
            {
                // Connect outer boundary to first ring
                triangles.Add(i);                          // Outer current
                triangles.Add(nextI);                      // Outer next
                triangles.Add(numBoundaryPoints + i);      // Inner current

                triangles.Add(numBoundaryPoints + i);      // Inner current
                triangles.Add(nextI);                      // Outer next
                triangles.Add(numBoundaryPoints + nextI);  // Inner next
            }
            else
            {
                // Connect between rings
                int curr1 = currentRingStart + i;
                int curr2 = currentRingStart + nextI;
                int next1 = nextRingStart + i;
                int next2 = nextRingStart + nextI;

                // First triangle
                triangles.Add(curr1);    // Current ring, current point
                triangles.Add(curr2);    // Current ring, next point
                triangles.Add(next1);    // Next ring, current point

                // Second triangle
                triangles.Add(next1);    // Next ring, current point
                triangles.Add(curr2);    // Current ring, next point
                triangles.Add(next2);    // Next ring, next point
            }
        }
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

    public void UpdateBoundaryPoints(Vector3[] newPoints)
    {
        boundaryPoints = newPoints;
        GenerateMesh();
    }

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