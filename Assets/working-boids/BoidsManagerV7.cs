using System;
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


public class BoidsManagerV7 : MonoBehaviour
{
    [Header("Polygon Boundary")]
    public Vector3[] polygonPoints;   // Set this in inspector or via code
    private NativeArray<float2> boundaryPoints;  // For use in jobs
    
    [Header("Anti-Boundary Points")]
    public Vector3[] antiPolygonPoints;   // Set this in inspector or via code
    public NativeArray<float2> antiBoundaryPoints;
    public float antiBoundaryForce;

    [Header("Mesh Settings")]
    public Mesh boidMesh;
    public Material preyMaterial;
    public Material predatorMaterial;
    private Matrix4x4[] preyMatrices;     
    private Matrix4x4[] predatorMatrices;     
    public NativeArray<Matrix4x4> nativePreyMatrices;
    public NativeArray<Matrix4x4> nativePredatorMatrices;
    
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
    [Header("Predator Settings")]
    public float predatorSeparationRadius = 2.5f;
    public float predatorSeparationWeight = 1.5f;
    
    [Header("Boundary Settings")]
    public float boundarySize = 20f;         // Size of the hard boundary
    public float softBoundaryOffset = 1f;    // How far from boundary to start turning (default 1 unit from hard boundary)
    public float boundaryTurnForce = 0.5f;   // Force applied to turn away from boundary



    // Native arrays for current frame data (persistent memory)
    public NativeArray<float3> preyPositions;      // Current prey positions
    public NativeArray<float3> preyVelocities;     // Current prey velocities
    public NativeArray<float3> predatorPositions;   // Current predator positions
    public NativeArray<float3> predatorVelocities;  // Current predator velocities
    // private Transform[] preyTransforms;             // References to prey GameObjects
    // private Transform[] predatorTransforms;         // References to predator GameObjects

    // Double-buffered arrays for next frame data (prevents race conditions)
    public NativeArray<float3> newPreyPositions;     // Next frame prey positions
    public NativeArray<float3> newPreyVelocities;    // Next frame prey velocities
    public NativeArray<float3> newPredatorPositions;  // Next frame predator positions
    public NativeArray<float3> newPredatorVelocities; // Next frame predator velocities
    private JobHandle predatorHandle;
    void Start()
    {

        startScript();

    }
    
    public void startScript()
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


    
    public void Reset(int newPreyCount, int newPredatorCount)
    {
   // Complete any pending jobs
    predatorHandle.Complete();
    
    // Create temporary arrays with sizes matching the new counts to avoid overflow
    NativeArray<float3> tempPreyPositions = new NativeArray<float3>(
        math.min(newPreyCount, newPreyPositions.Length), Allocator.Temp);
    NativeArray<float3> tempPreyVelocities = new NativeArray<float3>(
        math.min(newPreyCount, newPreyVelocities.Length), Allocator.Temp);
    NativeArray<Matrix4x4> tempPreyMatrices = new NativeArray<Matrix4x4>(
        math.min(newPreyCount, nativePreyMatrices.Length), Allocator.Temp);
    
    NativeArray<float3> tempPredatorPositions = new NativeArray<float3>(
        math.min(newPredatorCount, newPredatorPositions.Length), Allocator.Temp);
    NativeArray<float3> tempPredatorVelocities = new NativeArray<float3>(
        math.min(newPredatorCount, newPredatorVelocities.Length), Allocator.Temp);
    NativeArray<Matrix4x4> tempPredatorMatrices = new NativeArray<Matrix4x4>(
        math.min(newPredatorCount, nativePredatorMatrices.Length), Allocator.Temp);

    // Copy only the data we want to keep
    for (int i = 0; i < tempPreyPositions.Length; i++)
    {
        tempPreyPositions[i] = newPreyPositions[i];
        tempPreyVelocities[i] = newPreyVelocities[i];
        tempPreyMatrices[i] = nativePreyMatrices[i];
    }

    for (int i = 0; i < tempPredatorPositions.Length; i++)
    {
        tempPredatorPositions[i] = newPredatorPositions[i];
        tempPredatorVelocities[i] = newPredatorVelocities[i];
        tempPredatorMatrices[i] = nativePredatorMatrices[i];
    }

    // Safely dispose existing arrays
    SafeDispose();

    // Set new counts
    preyCount = newPreyCount;
    predatorCount = newPredatorCount;

    // Initialize new arrays
    InitializeArrays();
    InitializeBoundaryPoints();

    // Copy saved data back
    for (int i = 0; i < tempPreyPositions.Length; i++)
    {
        preyPositions[i] = tempPreyPositions[i];
        preyVelocities[i] = tempPreyVelocities[i];
        nativePreyMatrices[i] = tempPreyMatrices[i];
    }

    for (int i = 0; i < tempPredatorPositions.Length; i++)
    {
        predatorPositions[i] = tempPredatorPositions[i];
        predatorVelocities[i] = tempPredatorVelocities[i];
        nativePredatorMatrices[i] = tempPredatorMatrices[i];
    }

    // Spawn new entities if needed (for increased counts)
    for (int i = tempPreyPositions.Length; i < preyCount; i++)
    {
        Vector3 randomPos = getRandomPosition(polygonPoints, antiPolygonPoints);
        preyPositions[i] = randomPos;
        Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
        preyVelocities[i] = new float3(randomDir.x, 0, randomDir.y) * preySpeed;
        nativePreyMatrices[i] = Matrix4x4.TRS(randomPos, UnityEngine.Random.rotation, Vector3.one * 6);
    }

    for (int i = tempPredatorPositions.Length; i < predatorCount; i++)
    {
        Vector3 randomPos = getRandomPosition(polygonPoints, antiPolygonPoints);
        predatorPositions[i] = randomPos;
        Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
        predatorVelocities[i] = new float3(randomDir.x, 0, randomDir.y) * predatorSpeed;
        nativePredatorMatrices[i] = Matrix4x4.TRS(randomPos, UnityEngine.Random.rotation, Vector3.one * 20);
    }

    // Initialize double buffer arrays
    preyPositions.CopyTo(newPreyPositions);
    preyVelocities.CopyTo(newPreyVelocities);
    predatorPositions.CopyTo(newPredatorPositions);
    predatorVelocities.CopyTo(newPredatorVelocities);

    // Dispose temporary arrays
    tempPreyPositions.Dispose();
    tempPreyVelocities.Dispose();
    tempPreyMatrices.Dispose();
    tempPredatorPositions.Dispose();
    tempPredatorVelocities.Dispose();
    tempPredatorMatrices.Dispose();
    }

