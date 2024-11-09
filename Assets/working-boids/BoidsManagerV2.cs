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



    // Native arrays for current frame data (persistent memory)
    private NativeArray<float3> preyPositions;      // Current prey positions
    private NativeArray<float3> preyVelocities;     // Current prey velocities
    private NativeArray<float3> predatorPositions;   // Current predator positions
    private NativeArray<float3> predatorVelocities;  // Current predator velocities
    private Transform[] preyTransforms;             // References to prey GameObjects
    private Transform[] predatorTransforms;         // References to predator GameObjects

    // Double-buffered arrays for next frame data (prevents race conditions)
    private NativeArray<float3> newPreyPositions;     // Next frame prey positions
    private NativeArray<float3> newPreyVelocities;    // Next frame prey velocities
    private NativeArray<float3> newPredatorPositions;  // Next frame predator positions
    private NativeArray<float3> newPredatorVelocities; // Next frame predator velocities

    void Start()
    {
        InitializeArrays();
        SpawnBoids();
    }

    void InitializeArrays()
    {
        // Initialize main arrays with persistent allocator for long-term storage
        preyPositions = new NativeArray<float3>(preyCount, Allocator.Persistent);
        preyVelocities = new NativeArray<float3>(preyCount, Allocator.Persistent);
        predatorPositions = new NativeArray<float3>(predatorCount, Allocator.Persistent);
        predatorVelocities = new NativeArray<float3>(predatorCount, Allocator.Persistent);

        // Initialize temporary arrays for double buffering
        newPreyPositions = new NativeArray<float3>(preyCount, Allocator.Persistent);
        newPreyVelocities = new NativeArray<float3>(preyCount, Allocator.Persistent);
        newPredatorPositions = new NativeArray<float3>(predatorCount, Allocator.Persistent);
        newPredatorVelocities = new NativeArray<float3>(predatorCount, Allocator.Persistent);

        // Initialize transform arrays for GameObject references
        preyTransforms = new Transform[preyCount];
        predatorTransforms = new Transform[predatorCount];
    }

    /// <summary>
    /// Spawns initial boids in the scene and initializes their positions/velocities
    /// </summary>
    void SpawnBoids()
    {
        for (int i = 0; i < preyCount; i++)
        {
            // Use Random.insideUnitCircle for X-Z plane spawn
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * spawnRadius;
            Vector3 randomPos = new Vector3(randomCircle.x, 0, randomCircle.y); // Y is always 0
            
            // Instantiate prey and store transform
            GameObject prey = Instantiate(preyPrefab, randomPos, UnityEngine.Random.rotation);
            preyTransforms[i] = prey.transform;
            preyPositions[i] = randomPos;
        
            // Create random direction in X-Z plane
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
            preyVelocities[i] = new float3(randomDir.x, 0, randomDir.y) * preySpeed;
        }

        for (int i = 0; i < predatorCount; i++)
        {
            // Use Random.insideUnitCircle for X-Z plane spawn
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * spawnRadius;
            Vector3 randomPos = new Vector3(randomCircle.x, 0, randomCircle.y); // Y is always 0
            GameObject predator = Instantiate(predatorPrefab, randomPos, UnityEngine.Random.rotation);
            predatorTransforms[i] = predator.transform;
            predatorPositions[i] = randomPos;
        
            // Create random direction in X-Z plane
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
            predatorVelocities[i] = new float3(randomDir.x, 0, randomDir.y) * predatorSpeed;
        }

        // Initialize double buffer arrays with starting values
        preyPositions.CopyTo(newPreyPositions);
        preyVelocities.CopyTo(newPreyVelocities);
        predatorPositions.CopyTo(newPredatorPositions);
        predatorVelocities.CopyTo(newPredatorVelocities);
    }

    /// <summary>
    /// Updates boid positions and rotations each frame
    /// </summary>
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

        // Update arrays for next frame
        SwapArrays();
    }

    /// <summary>
    /// Swaps double buffered arrays to prepare for next frame
    /// </summary>
    void SwapArrays()
    {
        // Copy new positions and velocities to current arrays
        newPreyPositions.CopyTo(preyPositions);
        newPreyVelocities.CopyTo(preyVelocities);
        newPredatorPositions.CopyTo(predatorPositions);
        newPredatorVelocities.CopyTo(predatorVelocities);
    }

    /// <summary>
    /// Updates GameObject transforms with calculated positions
    /// </summary>
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
    // Time and position/velocity data
    public float deltaTime;                                    // Time since last frame
    [ReadOnly] public NativeArray<float3> currentPositions;    // Current prey positions
    [ReadOnly] public NativeArray<float3> currentVelocities;   // Current prey velocities
    [WriteOnly] public NativeArray<float3> newPositions;       // Output positions for next frame
    [WriteOnly] public NativeArray<float3> newVelocities;      // Output velocities for next frame
    [ReadOnly] public NativeArray<float3> predatorPositions;   // Current predator positions
    // Boundary parameters
    public float boundarySize;        // Size of the simulation space
    public float softBoundaryOffset;  // Distance from edge where turning starts
    public float boundaryTurnForce;   // Strength of boundary avoidance
    
    // Boid behavior parameters
    public float speed;              // Base movement speed
    public float perceptionRadius;   // Radius within which boids can sense others
    public float cohesionWeight;     // Strength of flock centering
    public float separationWeight;   // Strength of collision avoidance
    public float alignmentWeight;    // Strength of velocity matching
    public float avoidPredatorWeight;// Strength of predator avoidance

    /// <summary>
    /// Updates a single prey boid's position and velocity
    /// </summary>

    public void Execute(int index)
    {
        // Get current boid state
        float3 position = currentPositions[index];
        float3 velocity = currentVelocities[index];

        // Initialize flocking forces
        float3 cohesion = float3.zero;    // Force toward center of flock
        float3 separation = float3.zero;   // Force away from nearby boids
        float3 alignment = float3.zero;    // Force to match flock velocity
        float3 avoidPredator = float3.zero;// Force away from predators
        int neighborCount = 0;             // Number of nearby boids

        // Calculate flocking behaviors by checking all other prey
        for (int i = 0; i < currentPositions.Length; i++)
        {
            if (i == index) continue; // Skip self

            // Calculate distance to neighbor
            float3 offset = currentPositions[i] - position;
            float sqrDst = math.lengthsq(offset);

            // If neighbor is within perception radius
            if (sqrDst < perceptionRadius * perceptionRadius)
            {
                cohesion += currentPositions[i];          // Add position for averaging
                separation += -offset / math.sqrt(sqrDst); // Stronger separation when closer 
                alignment += currentVelocities[i];         // Add velocity for averaging
                neighborCount++;
            }
        }

        // Apply flocking rules if we have neighbors
        if (neighborCount > 0)
        {
            // Calculate center of mass and move toward it
            cohesion = (cohesion / neighborCount - position) * cohesionWeight;
            // Apply separation force to avoid collisions
            separation = separation * separationWeight;
            // Match velocity with neighbors
            alignment = (alignment / neighborCount) * alignmentWeight;
        }   

        // Calculate predator avoidance
        for (int i = 0; i < predatorPositions.Length; i++)
        {
            float3 offset = position - predatorPositions[i];
            float sqrDst = math.lengthsq(offset);
            
            // Stronger avoidance when predator is closer, within extended radius
            if (sqrDst < perceptionRadius * perceptionRadius * 4)
            {
                avoidPredator += math.normalize(offset) * (avoidPredatorWeight / math.sqrt(sqrDst));
            }
        }

        // Constrain to XZ plane
        velocity.y = 0;
        position.y = 0;

        // Combine all forces to update velocity
        velocity += cohesion + separation + alignment + avoidPredator;

        // Calculate boundary avoidance
        float softBound = boundarySize - softBoundaryOffset;
        float3 boundaryForce = GetBoundaryForce(position, softBound);
    
        // Apply boundary force if near edges
        if (math.lengthsq(boundaryForce) > 0)
        {
            velocity += boundaryForce * boundaryTurnForce;
        }

        // Maintain XZ plane constraint
        velocity.y = 0;
        velocity = math.normalize(velocity) * speed;
        position += velocity * deltaTime;

        // Enforce boundaries and ground plane
        position = math.clamp(position, new float3(-boundarySize), new float3(boundarySize));
        position.y = 0;
        
        // Store results
        newPositions[index] = position;
        newVelocities[index] = velocity;
    }
    /// <summary>
    /// Calculates force to avoid boundaries
    /// </summary>
    private float3 GetBoundaryForce(float3 position, float bound)
    {
        float3 force = float3.zero;
        float margin = bound * 0.1f; // 10% of boundary size as margin

        // Calculate X axis boundary force
        if (math.abs(position.x) > bound - margin)
            force.x = -math.sign(position.x) * (math.abs(position.x) - (bound - margin)) / margin;
    
        // Calculate Z axis boundary force
        if (math.abs(position.z) > bound - margin)
            force.z = -math.sign(position.z) * (math.abs(position.z) - (bound - margin)) / margin;

        // No vertical force
        force.y = 0;
    
        return force;
    }
}

