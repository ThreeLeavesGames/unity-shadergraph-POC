using System.Collections.Generic;
using UnityEngine;

public class CurvedBoundaryWallGeneratorV2 : MonoBehaviour
{
    [Header("Wall Settings")]
    [SerializeField] private float wallHeight = 2f;
    [SerializeField] private float wallThickness = 0.5f;
    [SerializeField] private Material wallMaterial;
    [SerializeField] private int segmentsPerCurve = 10; // Higher number = smoother curves
    
    public Vector3[] controlPoints; // The main points that define the shape
    private Vector3[] smoothedPoints; // Points after curve interpolation
    [SerializeField] private float cornerAngleThreshold = 45f; // Angles sharper than this will get corner pieces
    [SerializeField] private float cornerSize = 1f; // Size of corner pieces
    
    [Header("Corner Piece Settings")]
    [SerializeField] private GameObject cornerPrefab; // Optional: Custom corner mesh
    [SerializeField] private bool useCustomCornerPiece = false;
    private List<Vector3> cornerPoints = new List<Vector3>();

    private void Start()
    {
        // Example control points matching your crown-like shape
        // Adjust these points to match your exact shape
        // controlPoints = new Vector3[]
        // {
        //     new Vector3(-2f, 0f, 0f),    // Left point
        //     new Vector3(-1f, 0f, 1f),    // Left curve control
        //     new Vector3(0f, 0f, 1.5f),   // Top point
        //     new Vector3(1f, 0f, 1f),     // Right curve control
        //     new Vector3(2f, 0f, 0f)      // Right point
        // };

        GenerateSmoothedPoints();
        GenerateWalls();
    }

    private void GenerateSmoothedPoints()
    {
        // Create a list to store the interpolated points
        smoothedPoints = new Vector3[segmentsPerCurve * (controlPoints.Length - 1)];
        
        // Generate points along the curve using Catmull-Rom spline
        for (int i = 0; i < controlPoints.Length - 1; i++)
        {
            Vector3 p0 = controlPoints[Mathf.Max(i - 1, 0)];
            Vector3 p1 = controlPoints[i];
            Vector3 p2 = controlPoints[i + 1];
            Vector3 p3 = controlPoints[Mathf.Min(i + 2, controlPoints.Length - 1)];

            for (int j = 0; j < segmentsPerCurve; j++)
            {
                float t = j / (float)segmentsPerCurve;
                int index = i * segmentsPerCurve + j;
                smoothedPoints[index] = CatmullRomPoint(p0, p1, p2, p3, t);
            }
        }
    }

    private Vector3 CatmullRomPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        Vector3 point = 0.5f * (
            (-t3 + 2f * t2 - t) * p0 +
            (3f * t3 - 5f * t2 + 2f) * p1 +
            (-3f * t3 + 4f * t2 + t) * p2 +
            (t3 - t2) * p3
        );

