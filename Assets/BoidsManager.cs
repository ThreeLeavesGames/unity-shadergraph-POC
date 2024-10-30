using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class BoidsManager : MonoBehaviour
{
    public Mesh birdMesh;
    public Material birdMaterial;
    private Matrix4x4[] matrices;

    private NativeArray<Vector3> positions;
    private NativeArray<Vector3> velocities;
    private NativeArray<Vector3> separationForces;
    private NativeArray<Vector3> alignmentForces;
    private NativeArray<Vector3> cohesionForces;

    private int instanceCount = 5000;

    // Boids parameters
    public float speed = 2f;
    public float separationDistance = 2f;
    public float alignmentDistance = 5f;
    public float cohesionDistance = 5f;

    void Start()
    {
        matrices = new Matrix4x4[instanceCount];
        positions = new NativeArray<Vector3>(instanceCount, Allocator.Persistent);
        velocities = new NativeArray<Vector3>(instanceCount, Allocator.Persistent);
        separationForces = new NativeArray<Vector3>(instanceCount, Allocator.Persistent);
        alignmentForces = new NativeArray<Vector3>(instanceCount, Allocator.Persistent);
        cohesionForces = new NativeArray<Vector3>(instanceCount, Allocator.Persistent);

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
        // Schedule the Separation job
        SeparationJob separationJob = new SeparationJob
        {
            positions = positions,
            separationForces = separationForces,
            separationDistance = separationDistance,
            instanceCount = instanceCount
        };

        JobHandle separationHandle = separationJob.Schedule(instanceCount, 64);

        // Schedule the Alignment job
        AlignmentJob alignmentJob = new AlignmentJob
        {
            positions = positions,
            velocities = velocities,
            alignmentForces = alignmentForces,
            alignmentDistance = alignmentDistance,
            instanceCount = instanceCount
        };

        JobHandle alignmentHandle = alignmentJob.Schedule(instanceCount, 64, separationHandle);

        // Schedule the Cohesion job
        CohesionJob cohesionJob = new CohesionJob
        {
            positions = positions,
            cohesionForces = cohesionForces,
            cohesionDistance = cohesionDistance,
            instanceCount = instanceCount
        };

        JobHandle cohesionHandle = cohesionJob.Schedule(instanceCount, 64, alignmentHandle);

        // Ensure the job chain is completed
        cohesionHandle.Complete();

        // Apply the results and update positions
        for (int i = 0; i < instanceCount; i++)
        {
            Vector3 steeringForce = separationForces[i] * 1.5f + alignmentForces[i] * 1.0f + cohesionForces[i] * 1.0f;
            velocities[i] += steeringForce;
            velocities[i] = Vector3.ClampMagnitude(velocities[i], speed);

            positions[i] += velocities[i] * Time.deltaTime;
            matrices[i] = Matrix4x4.TRS(positions[i], Quaternion.identity, Vector3.one);
        }

        // Draw all instances
        Graphics.DrawMeshInstanced(birdMesh, 0, birdMaterial, matrices);
    }

    void OnDestroy()
    {
        // Dispose of the NativeArrays to avoid memory leaks
        if (positions.IsCreated) positions.Dispose();
        if (velocities.IsCreated) velocities.Dispose();
        if (separationForces.IsCreated) separationForces.Dispose();
        if (alignmentForces.IsCreated) alignmentForces.Dispose();
        if (cohesionForces.IsCreated) cohesionForces.Dispose();
    }

    // Job for calculating Separation forces
    [BurstCompile]
    struct SeparationJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> positions;
        public NativeArray<Vector3> separationForces;

        public float separationDistance;
        public int instanceCount;

        public void Execute(int index)
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

            separationForces[index] = steering;
        }
    }

    // Job for calculating Alignment forces
    [BurstCompile]
    struct AlignmentJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> positions;
        [ReadOnly] public NativeArray<Vector3> velocities;
        public NativeArray<Vector3> alignmentForces;

        public float alignmentDistance;
        public int instanceCount;

        public void Execute(int index)
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

            alignmentForces[index] = steering;
        }
    }

    // Job for calculating Cohesion forces
    [BurstCompile]
    struct CohesionJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> positions;
        public NativeArray<Vector3> cohesionForces;

        public float cohesionDistance;
        public int instanceCount;

        public void Execute(int index)
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

            cohesionForces[index] = steering;
        }
    }
}
