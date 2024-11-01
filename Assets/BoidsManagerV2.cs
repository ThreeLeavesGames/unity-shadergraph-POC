using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

public class BoidsManagerV2 : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject preyPrefab;
    public GameObject predatorPrefab;
    public int preyCount = 100;
    public int predatorCount = 3;
    public float spawnRadius = 10f;

    [Header("Prey Settings")]
    public float preySpeed = 5f;
    public float preyPerceptionRadius = 2.5f;
    public float preyCohesionWeight = 1f;
    public float preySeparationWeight = 1.5f;
    public float preyAlignmentWeight = 1f;
    public float preyAvoidPredatorWeight = 2f;

    [Header("Predator Settings")]
    public float predatorSpeed = 7f;
    public float predatorPerceptionRadius = 5f;
    public float predatorChaseWeight = 2f;
    
    [Header("Boundary Settings")]
    public float boundarySize = 20f;         // Size of the hard boundary
    public float softBoundaryOffset = 1f;    // How far from boundary to start turning (default 1 unit from hard boundary)
    public float boundaryTurnForce = 0.5f;   // Force applied to turn away from boundary


    private NativeArray<float3> preyPositions;
    private NativeArray<float3> preyVelocities;
    private NativeArray<float3> predatorPositions;
    private NativeArray<float3> predatorVelocities;
    private Transform[] preyTransforms;
    private Transform[] predatorTransforms;

    // Arrays to store updated positions and velocities
    private NativeArray<float3> newPreyPositions;
    private NativeArray<float3> newPreyVelocities;
    private NativeArray<float3> newPredatorPositions;
    private NativeArray<float3> newPredatorVelocities;

    void Start()
    {
        InitializeArrays();
        SpawnBoids();
    }

    void InitializeArrays()
    {
        // Initialize main arrays
        preyPositions = new NativeArray<float3>(preyCount, Allocator.Persistent);
        preyVelocities = new NativeArray<float3>(preyCount, Allocator.Persistent);
        predatorPositions = new NativeArray<float3>(predatorCount, Allocator.Persistent);
        predatorVelocities = new NativeArray<float3>(predatorCount, Allocator.Persistent);

        // Initialize temporary arrays for updates
        newPreyPositions = new NativeArray<float3>(preyCount, Allocator.Persistent);
        newPreyVelocities = new NativeArray<float3>(preyCount, Allocator.Persistent);
        newPredatorPositions = new NativeArray<float3>(predatorCount, Allocator.Persistent);
        newPredatorVelocities = new NativeArray<float3>(predatorCount, Allocator.Persistent);

        preyTransforms = new Transform[preyCount];
        predatorTransforms = new Transform[predatorCount];
    }

    void SpawnBoids()
    {
        for (int i = 0; i < preyCount; i++)
        {
            Vector3 randomPos = UnityEngine.Random.insideUnitSphere * spawnRadius;
            GameObject prey = Instantiate(preyPrefab, randomPos, UnityEngine.Random.rotation);
            preyTransforms[i] = prey.transform;
            preyPositions[i] = randomPos;
            preyVelocities[i] = UnityEngine.Random.insideUnitSphere.normalized * preySpeed;
        }

        for (int i = 0; i < predatorCount; i++)
        {
            Vector3 randomPos = UnityEngine.Random.insideUnitSphere * spawnRadius;
            GameObject predator = Instantiate(predatorPrefab, randomPos, UnityEngine.Random.rotation);
            predatorTransforms[i] = predator.transform;
            predatorPositions[i] = randomPos;
            predatorVelocities[i] = UnityEngine.Random.insideUnitSphere.normalized * predatorSpeed;
        }

        // Copy initial values to new arrays
        preyPositions.CopyTo(newPreyPositions);
        preyVelocities.CopyTo(newPreyVelocities);
        predatorPositions.CopyTo(newPredatorPositions);
        predatorVelocities.CopyTo(newPredatorVelocities);
    }

    void Update()
    {
        UpdateBoidsPositions();
        UpdateTransforms();
    }

    void UpdateBoidsPositions()
    {
        // Create prey update job
        PreyUpdateJob preyJob = new PreyUpdateJob
        {
            deltaTime = Time.deltaTime,
            currentPositions = preyPositions,
            currentVelocities = preyVelocities,
            newPositions = newPreyPositions,
            newVelocities = newPreyVelocities,
            predatorPositions = predatorPositions,
            speed = preySpeed,
            perceptionRadius = preyPerceptionRadius,
            cohesionWeight = preyCohesionWeight,
            separationWeight = preySeparationWeight,
            alignmentWeight = preyAlignmentWeight,
            avoidPredatorWeight = preyAvoidPredatorWeight,
            boundarySize = boundarySize,                  // Add these new parameters
            softBoundaryOffset = softBoundaryOffset,
            boundaryTurnForce = boundaryTurnForce
        };

        // Create predator update job
        PredatorUpdateJob predatorJob = new PredatorUpdateJob
        {
            deltaTime = Time.deltaTime,
            currentPositions = predatorPositions,
            currentVelocities = predatorVelocities,
            newPositions = newPredatorPositions,
            newVelocities = newPredatorVelocities,
            preyPositions = preyPositions,
            speed = predatorSpeed,
            perceptionRadius = predatorPerceptionRadius,
            chaseWeight = predatorChaseWeight,
            boundarySize = boundarySize,                  // Add these new parameters
            softBoundaryOffset = softBoundaryOffset,
            boundaryTurnForce = boundaryTurnForce
        };

        // Schedule both jobs
        JobHandle preyHandle = preyJob.Schedule(preyCount, 64);
        JobHandle predatorHandle = predatorJob.Schedule(predatorCount, 32, preyHandle);

        // Wait for all jobs to complete
        predatorHandle.Complete();

        // Swap the arrays
        SwapArrays();
    }

    void SwapArrays()
    {
        // Copy new positions and velocities to current arrays
        newPreyPositions.CopyTo(preyPositions);
        newPreyVelocities.CopyTo(preyVelocities);
        newPredatorPositions.CopyTo(predatorPositions);
        newPredatorVelocities.CopyTo(predatorVelocities);
    }

    void UpdateTransforms()
    {
        for (int i = 0; i < preyCount; i++)
        {
            preyTransforms[i].position = preyPositions[i];
            if (math.lengthsq(preyVelocities[i]) > 0.001f)
            {
                preyTransforms[i].rotation = Quaternion.LookRotation(preyVelocities[i]);
            }
        }

        for (int i = 0; i < predatorCount; i++)
        {
            predatorTransforms[i].position = predatorPositions[i];
            if (math.lengthsq(predatorVelocities[i]) > 0.001f)
            {
                predatorTransforms[i].rotation = Quaternion.LookRotation(predatorVelocities[i]);
            }
        }
    }

    void OnDestroy()
    {
        // Dispose all NativeArrays
        preyPositions.Dispose();
        preyVelocities.Dispose();
        predatorPositions.Dispose();
        predatorVelocities.Dispose();
        newPreyPositions.Dispose();
        newPreyVelocities.Dispose();
        newPredatorPositions.Dispose();
        newPredatorVelocities.Dispose();
    }
}
[BurstCompile]
public struct PreyUpdateJob : IJobParallelFor
{
    public float deltaTime;
    [ReadOnly] public NativeArray<float3> currentPositions;
    [ReadOnly] public NativeArray<float3> currentVelocities;
    [WriteOnly] public NativeArray<float3> newPositions;
    [WriteOnly] public NativeArray<float3> newVelocities;
    [ReadOnly] public NativeArray<float3> predatorPositions;
    public float boundarySize;        // Size of the hard boundary
    public float softBoundaryOffset;  // Distance from boundary to start turning
    public float boundaryTurnForce;   // Strength of the turn force
    
