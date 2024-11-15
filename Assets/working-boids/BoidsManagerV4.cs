using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

// Add this new struct to store polygon points
// public struct Polygon
// {
//     public NativeArray<float2> points;  // Stores the vertices of the polygon in XZ plane
// }


public class BoidsManagerV4 : MonoBehaviour
{
    [Header("Polygon Boundary")]
    public Vector3[] polygonPoints;   // Set this in inspector or via code
    private NativeArray<float2> boundaryPoints;  // For use in jobs
    
    [Header("Anti-Boundary Points")]
    public Vector3[] antiPolygonPoints;   // Set this in inspector or via code
    [ReadOnly] public NativeArray<float2> antiBoundaryPoints;
    public float antiBoundaryForce;

    
    
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
        InitializeBoundaryPoints();
        InitializeArrays();
        SpawnBoids();
    }
    
    void InitializeBoundaryPoints()
    {
        // Convert Vector2 array to NativeArray<float2>
        boundaryPoints  = new NativeArray<float2>(polygonPoints.Length, Allocator.Persistent);
        for (int i = 0; i < polygonPoints.Length; i++)
        {
            boundaryPoints[i] = new float2(polygonPoints[i].x, polygonPoints[i].z);
        }
        
        // Convert Vector2 array to NativeArray<float2>
        antiBoundaryPoints  = new NativeArray<float2>(antiPolygonPoints.Length, Allocator.Persistent);
        for (int i = 0; i < antiPolygonPoints.Length; i++)
        {
            antiBoundaryPoints[i] = new float2(antiPolygonPoints[i].x, antiPolygonPoints[i].z);
        }
    }
    
    // Helper method to visualize the boundary in the editor
    void OnDrawGizmos()
    {
        if (polygonPoints != null && polygonPoints.Length > 2)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < polygonPoints.Length; i++)
            {
                Vector3 current = polygonPoints[i];
                Vector3 next = polygonPoints[(i + 1) % polygonPoints.Length];
                Gizmos.DrawLine(current, next);
            }
        }
        if (antiPolygonPoints != null && antiPolygonPoints.Length > 2)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < antiPolygonPoints.Length; i++)
            {
                Vector3 current = antiPolygonPoints[i];
                Vector3 next = antiPolygonPoints[(i + 1) % antiPolygonPoints.Length];
                Gizmos.DrawLine(current, next);
            }
        }
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
        PreyUpdateJobV4 preyJob = new PreyUpdateJobV4
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
            boundaryPoints = boundaryPoints,  // Add this
            boundaryTurnForce = boundaryTurnForce,
            antiBoundaryPoints = antiBoundaryPoints,
            antiBoundaryForce = antiBoundaryForce
        };

        // Create predator update job
        PredatorUpdateJobV4 predatorJob = new PredatorUpdateJobV4
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
            boundaryPoints = boundaryPoints,  // Add this
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
        if (boundaryPoints.IsCreated)
            boundaryPoints.Dispose();
        if (antiBoundaryPoints.IsCreated)
            antiBoundaryPoints.Dispose();

    }
}
[BurstCompile]
public struct PreyUpdateJobV4 : IJobParallelFor
{
    [ReadOnly] public NativeArray<float2> boundaryPoints;
    [ReadOnly] public NativeArray<float2> antiBoundaryPoints;  // Array of anti-boundary shapes
    public float antiBoundaryForce;
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

   
public void Execute(int index)
{
    // Get current boid state
    float3 position = currentPositions[index];
    float3 velocity = currentVelocities[index];

    // Initialize flocking forces
    float3 cohesion = float3.zero;    
    float3 separation = float3.zero;   
    float3 alignment = float3.zero;    
    float3 avoidPredator = float3.zero;
    float3 avoidBoundary = float3.zero;
    int neighborCount = 0;             

    // Calculate flocking behaviors (same as before)
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

    if (neighborCount > 0)
    {
        cohesion = (cohesion / neighborCount - position) * cohesionWeight;
        separation = separation * separationWeight;
        alignment = (alignment / neighborCount) * alignmentWeight;
    }   

    // Calculate predator avoidance (same as before)
    for (int i = 0; i < predatorPositions.Length; i++)
    {
        float3 offset = position - predatorPositions[i];
        float sqrDst = math.lengthsq(offset);
        
        if (sqrDst < perceptionRadius * perceptionRadius * 4)
        {
            avoidPredator += math.normalize(offset) * (avoidPredatorWeight / math.sqrt(sqrDst));
        }
    }

    // Calculate boundary line avoidance
    float2 pos2D = new float2(position.x, position.z);
    float avoidanceRadius = perceptionRadius * 2; // Distance at which to start avoiding boundaries

    for (int i = 0; i < boundaryPoints.Length; i++)
    {
        float2 lineStart = boundaryPoints[i];
        float2 lineEnd = boundaryPoints[(i + 1) % boundaryPoints.Length];
        
        // Find closest point on line segment
        float2 line = lineEnd - lineStart;
        float len = math.length(line);
        float2 lineDir = line / len;
        
        float t = math.dot(pos2D - lineStart, lineDir);
        t = math.clamp(t, 0, len);
        
        float2 closestPoint = lineStart + lineDir * t;
        float dist = math.distance(pos2D, closestPoint);
        
        // Apply avoidance force if close to line
        if (dist < avoidanceRadius)
        {
            float2 awayDir = math.normalize(pos2D - closestPoint);
            // Use inverse square law like predator avoidance
            float strength = boundaryTurnForce / (dist * dist);
            avoidBoundary += new float3(awayDir.x, 0, awayDir.y) * strength;
        }
    }
    

    // Combine all forces
    velocity += cohesion + separation + alignment + avoidPredator + avoidBoundary;

    // Normalize and apply speed
    velocity.y = 0;
    velocity = math.normalize(velocity) * speed;

    // Test next position before moving
    float3 nextPosition = position + velocity * deltaTime;
    nextPosition.y = 0;

    // Check if next position would be inside polygon
    if (!IsPointInPolygon(new float2(nextPosition.x, nextPosition.z),antiBoundaryPoints))
    {
        
        position = nextPosition;
    } else
    {
       
        float2 currentPos2D = new float2(position.x, position.z);
    
        // Find closest boundary point
        float2 closestPoint = antiBoundaryPoints[0];
        float minDist = float.MaxValue;
    
        for (int i = 0; i < antiBoundaryPoints.Length; i++)
        {
            float2 point = antiBoundaryPoints[i];
            float dist = math.distance(currentPos2D, point);
            if (dist < minDist)
            {
                minDist = dist;
                closestPoint = point;
            }
        }

        // Calculate direction away from boundary
        float2 awayDir = math.normalize(currentPos2D - closestPoint);
    
        // Use boundaryTurnForce to adjust the strength of the correction
        float3 turnForce = new float3(awayDir.x, 0, awayDir.y) * boundaryTurnForce;
        velocity += turnForce;
    
        // Normalize and apply speed after adding turn force
        velocity = math.normalize(velocity) * speed;
        velocity.y = 0;
    }

    // Final velocity normalization
    velocity = math.normalize(velocity) * speed;
    velocity.y = 0;

    // Store results
    newPositions[index] = position;
    newVelocities[index] = velocity;
}

private float2 GetPolygonCenter(NativeArray<float2> points)
{
    float2 center = float2.zero;
    for (int i = 0; i < points.Length; i++)
    {
        center += points[i];
    }
    return center / points.Length;
}

private bool IsPointInPolygon(float2 point, NativeArray<float2> points)
    {
        int windingNumber = 0;
    
        for (int i = 0; i < points.Length; i++)
        {
            float2 current = points[i];
            float2 next = points[(i + 1) % points.Length];

            windingNumber += CalculateWindingSegment(point, current, next);
        }

        return windingNumber != 0;
    }