    void InitializeArrays()
    {
        preyMatrices = new Matrix4x4[preyCount];
        predatorMatrices = new Matrix4x4[predatorCount];
        nativePreyMatrices = new NativeArray<Matrix4x4>(preyCount, Allocator.Persistent);
        nativePredatorMatrices = new NativeArray<Matrix4x4>(predatorCount, Allocator.Persistent);
        
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

    }

    private Vector3 getRandomPosition(Vector3[] points1,Vector3[] points2)
    {
        float minX = float.MaxValue, minZ = float.MaxValue;
        float maxX = float.MinValue, maxZ = float.MinValue;
    
        for (int j = 0; j < points1.Length; j++)
        {
            minX = math.min(minX, points1[j].x);
            maxX = math.max(maxX, points1[j].x);
            minZ = math.min(minZ, points1[j].z);
            maxZ = math.max(maxZ, points1[j].z);
        }
        float2 testPoint;
        do
        {
            do
            {
                float randX = UnityEngine.Random.Range(minX, maxX);
                float randZ = UnityEngine.Random.Range(minZ, maxZ);
                testPoint = new float2(randX,randZ);
            } while (PolygonUtility.IsPointInPolygon(testPoint,points2) );
              
        } while (!PolygonUtility.IsPointInPolygon(testPoint,points1) );

        return new Vector3(testPoint.x, 0, testPoint.y);
    }

    void SpawnBoids()
    {
        for (int i = 0; i < preyCount; i++)
        {
            
            Vector3 randomPos = getRandomPosition(polygonPoints,antiPolygonPoints);
            preyPositions[i] = randomPos;
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
            preyVelocities[i] = new float3(randomDir.x, 0, randomDir.y) * preySpeed;
            
            nativePreyMatrices[i] = Matrix4x4.TRS(randomPos,  UnityEngine.Random.rotation, Vector3.one * 6);

        }

        for (int i = 0; i < predatorCount; i++)
        {
            Vector3 randomPos = getRandomPosition(polygonPoints,antiPolygonPoints);
            predatorPositions[i] = randomPos;
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
            predatorVelocities[i] = new float3(randomDir.x, 0, randomDir.y) * predatorSpeed;
            nativePredatorMatrices[i] = Matrix4x4.TRS(randomPos,  UnityEngine.Random.rotation, Vector3.one * 20);

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

    public void UpdateBoidsPositions()
    {
        predatorHandle.Complete();
        // Create prey update job
        PreyUpdateJobV7 preyJob = new PreyUpdateJobV7
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
            antiBoundaryForce = antiBoundaryForce,
            nativePreyMatrices = nativePreyMatrices,
        };

        // Create predator update job
        PredatorUpdateJobV7 predatorJob = new PredatorUpdateJobV7
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
            // boundarySize = boundarySize,                  // Add these new parameters
            // softBoundaryOffset = softBoundaryOffset,
            boundaryPoints = boundaryPoints,  // Add this
            boundaryTurnForce = boundaryTurnForce,
            nativePredatorMatrices = nativePredatorMatrices,
            antiBoundaryPoints = antiBoundaryPoints,
            predatorSeparationRadius  = predatorSeparationRadius ,
            predatorSeparationWeight = predatorSeparationWeight 
        };

        // Schedule both jobs
        JobHandle preyHandle = preyJob.Schedule(preyCount, 64);
         predatorHandle = predatorJob.Schedule(predatorCount, 32, preyHandle);

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
            preyMatrices[i] = nativePreyMatrices[i];
        }