    public float speed;
    public float perceptionRadius;
    public float cohesionWeight;
    public float separationWeight;
    public float alignmentWeight;
    public float avoidPredatorWeight;

    public void Execute(int index)
    {
        float3 position = currentPositions[index];
        float3 velocity = currentVelocities[index];

        float3 cohesion = float3.zero;
        float3 separation = float3.zero;
        float3 alignment = float3.zero;
        float3 avoidPredator = float3.zero;

        int neighborCount = 0;

        // Calculate flocking behaviors
        for (int i = 0; i < currentPositions.Length; i++)
        {
            if (i == index) continue;

            float3 offset = currentPositions[i] - position;
            float sqrDst = math.lengthsq(offset);

            if (sqrDst < perceptionRadius * perceptionRadius)
            {
                cohesion += currentPositions[i];
                separation += -offset / math.sqrt(sqrDst);
                alignment += currentVelocities[i];
                neighborCount++;
            }
        }

        // Apply flocking rules
        if (neighborCount > 0)
        {
            cohesion = (cohesion / neighborCount - position) * cohesionWeight;
            separation = separation * separationWeight;
            alignment = (alignment / neighborCount) * alignmentWeight;
        }

        // Avoid predators
        for (int i = 0; i < predatorPositions.Length; i++)
        {
            float3 offset = position - predatorPositions[i];
            float sqrDst = math.lengthsq(offset);
            
            if (sqrDst < perceptionRadius * perceptionRadius * 4)
            {
                avoidPredator += math.normalize(offset) * (avoidPredatorWeight / math.sqrt(sqrDst));
            }
        }

        // Update velocity and position
        velocity += cohesion + separation + alignment + avoidPredator;

        float softBound = boundarySize - softBoundaryOffset;
        float3 boundaryForce = GetBoundaryForce(position, softBound);
    
        // Only apply boundary force if we're actually near a boundary
        if (math.lengthsq(boundaryForce) > 0)
        {
            velocity += boundaryForce * boundaryTurnForce;
        }

        velocity = math.normalize(velocity) * speed;
        position += velocity * deltaTime;

        // Clamp position to boundaries
       // float hardBound = 20f;
        position = math.clamp(position, new float3(-boundarySize), new float3(boundarySize));

        newPositions[index] = position;
        newVelocities[index] = velocity;
    }