    private int CalculateWindingSegment(float2 point, float2 vertex1, float2 vertex2)
    {
        if (vertex1.y <= point.y)
        {
            if (vertex2.y > point.y)
            {
                if (IsLeft(vertex1, vertex2, point) > 0)
                {
                    return 1;
                }
            }
        }
        else
        {
            if (vertex2.y <= point.y)
            {
                if (IsLeft(vertex1, vertex2, point) < 0)
                {
                    return -1;
                }
            }
        }
        return 0;
    }

    private float IsLeft(float2 vertex1, float2 vertex2, float2 point)
    {
        return ((vertex2.x - vertex1.x) * (point.y - vertex1.y) - 
                (point.x - vertex1.x) * (vertex2.y - vertex1.y));
    }

    private float3 GetBoundaryForce(float3 position, float bound)
    {
        float2 pos2D = new float2(position.x, position.z);
        float3 force = float3.zero;
    
        // Find closest point on any boundary line
        float minDist = float.MaxValue;
        float2 closestPoint = float2.zero;
    
        for (int i = 0; i < boundaryPoints.Length; i++)
        {
            float2 start = boundaryPoints[i];
            float2 end = boundaryPoints[(i + 1) % boundaryPoints.Length];
        
            // Get closest point on this line segment
            float2 line = end - start;
            float len = math.length(line);
            float2 dir = line / len;
        
            float t = math.dot(pos2D - start, dir);
            t = math.clamp(t, 0, len);
        
            float2 nearest = start + dir * t;
            float dist = math.distance(pos2D, nearest);
        
            if (dist < minDist)
            {
                minDist = dist;
                closestPoint = nearest;
            }
        }
    
        // If we're close to the boundary, apply a strong repulsive force
        if (minDist < bound)
        {
            float2 awayDir = math.normalize(pos2D - closestPoint);
            float strength = (bound - minDist) / bound * 10f; // Strong force that increases as we get closer
            force.x = awayDir.x * strength;
            force.z = awayDir.y * strength;
        }
    
        return force;
    }

}

