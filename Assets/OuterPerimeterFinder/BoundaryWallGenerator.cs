using UnityEngine;

public class BoundaryWallGenerator : MonoBehaviour
{
    [Header("Wall Settings")]
    [SerializeField] private float wallHeight = 2f;
    [SerializeField] private float wallThickness = 0.5f;
    [SerializeField] private Material wallMaterial;
    [SerializeField] private Vector3[] loopPoints;
    [SerializeField] private bool generateOnStart = true;

    private void Start()
    {
        if (generateOnStart && loopPoints != null && loopPoints.Length > 2)
        {
            GenerateWalls();
        }
    }

    public void GenerateWalls()
    {
        // Create a parent object for all walls
        GameObject wallsParent = new GameObject("BoundaryWalls");
        wallsParent.transform.parent = transform;

        // Generate walls between each consecutive pair of points
        for (int i = 0; i < loopPoints.Length; i++)
        {
            Vector3 startPoint = loopPoints[i];
            Vector3 endPoint = loopPoints[(i + 1) % loopPoints.Length]; // Use modulo to connect back to start

            CreateWallSegment(startPoint, endPoint, wallsParent.transform);
        }
    }

    private void CreateWallSegment(Vector3 start, Vector3 end, Transform parent)
    {
        // Create wall GameObject
        GameObject wall = new GameObject($"Wall_Segment_{start}_{end}");
        wall.transform.parent = parent;

        // Add mesh components
        MeshFilter meshFilter = wall.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = wall.AddComponent<MeshRenderer>();
        BoxCollider boxCollider = wall.AddComponent<BoxCollider>();

        // Calculate wall properties
        Vector3 wallDirection = end - start;
        float wallLength = wallDirection.magnitude;
        Vector3 wallCenter = start + (wallDirection / 2f);

        // Position and rotate the wall
        wall.transform.position = wallCenter;
        wall.transform.rotation = Quaternion.LookRotation(wallDirection);

        // Create wall mesh
        Mesh mesh = new Mesh();
        
        // Define vertices for a simple cube mesh
        Vector3[] vertices = new Vector3[]
        {
            // Front face
            new Vector3(-wallLength/2, 0, -wallThickness/2),
            new Vector3(wallLength/2, 0, -wallThickness/2),
            new Vector3(wallLength/2, wallHeight, -wallThickness/2),
            new Vector3(-wallLength/2, wallHeight, -wallThickness/2),
            
            // Back face
            new Vector3(-wallLength/2, 0, wallThickness/2),
            new Vector3(wallLength/2, 0, wallThickness/2),
            new Vector3(wallLength/2, wallHeight, wallThickness/2),
            new Vector3(-wallLength/2, wallHeight, wallThickness/2)
        };

        // Define triangles
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

        // Set mesh data
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        // Assign mesh and material
        meshFilter.mesh = mesh;
        meshRenderer.material = wallMaterial;

        // Update collider
        boxCollider.center = Vector3.up * (wallHeight / 2f);
        boxCollider.size = new Vector3(wallLength, wallHeight, wallThickness);
    }

    // Helper method to set loop points from code
    public void SetLoopPoints(Vector3[] points)
    {
        loopPoints = points;
    }
}