        return point;
    }

    private void GenerateWalls()
    {
        GameObject wallsParent = new GameObject("CurvedBoundaryWalls");
        wallsParent.transform.parent = transform;

        // Generate wall segments between each pair of smoothed points
        for (int i = 0; i < smoothedPoints.Length - 1; i++)
        {
            CreateWallSegment(smoothedPoints[i], smoothedPoints[i + 1], wallsParent.transform);
        }
        
        // Close the loop by connecting the last point to the first
        CreateWallSegment(smoothedPoints[smoothedPoints.Length - 1], smoothedPoints[0], wallsParent.transform);
    }

    private void CreateWallSegment(Vector3 start, Vector3 end, Transform parent)
    {
        GameObject wall = new GameObject($"Wall_Segment");
        wall.transform.parent = parent;

        MeshFilter meshFilter = wall.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = wall.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = wall.AddComponent<MeshCollider>();

        Vector3 wallDirection = end - start;
        float wallLength = wallDirection.magnitude;
        Vector3 wallCenter = start + (wallDirection / 2f);

        // Create wall mesh
        Mesh mesh = new Mesh();
        
        // Calculate the rotation to face the next point
        Quaternion rotation = Quaternion.LookRotation(wallDirection.normalized);
        
        // Generate vertices with proper curve following
        Vector3[] vertices = new Vector3[]
        {
            // Front face
            rotation * new Vector3(-wallLength/2, 0, -wallThickness/2),
            rotation * new Vector3(wallLength/2, 0, -wallThickness/2),
            rotation * new Vector3(wallLength/2, wallHeight, -wallThickness/2),
            rotation * new Vector3(-wallLength/2, wallHeight, -wallThickness/2),
            
            // Back face
            rotation * new Vector3(-wallLength/2, 0, wallThickness/2),
            rotation * new Vector3(wallLength/2, 0, wallThickness/2),
            rotation * new Vector3(wallLength/2, wallHeight, wallThickness/2),
            rotation * new Vector3(-wallLength/2, wallHeight, wallThickness/2),
        };

        // Adjust vertex positions to world space
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] += wallCenter;
        }

        int[] triangles = new int[]
        {
            // Front face
            0, 2, 1,
            0, 3, 2,
            // Back face
            5, 7, 4,
            5, 6, 7,
            // Top face
            3, 7, 6,
            3, 6, 2,
            // Bottom face
            0, 1, 5,
            0, 5, 4,
            // Left face
            0, 4, 7,
            0, 7, 3,
            // Right face
            1, 2, 6,
            1, 6, 5
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshRenderer.material = wallMaterial;
        meshCollider.sharedMesh = mesh;
    }

     private void CreateProceduralCornerPiece(Vector3 cornerPoint, Vector3 prevPoint, Vector3 nextPoint, Transform parent)
    {
        GameObject corner = new GameObject("Corner_Piece");
        corner.transform.parent = parent;
        corner.transform.position = cornerPoint;

        MeshFilter meshFilter = corner.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = corner.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = corner.AddComponent<MeshCollider>();

        // Calculate directions
        Vector3 dirFromPrev = (cornerPoint - prevPoint).normalized;
        Vector3 dirToNext = (nextPoint - cornerPoint).normalized;

        // Create rounded corner mesh
        Mesh mesh = CreateRoundedCornerMesh(dirFromPrev, dirToNext);

        meshFilter.mesh = mesh;
        meshRenderer.material = wallMaterial;
        meshCollider.sharedMesh = mesh;
    }
      private bool IsCornerPoint(Vector3 point)
    {
        return cornerPoints.Contains(point);
    }
     private void GenerateWallsWithCorners()
    {
        GameObject wallsParent = new GameObject("EnhancedBoundaryWalls");
        wallsParent.transform.parent = transform;

        // First pass: Detect corners
        DetectCorners();

        // Second pass: Generate walls between corners
        for (int i = 0; i < smoothedPoints.Length - 1; i++)
        {
            Vector3 current = smoothedPoints[i];
            Vector3 next = smoothedPoints[i + 1];

            if (IsCornerPoint(current))
            {
                CreateCornerPiece(current, smoothedPoints[Mathf.Max(0, i-1)], next, wallsParent.transform);
            }
            else
            {
                CreateWallSegment(current, next, wallsParent.transform);
            }
        }

        // Close the loop
        CreateWallSegment(smoothedPoints[smoothedPoints.Length - 1], smoothedPoints[0], wallsParent.transform);
    }
   private void DetectCorners()
    {
        cornerPoints.Clear();
        
        for (int i = 0; i < smoothedPoints.Length; i++)
        {
            Vector3 prev = smoothedPoints[(i - 1 + smoothedPoints.Length) % smoothedPoints.Length];
            Vector3 current = smoothedPoints[i];
            Vector3 next = smoothedPoints[(i + 1) % smoothedPoints.Length];

            Vector3 dirToPrev = (prev - current).normalized;
            Vector3 dirToNext = (next - current).normalized;

            float angle = Vector3.Angle(dirToPrev, dirToNext);

            if (angle < (180f - cornerAngleThreshold))
            {
                cornerPoints.Add(current);
            }
        }
    }
    private void CreateCornerPiece(Vector3 cornerPoint, Vector3 prevPoint, Vector3 nextPoint, Transform parent)
    {
        if (useCustomCornerPiece && cornerPrefab != null)
        {
            CreateCustomCornerPiece(cornerPoint, prevPoint, nextPoint, parent);
        }
        else
        {
            CreateProceduralCornerPiece(cornerPoint, prevPoint, nextPoint, parent);
        }
    }
      private void CreateCustomCornerPiece(Vector3 cornerPoint, Vector3 prevPoint, Vector3 nextPoint, Transform parent)
    {
        GameObject corner = Instantiate(cornerPrefab, cornerPoint, Quaternion.identity, parent);
        
        // Calculate the rotation to align with the walls
        Vector3 dirFromPrev = (cornerPoint - prevPoint).normalized;
        Vector3 dirToNext = (nextPoint - cornerPoint).normalized;
        Vector3 averageDir = ((dirFromPrev + dirToNext) / 2f).normalized;
        
        // Calculate rotation based on the average direction
        corner.transform.rotation = Quaternion.LookRotation(averageDir);
        
        // Scale the corner piece appropriately
        corner.transform.localScale = new Vector3(cornerSize, wallHeight, cornerSize);
    }
    private Mesh CreateRoundedCornerMesh(Vector3 dirFromPrev, Vector3 dirToNext)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        int segments = 8; // Number of segments in the rounded corner
        float radius = cornerSize / 2f;

        // Calculate the angle between the directions
        float angle = Vector3.SignedAngle(dirFromPrev, dirToNext, Vector3.up);
        
        // Create vertices for the rounded corner
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float currentAngle = Mathf.Deg2Rad * angle * t;
            
            // Calculate position on the arc
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * dirFromPrev;
            
            // Add vertices for both bottom and top of the wall
            vertices.Add(radius * direction); // Bottom
            vertices.Add(radius * direction + Vector3.up * wallHeight); // Top
        }

        // Create triangles
        for (int i = 0; i < segments; i++)
        {
            int bottomLeft = i * 2;
            int bottomRight = (i + 1) * 2;
            int topLeft = bottomLeft + 1;
            int topRight = bottomRight + 1;

            // Front face
            triangles.AddRange(new int[] {
                bottomLeft, topLeft, bottomRight,
                bottomRight, topLeft, topRight
            });
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
       // Helper method to set points
    public void SetPoints(Vector3[] points)
    {
        smoothedPoints = points;
        GenerateWallsWithCorners();
    }
}