[BurstCompile]
public struct PredatorUpdateJobV4 : IJobParallelFor
{
    [ReadOnly] public NativeArray<float2> boundaryPoints;
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
        
        // // Calculate and apply boundary avoidance
        // float softBound = boundarySize - softBoundaryOffset;
        // float3 boundaryForce = GetBoundaryForce(position, softBound);
        //
        // // Only apply boundary force if we're actually near a boundary
        // if (math.lengthsq(boundaryForce) > 0)
        // {
        //     velocity += boundaryForce * boundaryTurnForce;
        // }
        if (!IsPointInPolygon(new float2(position.x, position.z)))
        {
            float3 boundaryForce = CalculatePolygonBoundaryForce(position);
            velocity += boundaryForce * boundaryTurnForce;
        }

        // Ensure velocity stays in X-Z plane
        velocity.y = 0;
        velocity = math.normalize(velocity) * speed;
        position += velocity * deltaTime;
        position.y = 0;

        // Clamp position to boundaries
       // position = math.clamp(position, new float3(-boundarySize), new float3(boundarySize));

        newPositions[index] = position;
        newVelocities[index] = velocity;
    }

   private bool IsPointInPolygon(float2 point)
    {
        int winding = 0;
        int vertexCount = boundaryPoints.Length;

        for (int i = 0; i < vertexCount; i++)
        {
            int nextIndex = (i + 1) % vertexCount;
            float2 vertex1 = boundaryPoints[i];
            float2 vertex2 = boundaryPoints[nextIndex];

            if (vertex1.y <= point.y)
            {
                if (vertex2.y > point.y && IsLeft(vertex1, vertex2, point) > 0)
                    winding++;
            }
            else
            {
                if (vertex2.y <= point.y && IsLeft(vertex1, vertex2, point) < 0)
                    winding--;
            }
        }

        return winding != 0;
    }

    private float IsLeft(float2 p0, float2 p1, float2 point)
    {
        return (p1.x - p0.x) * (point.y - p0.y) - (point.x - p0.x) * (p1.y - p0.y);
    }

    private float3 CalculatePolygonBoundaryForce(float3 position)
    {
        float3 force = float3.zero;
        float2 pos2D = new float2(position.x, position.z);
        
        // Find closest point on polygon
        float minDist = float.MaxValue;
        float2 closestPoint = float2.zero;
        
        for (int i = 0; i < boundaryPoints.Length; i++)
        {
            int nextIndex = (i + 1) % boundaryPoints.Length;
            float2 vertex1 = boundaryPoints[i];
            float2 vertex2 = boundaryPoints[nextIndex];
            
            float2 closest = GetClosestPointOnLineSegment(vertex1, vertex2, pos2D);
            float dist = math.distance(pos2D, closest);
            
            if (dist < minDist)
            {
                minDist = dist;
                closestPoint = closest;
            }
        }
        
        // Calculate force direction (away from boundary)
        float2 directionFromBoundary = math.normalize(pos2D - closestPoint);
        force.x = directionFromBoundary.x;
        force.z = directionFromBoundary.y;
        
        return force;
    }

    private float2 GetClosestPointOnLineSegment(float2 a, float2 b, float2 point)
    {
        float2 ab = b - a;
        float t = math.dot(point - a, ab) / math.dot(ab, ab);
        t = math.clamp(t, 0, 1);
        return a + t * ab;
    }
}
