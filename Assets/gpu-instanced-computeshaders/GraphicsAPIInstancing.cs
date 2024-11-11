using UnityEngine;

public class GraphicsAPIInstancing : MonoBehaviour
{
     public Mesh instanceMesh;
    public Material instanceMaterial;
    private Matrix4x4[] matrices;
    private const int TOTAL_INSTANCES = 50000;
    
    private MaterialPropertyBlock propertyBlock;
    private ComputeBuffer propertyBuffer;
    
    private struct InstanceData
    {
        public Vector4 color;
    }

    void Start()
    {
        // Initialize matrices array
        matrices = new Matrix4x4[TOTAL_INSTANCES];
        
        // Position instances in a more spread out grid
        for (int i = 0; i < TOTAL_INSTANCES; i++)
        {
            Vector3 position = new Vector3(
                (i % 100) * 3.0f,  // Increased spacing
                0,
                (i / 10) * 3.0f   // Increased spacing
            );
            
            matrices[i] = Matrix4x4.TRS(
                position,
                Quaternion.identity,
                Vector3.one
            );
        }

        // Setup buffer
        propertyBlock = new MaterialPropertyBlock();
        propertyBuffer = new ComputeBuffer(TOTAL_INSTANCES, 4 * sizeof(float));
        
        UpdateColors();
    }

    void UpdateColors()
    {
        var instanceData = new InstanceData[TOTAL_INSTANCES];
        
        // Make each instance a different color for debugging
        for (int i = 0; i < TOTAL_INSTANCES; i++)
        {
            float t = i / (float)TOTAL_INSTANCES;
            instanceData[i].color = Color.HSVToRGB(t*0.5f, 1, 1);
        }
        
        propertyBuffer.SetData(instanceData);
        propertyBlock.SetBuffer("_InstanceData", propertyBuffer);
    }

    void Update()
    {
        // Make sure the property block is set
        Graphics.DrawMeshInstanced(instanceMesh, 0, instanceMaterial, matrices, matrices.Length, propertyBlock);
    }

    void OnDestroy()
    {
        if (propertyBuffer != null)
        {
            propertyBuffer.Release();
            propertyBuffer = null;
        }
    }

    // Add this to verify instances are being created
    void OnDrawGizmos()
    {
        if (matrices != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var matrix in matrices)
            {
                Gizmos.DrawWireSphere(matrix.GetColumn(3), 0.5f);
            }
        }
    }
}