    private float3 GetBoundaryForce(float3 position, float bound)
    {
        float3 force = float3.zero;
        float margin = bound * 0.1f; // Only start applying force when within 10% of the boundary

        // Only apply force when close to boundaries
        if (math.abs(position.x) > bound - margin)
            force.x = -math.sign(position.x) * (math.abs(position.x) - (bound - margin)) / margin;
    
        if (math.abs(position.y) > bound - margin)
            force.y = -math.sign(position.y) * (math.abs(position.y) - (bound - margin)) / margin;
    
        if (math.abs(position.z) > bound - margin)
            force.z = -math.sign(position.z) * (math.abs(position.z) - (bound - margin)) / margin;

        return force;
    }
}

[BurstCompile]
public struct PredatorUpdateJob : IJobParallelFor
{
    public float deltaTime;
    [ReadOnly] public NativeArray<float3> currentPositions;
    [ReadOnly] public NativeArray<float3> currentVelocities;
    [WriteOnly] public NativeArray<float3> newPositions;
    [WriteOnly] public NativeArray<float3> newVelocities;
    [ReadOnly] public NativeArray<float3> preyPositions;
    
    public float boundarySize;        // Size of the hard boundary
    public float softBoundaryOffset;  // Distance from boundary to start turning
    public float boundaryTurnForce;   // Strength of the turn force
    
    public float speed;
    public float perceptionRadius;
    public float chaseWeight;

    public void Execute(int index)
    {
        float3 position = currentPositions[index];
        float3 velocity = currentVelocities[index];
        float3 preyDirection = float3.zero;
        float closestPreyDistance = float.MaxValue;

        // Find closest prey
        for (int i = 0; i < preyPositions.Length; i++)
        {
            float3 offset = preyPositions[i] - position;
            float sqrDst = math.lengthsq(offset);

            if (sqrDst < closestPreyDistance && sqrDst < perceptionRadius * perceptionRadius)
            {
                closestPreyDistance = sqrDst;
                preyDirection = math.normalize(offset);
            }
        }

        // Chase prey if found
        if (closestPreyDistance < float.MaxValue)
        {
            velocity = math.lerp(velocity, preyDirection * speed, chaseWeight * deltaTime);
        }

        // Add boundary avoidance force
        // float bound = 19f;
        // float turnForce = 0.5f;
        // velocity += GetBoundaryForce(position, bound) * turnForce;
        
        // Add boundary avoidance with configurable parameters
        float softBound = boundarySize - softBoundaryOffset;
        float3 boundaryForce = GetBoundaryForce(position, softBound);
    
        // Only apply boundary force if we're actually near a boundary
        if (math.lengthsq(boundaryForce) > 0)
        {
            velocity += boundaryForce * boundaryTurnForce;
        }

        // Update position
        velocity = math.normalize(velocity) * speed;
        position += velocity * deltaTime;

        // Clamp position to boundaries
       // float hardBound = 20f;
        position = math.clamp(position, new float3(-boundarySize), new float3(boundarySize));

        newPositions[index] = position;
        newVelocities[index] = velocity;
    }

    private float3 GetBoundaryForce(float3 position, float bound)
    {
        float3 force = float3.zero;
        float margin = bound * 0.1f; // Only start applying force when within 10% of the boundary

        // Only apply force when close to boundaries
        if (math.abs(position.x) > bound - margin)
            force.x = -math.sign(position.x) * (math.abs(position.x) - (bound - margin)) / margin;
        
        if (math.abs(position.y) > bound - margin)
            force.y = -math.sign(position.y) * (math.abs(position.y) - (bound - margin)) / margin;
        
        if (math.abs(position.z) > bound - margin)
            force.z = -math.sign(position.z) * (math.abs(position.z) - (bound - margin)) / margin;

        return force;
    }
}
