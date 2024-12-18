using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class SplineMeshExtruder : MonoBehaviour
{
    public Mesh profileMesh; // The mesh to extrude
    public float scale = 1f;
    public int segments = 50;

    private SplineContainer splineContainer;
    private MeshFilter meshFilter;
    
    void Start()
    {
        splineContainer = GetComponent<SplineContainer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        
        ExtrudeMesh();
    }

    void ExtrudeMesh()
    {
        if (profileMesh == null || splineContainer == null) return;

        Spline spline = splineContainer.Spline;
        Vector3[] profile = profileMesh.vertices;
        
        // Create vertices and triangles arrays
        Vector3[] vertices = new Vector3[profile.Length * segments];
        int[] triangles = new int[(profileMesh.triangles.Length) * (segments - 1)];

        // Generate vertices along spline
        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector3 position = spline.EvaluatePosition(t);
            Quaternion rotation = Quaternion.LookRotation(
                spline.EvaluateTangent(t),
                Vector3.up
            );

            // Transform profile vertices
            for (int j = 0; j < profile.Length; j++)
            {
                vertices[i * profile.Length + j] = 
                    position + (rotation * (profile[j] * scale));
            }
        }

        // Generate triangles
        int triIndex = 0;
        for (int i = 0; i < segments - 1; i++)
        {
            for (int j = 0; j < profileMesh.triangles.Length; j += 3)
            {
                int baseIndex = i * profile.Length;
                triangles[triIndex] = baseIndex + profileMesh.triangles[j];
                triangles[triIndex + 1] = baseIndex + profileMesh.triangles[j + 1];
                triangles[triIndex + 2] = baseIndex + profileMesh.triangles[j + 2];
                triIndex += 3;
            }
        }

        // Create and assign mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        meshFilter.mesh = mesh;
    }
}