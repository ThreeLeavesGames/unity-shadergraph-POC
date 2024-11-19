using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Random = UnityEngine.Random;


public class HierarchicalBoids : MonoBehaviour
{
  public int boidCount = 100;
    public float boundaryRadius = 50f;
    public float predatorForce = 2f;
    public float preyAvoidanceForce = 1.5f;
    public float cohesionForce = 1f;
    public float alignmentForce = 1f;

    private NativeArray<Vector3> positions;
    private NativeArray<Vector3> velocities;
    private NativeArray<Vector3> newVelocities;
    private NativeArray<int> hierarchy;

    private void Start()
    {
        positions = new NativeArray<Vector3>(boidCount, Allocator.Persistent);
        velocities = new NativeArray<Vector3>(boidCount, Allocator.Persistent);
        newVelocities = new NativeArray<Vector3>(boidCount, Allocator.Persistent);
        hierarchy = new NativeArray<int>(boidCount, Allocator.Persistent);

        // Initialize boid data
        for (int i = 0; i < boidCount; i++)
        {
            positions[i] = Random.insideUnitSphere * boundaryRadius * 0.5f;
            velocities[i] = Random.insideUnitSphere;
            hierarchy[i] = Random.Range(0, 5); // Assign random roles (0 = A, ..., 4 = E)
        }
    }

    private void Update()
    {
        var job = new BoidJob
        {
            deltaTime = Time.deltaTime,
            boundaryRadius = boundaryRadius,
            positions = positions,
            velocities = velocities,
            newVelocities = newVelocities,
            hierarchy = hierarchy,
            predatorForce = predatorForce,
            preyAvoidanceForce = preyAvoidanceForce,
            cohesionForce = cohesionForce,
            alignmentForce = alignmentForce
        };

        var jobHandle = job.Schedule(boidCount, 32);
        jobHandle.Complete();

        // Copy new velocities into the velocities array
        for (int i = 0; i < boidCount; i++)
        {
            velocities[i] = newVelocities[i];
            positions[i] += velocities[i] * Time.deltaTime;
        }
    }

    private Color[] hierarchyColors = new Color[]
    {
        Color.red,    // A
        Color.blue,   // B
        Color.green,  // C
        Color.yellow, // D
        Color.magenta // E
    };

    private void OnDrawGizmos()
    {
        if (positions.IsCreated && hierarchy.IsCreated)
        {
            for (int i = 0; i < boidCount; i++)
            {
                // Set Gizmo color based on hierarchy role
                Gizmos.color = hierarchyColors[hierarchy[i]];
                Gizmos.DrawSphere(positions[i], 0.5f);
            }
        }

        // Draw boundary
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, boundaryRadius);
    }

    private void OnDestroy()
    {
        if (positions.IsCreated) positions.Dispose();
        if (velocities.IsCreated) velocities.Dispose();
        if (newVelocities.IsCreated) newVelocities.Dispose();
        if (hierarchy.IsCreated) hierarchy.Dispose();
    }

 [Unity.Burst.BurstCompile]
private struct BoidJob : IJobParallelFor
{
    public float deltaTime;
    public float boundaryRadius;
    public float predatorForce;
    public float preyAvoidanceForce;
    public float cohesionForce;
    public float alignmentForce;

    [ReadOnly] public NativeArray<Vector3> positions;
    [ReadOnly] public NativeArray<Vector3> velocities;
    [ReadOnly] public NativeArray<int> hierarchy;

    [WriteOnly] public NativeArray<Vector3> newVelocities;

    public void Execute(int index)
    {
        Vector3 currentPosition = positions[index];
        Vector3 currentVelocity = velocities[index];
        int currentRole = hierarchy[index];

        Vector3 predatorAttraction = Vector3.zero;
        Vector3 preyAvoidance = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        Vector3 alignment = Vector3.zero;

        int neighborCount = 0;

        for (int i = 0; i < positions.Length; i++)
        {
            if (i == index) continue;

            Vector3 direction = positions[i] - currentPosition;
            float distance = direction.magnitude;

            if (distance < boundaryRadius * 0.2f) // Neighbor range
            {
                neighborCount++;
                cohesion += positions[i];
                alignment += velocities[i];

                // Predator-prey hierarchy enforcement
                if (IsPredator(currentRole, hierarchy[i]))
                {
                    predatorAttraction += direction.normalized * predatorForce / distance;
                }
                else if (IsPrey(currentRole, hierarchy[i]))
                {
                    preyAvoidance -= direction.normalized * preyAvoidanceForce / distance;
                }
            }
        }

        if (neighborCount > 0)
        {
            cohesion = (cohesion / neighborCount - currentPosition).normalized * cohesionForce;
            alignment = (alignment / neighborCount - currentVelocity).normalized * alignmentForce;
        }

        // Boundary check
        Vector3 boundaryForce = Vector3.zero;
        float distanceFromCenter = currentPosition.magnitude;

        if (distanceFromCenter > boundaryRadius)
        {
            // Push back towards the center with proportional strength
            Vector3 directionToCenter = -currentPosition.normalized;
            float overshoot = distanceFromCenter - boundaryRadius;
            boundaryForce = directionToCenter * overshoot * 2.0f; // Increase the multiplier for stronger force
        }

// Calculate new velocity
        Vector3 newVelocity = currentVelocity + (predatorAttraction + preyAvoidance + cohesion + alignment + boundaryForce) * deltaTime;

// Ensure boid stays within boundary by clamping the position
        if (distanceFromCenter > boundaryRadius)
        {
            newVelocity += -currentPosition.normalized * predatorForce; // Extra force if it's too far out
        }
        newVelocities[index] = newVelocity.normalized * 5f;
    }

    private bool IsPredator(int role, int targetRole)
    {
        // Predator-prey hierarchy logic
        return (role == 0 && targetRole >= 1) || // A is predator of B, C, D, E
               (role == 1 && targetRole >= 2) || // B is predator of C, D, E
               (role == 2 && targetRole >= 3);   // C is predator of D, E
    }

    private bool IsPrey(int role, int targetRole)
    {
        // Inverse of IsPredator for prey behavior
        return IsPredator(targetRole, role);
    }
}
}