[BurstCompile]
public struct PredatorUpdateJob : IJobParallelFor
{
    // Time and position/velocity data
    public float deltaTime;                                    // Time since last frame
    [ReadOnly] public NativeArray<float3> currentPositions;    // Current predator positions
    [ReadOnly] public NativeArray<float3> currentVelocities;   // Current predator velocities
    [WriteOnly] public NativeArray<float3> newPositions;       // Output positions for next frame
    [WriteOnly] public NativeArray<float3> newVelocities;      // Output velocities for next frame
    [ReadOnly] public NativeArray<float3> preyPositions;       // Current prey positions
    
    // Boundary parameters
    public float boundarySize;        // Size of simulation space
    public float softBoundaryOffset;  // Distance from edge where turning starts
    public float boundaryTurnForce;   // Strength of boundary avoidance
    
    // Predator behavior parameters
    public float speed;              // Base movement speed
    public float perceptionRadius;   // How far predator can see prey
    public float chaseWeight;        // Strength of chase behavior

    /// <summary>
    /// Updates a single predator's position and velocity
    /// </summary>
    public void Execute(int index)
    {
        // Get current predator state
        float3 position = currentPositions[index];
        float3 velocity = currentVelocities[index];
        float3 preyDirection = float3.zero;
        float closestPreyDistance = float.MaxValue;
        

        // Find closest prey within perception radius
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
        
        // Keep y component at 0
        velocity.y = 0;
        position.y = 0;

        // Chase nearest prey if one was found
        if (closestPreyDistance < float.MaxValue)
        {
            velocity = math.lerp(velocity, preyDirection * speed, chaseWeight * deltaTime);
        }
        
        // Calculate and apply boundary avoidance
        float softBound = boundarySize - softBoundaryOffset;
        float3 boundaryForce = GetBoundaryForce(position, softBound);
    
        // Only apply boundary force if we're actually near a boundary
        if (math.lengthsq(boundaryForce) > 0)
        {
            velocity += boundaryForce * boundaryTurnForce;
        }

        // Ensure velocity stays in X-Z plane
        velocity.y = 0;
        velocity = math.normalize(velocity) * speed;
        position += velocity * deltaTime;
        position.y = 0;

        // Clamp position to boundaries
        position = math.clamp(position, new float3(-boundarySize), new float3(boundarySize));

        newPositions[index] = position;
        newVelocities[index] = velocity;
    }

    private float3 GetBoundaryForce(float3 position, float bound)
    {
        float3 force = float3.zero;
        float margin = bound * 0.1f;

        // Only check X and Z boundaries
        if (math.abs(position.x) > bound - margin)
            force.x = -math.sign(position.x) * (math.abs(position.x) - (bound - margin)) / margin;
    
        if (math.abs(position.z) > bound - margin)
            force.z = -math.sign(position.z) * (math.abs(position.z) - (bound - margin)) / margin;

        // Keep Y force at 0
        force.y = 0;
    
        return force;
    }
}
