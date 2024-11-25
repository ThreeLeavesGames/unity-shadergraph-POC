using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class BoundaryMeshGeneratorV4 : MonoBehaviour
{
  [Header("Boundary Settings")]
    public Vector3[] boundaryPoints;

    [Header("Mesh Settings")]
    [Range(1, 20)]
    public int innerRings = 5;
    [Range(0f, 1f)]
    public float smoothingFactor = 0.5f;
    [Range(0f, 1f)]
    public float holeSize = 0.5f;
    [Range(0.01f, 5f)]
    public float thickness = 0.5f; // Controls the thickness of the mesh

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
        Vector3 normal = CalculateNormal(boundaryPoints);

        // Generate top vertices
        GenerateRings(normal * thickness / 2);
        
        // Generate bottom vertices
        GenerateRings(-normal * thickness / 2);

        GenerateTriangles();
        GenerateUVs();
        UpdateMesh();
    }

    void GenerateRings(Vector3 offset)
    {
        Vector3 centroid = CalculateCentroid();
        
        // Add boundary points
        for (int i = 0; i < boundaryPoints.Length; i++)
        {
            vertices.Add(boundaryPoints[i] + offset);
        }

        // Generate inner rings
        for (int ring = 1; ring <= innerRings; ring++)
        {
            float t = (float)ring / innerRings;
            float adjustedT = Mathf.Lerp(0f, holeSize, t);
            
            for (int i = 0; i < boundaryPoints.Length; i++)
            {
                Vector3 currentPoint = boundaryPoints[i];
                Vector3 nextPoint = boundaryPoints[(i + 1) % boundaryPoints.Length];
                Vector3 prevPoint = boundaryPoints[(i - 1 + boundaryPoints.Length) % boundaryPoints.Length];

                Vector3 direction = (currentPoint - centroid).normalized;
                float distanceToCentroid = Vector3.Distance(currentPoint, centroid);
                
                Vector3 ringPoint = Vector3.Lerp(currentPoint, 
                    centroid + direction * (distanceToCentroid * holeSize), 
                    adjustedT);

                if (smoothingFactor > 0)
                {
                    Vector3 neighborEffect = (nextPoint + prevPoint - 2 * currentPoint) * 
                                           smoothingFactor * (1 - t) * 0.25f;
                    ringPoint += neighborEffect;
                }

                vertices.Add(ringPoint + offset);
            }
        }
    }

    Vector3 CalculateNormal(Vector3[] points)
    {
        if (points.Length < 3) return Vector3.up;
        
        Vector3 v1 = points[1] - points[0];
        Vector3 v2 = points[2] - points[0];
        return Vector3.Cross(v1, v2).normalized;
    }

    void GenerateTriangles()
    {
        int numBoundaryPoints = boundaryPoints.Length;
        int vertsPerLayer = numBoundaryPoints * (innerRings + 1);
        int bottomLayerStart = vertsPerLayer;

        // Generate triangles for top and bottom faces
        for (int layer = 0; layer < 2; layer++)
        {
            int layerOffset = layer * vertsPerLayer;
            bool isBottom = layer == 1;

            for (int ring = 0; ring < innerRings; ring++)
            {
                int currentRingStart = layerOffset + (ring * numBoundaryPoints);
                int nextRingStart = layerOffset + ((ring + 1) * numBoundaryPoints);

                for (int i = 0; i < numBoundaryPoints; i++)
                {
                    int nextI = (i + 1) % numBoundaryPoints;
                    
                    if (!isBottom)
                    {
                        // Top face triangles
                        triangles.Add(currentRingStart + i);
                        triangles.Add(nextRingStart + i);
                        triangles.Add(currentRingStart + nextI);

                        triangles.Add(currentRingStart + nextI);
                        triangles.Add(nextRingStart + i);
                        triangles.Add(nextRingStart + nextI);
                    }
                    else
                    {
                        // Bottom face triangles (reversed winding)
                        triangles.Add(currentRingStart + i);
                        triangles.Add(currentRingStart + nextI);
                        triangles.Add(nextRingStart + i);

                        triangles.Add(currentRingStart + nextI);
                        triangles.Add(nextRingStart + nextI);
                        triangles.Add(nextRingStart + i);
                    }
                }
            }
        }

        // Generate side triangles connecting top and bottom
        for (int i = 0; i < numBoundaryPoints; i++)
        {
            for (int ring = 0; ring < innerRings; ring++)
            {
                int topCurrentRing = ring * numBoundaryPoints + i;
                int topNextRing = ((ring + 1) * numBoundaryPoints) + i;
                int bottomCurrentRing = bottomLayerStart + (ring * numBoundaryPoints) + i;
                int bottomNextRing = bottomLayerStart + ((ring + 1) * numBoundaryPoints) + i;

                // Side triangles
                // First triangle
                triangles.Add(topCurrentRing);
                triangles.Add(bottomCurrentRing);
                triangles.Add(topNextRing);

                // Second triangle
                triangles.Add(bottomCurrentRing);
                triangles.Add(bottomNextRing);
                triangles.Add(topNextRing);
            }
        }
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

    void GenerateUVs()
    {
        // Simple UV mapping
        foreach (Vector3 vertex in vertices)
        {
            uvs.Add(new Vector2(vertex.x, vertex.z));
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
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