        for (int i = 0; i < predatorCount; i++)
        {
            predatorMatrices[i] = nativePredatorMatrices[i];
        }
        Graphics.DrawMeshInstanced(boidMesh, 0, preyMaterial, preyMatrices);
        Graphics.DrawMeshInstanced(boidMesh, 0, predatorMaterial, predatorMatrices);

    }

    void OnDestroy()
    {
        SafeDispose();

    }

    public void SafeDispose()
    {
        // Complete any pending jobs first
        predatorHandle.Complete();

        // Dispose all NativeArrays if they exist
        if (preyPositions.IsCreated) preyPositions.Dispose();
        if (preyVelocities.IsCreated) preyVelocities.Dispose();
        if (predatorPositions.IsCreated) predatorPositions.Dispose();
        if (predatorVelocities.IsCreated) predatorVelocities.Dispose();
        if (newPreyPositions.IsCreated) newPreyPositions.Dispose();
        if (newPreyVelocities.IsCreated) newPreyVelocities.Dispose();
        if (newPredatorPositions.IsCreated) newPredatorPositions.Dispose();
        if (newPredatorVelocities.IsCreated) newPredatorVelocities.Dispose();
        if (boundaryPoints.IsCreated) boundaryPoints.Dispose();
        if (antiBoundaryPoints.IsCreated) antiBoundaryPoints.Dispose();
        if (nativePredatorMatrices.IsCreated) nativePredatorMatrices.Dispose();
        if (nativePreyMatrices.IsCreated) nativePreyMatrices.Dispose();
    }
}
[BurstCompile]
public struct PreyUpdateJobV7 : IJobParallelFor
{
    [ReadOnly] public NativeArray<float2> boundaryPoints;
    [ReadOnly] public NativeArray<float2> antiBoundaryPoints;  // Array of anti-boundary shapes
    [WriteOnly] public NativeArray<Matrix4x4> nativePreyMatrices;
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
    // Calculate anti-boundary line avoidance
    float2 pos2DAnti = new float2(position.x, position.z);
    float avoidanceRadiusAnti = perceptionRadius; // Larger radius for anti-boundary
    float3 antiBoundaryForce = float3.zero;

    for (int i = 0; i < antiBoundaryPoints.Length; i++)
    {
        float2 lineStart = antiBoundaryPoints[i];
        float2 lineEnd = antiBoundaryPoints[(i + 1) % antiBoundaryPoints.Length];
    
        // Find closest point on line segment
        float2 line = lineEnd - lineStart;
        float len = math.length(line);
        float2 lineDir = line / len;
    
        float t = math.dot(pos2DAnti - lineStart, lineDir);
        t = math.clamp(t, 0, len);
    
        float2 closestPoint = lineStart + lineDir * t;
        float dist = math.distance(pos2DAnti, closestPoint);
    
        // Apply avoidance force if close to line
        if (dist < avoidanceRadiusAnti)
        {
            float2 awayDir = math.normalize(pos2DAnti - closestPoint);
            // Stronger force for anti-boundary
            float strength = boundaryTurnForce / (dist * dist);
            antiBoundaryForce += new float3(awayDir.x, 0, awayDir.y) * strength;
        }
    }
    

    // Combine all forces
    velocity += cohesion + separation + alignment + (avoidPredator * 4) + avoidBoundary  + antiBoundaryForce;

    // Normalize and apply speed
    velocity.y = 0;
    velocity = math.normalize(velocity) * speed;

    // Test next position before moving
    float3 nextPosition = position + velocity * deltaTime;
    nextPosition.y = 0;
    position = nextPosition;
    
    // Final velocity normalization
    velocity = math.normalize(velocity) * speed;
    velocity.y = 0;

    if (math.lengthsq(velocity) > 0.001f)
    {
        nativePreyMatrices[index] = Matrix4x4.TRS(
            position, 
            Quaternion.LookRotation(velocity), 
            Vector3.one * 6
        );
    }
    newPositions[index] = position;
    newVelocities[index] = velocity;
}

}

