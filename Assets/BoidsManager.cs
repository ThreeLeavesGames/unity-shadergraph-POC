using UnityEngine;

public class BoidsManager : MonoBehaviour
{
    public Mesh birdMesh;
    public Material birdMaterial;
    private Matrix4x4[] matrices;
    private Vector3[] positions;
    private Vector3[] velocities;
    private int instanceCount = 500;

    // Boids parameters
    public float speed = 2f;
    public float separationDistance = 2f;
    public float alignmentDistance = 5f;
    public float cohesionDistance = 5f;

    void Start()
    {
        matrices = new Matrix4x4[instanceCount];
        positions = new Vector3[instanceCount];
        velocities = new Vector3[instanceCount];

        // Initialize positions and velocities
        for (int i = 0; i < instanceCount; i++)
        {
            Vector3 position = new Vector3(Random.Range(-50, 50), Random.Range(-5, 5), Random.Range(-50, 50));
            positions[i] = position;
            velocities[i] = Random.insideUnitSphere; // Random initial velocity
            matrices[i] = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
        }
    }

    void Update()
    {
        for (int i = 0; i < instanceCount; i++)
        {
            Vector3 steeringForce = Vector3.zero;

            // Calculate steering forces based on other boids
            steeringForce += CalculateSeparation(i) * 1.5f; // Weight for separation
            steeringForce += CalculateAlignment(i) * 1.0f; // Weight for alignment
            steeringForce += CalculateCohesion(i) * 1.0f; // Weight for cohesion

            // Update velocity and clamp to max speed
            velocities[i] += steeringForce;
            velocities[i] = Vector3.ClampMagnitude(velocities[i], speed);
            
            // Update position
            positions[i] += velocities[i] * Time.deltaTime;

            // Update the transformation matrix for rendering
            matrices[i] = Matrix4x4.TRS(positions[i], Quaternion.identity, Vector3.one);
        }

        // Draw all instances
        Graphics.DrawMeshInstanced(birdMesh, 0, birdMaterial, matrices);
    }

    private Vector3 CalculateSeparation(int index)
    {
        Vector3 steering = Vector3.zero;
        int count = 0;

        for (int i = 0; i < instanceCount; i++)
        {
            if (i != index)
            {
                float distance = Vector3.Distance(positions[index], positions[i]);
                if (distance < separationDistance)
                {
                    Vector3 direction = (positions[index] - positions[i]).normalized;
                    steering += direction / distance; // Weight by distance
                    count++;
                }
            }
        }

        if (count > 0)
            steering /= count;

        return steering;
    }

    private Vector3 CalculateAlignment(int index)
    {
        Vector3 steering = Vector3.zero;
        int count = 0;

        for (int i = 0; i < instanceCount; i++)
        {
            if (i != index)
            {
                float distance = Vector3.Distance(positions[index], positions[i]);
                if (distance < alignmentDistance)
                {
                    steering += velocities[i]; // Add the velocity of nearby boids
                    count++;
                }
            }
        }

        if (count > 0)
        {
            steering /= count; // Average velocity
            steering = (steering - velocities[index]).normalized; // Steer towards the average
        }

        return steering;
    }

    private Vector3 CalculateCohesion(int index)
    {
        Vector3 steering = Vector3.zero;
        int count = 0;

        for (int i = 0; i < instanceCount; i++)
        {
            if (i != index)
            {
                float distance = Vector3.Distance(positions[index], positions[i]);
                if (distance < cohesionDistance)
                {
                    steering += positions[i]; // Add the position of nearby boids
                    count++;
                }
            }
        }

        if (count > 0)
        {
            steering /= count; // Average position
            steering = (steering - positions[index]).normalized; // Steer towards the average
        }

        return steering;
    }
}
