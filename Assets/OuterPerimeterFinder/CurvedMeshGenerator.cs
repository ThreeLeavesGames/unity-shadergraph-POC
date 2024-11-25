using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CurvedMeshGenerator : MonoBehaviour
{
    [Header("Mesh Settings")]
    [Range(2, 50)]
    public int subdivisions = 20; // Controls mesh density
    public float width = 5f;
    public float height = 5f;
    public float curvature = 1f; // Controls how much the mesh curves
    
    [Header("Animation")]
    public bool animate = false;
    public float waveSpeed = 1f;
    public float waveAmplitude = 0.1f;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;

    void Start()
    {
        GenerateMesh();
    }

  
    void GenerateMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateVertices();
        CreateTriangles();
        CreateUVs();
        UpdateMesh();
    }

    void CreateVertices()
    {
        vertices = new Vector3[(subdivisions + 1) * (subdivisions + 1)];

        for (int i = 0, z = 0; z <= subdivisions; z++)
        {
            for (int x = 0; x <= subdivisions; x++)
            {
                float xPos = ((float)x / subdivisions - 0.5f) * width;
                float zPos = ((float)z / subdivisions - 0.5f) * height;
                
                // Create the curved surface using a quadratic function
                float yPos = CalculateCurvedHeight(xPos, zPos);
                
                vertices[i] = new Vector3(xPos, yPos, zPos);
                i++;
            }
        }
    }

    float CalculateCurvedHeight(float x, float z)
    {
        // Create a sail-like curve using a combination of quadratic and sine functions
        float normalizedX = x / width;
        float normalizedZ = z / height;
        
        float curve = curvature * (
            Mathf.Pow(normalizedX, 2) + 
            Mathf.Pow(normalizedZ, 2) - 
            (normalizedX * normalizedZ)
        );
        
        return curve;
    }

    void CreateTriangles()
    {
        triangles = new int[subdivisions * subdivisions * 6];
        int vert = 0;
        int tris = 0;

        for (int z = 0; z < subdivisions; z++)
        {
            for (int x = 0; x < subdivisions; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + subdivisions + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + subdivisions + 1;
                triangles[tris + 5] = vert + subdivisions + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    void CreateUVs()
    {
        uvs = new Vector2[vertices.Length];

        for (int i = 0, z = 0; z <= subdivisions; z++)
        {
            for (int x = 0; x <= subdivisions; x++)
            {
                uvs[i] = new Vector2((float)x / subdivisions, (float)z / subdivisions);
                i++;
            }
        }
    }


    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }
}