[BurstCompile]
public struct PredatorUpdateJobV7 : IJobParallelFor
{
    [ReadOnly] public NativeArray<float2> boundaryPoints;
    [ReadOnly] public NativeArray<float2> antiBoundaryPoints;  // Array of anti-boundary shapes
    [WriteOnly] public NativeArray<Matrix4x4> nativePredatorMatrices;
    // Time and position/velocity data
    public float deltaTime;                                    // Time since last frame
    [ReadOnly] public NativeArray<float3> currentPositions;    // Current predator positions
    [ReadOnly] public NativeArray<float3> currentVelocities;   // Current predator velocities
    [WriteOnly] public NativeArray<float3> newPositions;       // Output positions for next frame
    [WriteOnly] public NativeArray<float3> newVelocities;      // Output velocities for next frame
    [ReadOnly] public NativeArray<float3> preyPositions;       // Current prey positions
    
    // Boundary parameters
    public float predatorSeparationWeight;        // Size of simulation space
    public float predatorSeparationRadius;  // Distance from edge where turning starts
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
       float3 position = currentPositions[index];
        float3 velocity = currentVelocities[index];
        float3 preyDirection = float3.zero;
        float closestPreyDistance = float.MaxValue;
        float3 avoidBoundary = float3.zero;
        float3 antiBoundaryForce = float3.zero;
        float3 separation = float3.zero;  // Add separation force
        
        
        // Add separation from other predators
        for (int i = 0; i < currentPositions.Length; i++)
        {
            if (i == index) continue;

            float3 offset = position - currentPositions[i];
            float sqrDst = math.lengthsq(offset);
            
            // Use smaller radius for separation than prey detection
            if (sqrDst < predatorSeparationRadius * predatorSeparationRadius * 0.5f)
            {
                // Stronger separation when closer
                separation += math.normalize(offset) * (1.0f / math.sqrt(sqrDst));
            }
        }

        // Only look for prey if chase weight is greater than zero
        if (chaseWeight > 0)
        {
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
        }

        velocity.y = 0;
        position.y = 0;

        // Calculate boundary avoidance
        float2 pos2D = new float2(position.x, position.z);
        float avoidanceRadius = perceptionRadius * 2;

        for (int i = 0; i < boundaryPoints.Length; i++)
        {
            float2 lineStart = boundaryPoints[i];
            float2 lineEnd = boundaryPoints[(i + 1) % boundaryPoints.Length];
            
            float2 line = lineEnd - lineStart;
            float len = math.length(line);
            float2 lineDir = line / len;
            
            float t = math.dot(pos2D - lineStart, lineDir);
            t = math.clamp(t, 0, len);
            
            float2 closestPoint = lineStart + lineDir * t;
            float dist = math.distance(pos2D, closestPoint);
            
            if (dist < avoidanceRadius)
            {
                float2 awayDir = math.normalize(pos2D - closestPoint);
                float strength = boundaryTurnForce / (dist * dist);
                avoidBoundary += new float3(awayDir.x, 0, awayDir.y) * strength;
            }
        }

        // Calculate anti-boundary avoidance
        float avoidanceRadiusAnti = perceptionRadius * 2;
        for (int i = 0; i < antiBoundaryPoints.Length; i++)
        {
            float2 lineStart = antiBoundaryPoints[i];
            float2 lineEnd = antiBoundaryPoints[(i + 1) % antiBoundaryPoints.Length];
            
            float2 line = lineEnd - lineStart;
            float len = math.length(line);
            float2 lineDir = line / len;
            
            float t = math.dot(pos2D - lineStart, lineDir);
            t = math.clamp(t, 0, len);
            
            float2 closestPoint = lineStart + lineDir * t;
            float dist = math.distance(pos2D, closestPoint);
            
            if (dist < avoidanceRadiusAnti)
            {
                float2 awayDir = math.normalize(pos2D - closestPoint);
                float strength = boundaryTurnForce / (dist * dist);
                antiBoundaryForce += new float3(awayDir.x, 0, awayDir.y) * strength;
            }
        }

        // Combine all forces
        if (chaseWeight > 0 && closestPreyDistance < float.MaxValue)
        {
            velocity += preyDirection * chaseWeight;
        }
        velocity +=  (separation * predatorSeparationWeight) + avoidBoundary + antiBoundaryForce;

        // Normalize and apply speed
        velocity.y = 0;
        velocity = math.normalize(velocity) * speed;

        // Update position
        float3 nextPosition = position + velocity * deltaTime;
        nextPosition.y = 0;
        position = nextPosition;

        // Final velocity normalization
        velocity = math.normalize(velocity) * speed;
        velocity.y = 0;

        // Update matrix
        if (math.lengthsq(velocity) > 0.001f)
        {
            nativePredatorMatrices[index] = Matrix4x4.TRS(
                position, 
                Quaternion.LookRotation(velocity), 
                Vector3.one * 20
            );
        }

        newPositions[index] = position;
        newVelocities[index] = velocity;
    }

}
