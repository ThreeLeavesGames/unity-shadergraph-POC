using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Random = UnityEngine.Random;

// Add this new struct to store polygon points
// public struct Polygon
// {
//     public NativeArray<float2> points;  // Stores the vertices of the polygon in XZ plane
// }


public class BoidsManagerV10 : MonoBehaviour
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
    public Material[] rankMaterials; // Array of materials for different ranks
    private Matrix4x4[] boidMatrices;     
    public NativeArray<Matrix4x4> nativeBoidMatrices;
    
    [Header("Spawn Settings")]
    public GameObject boidPrefab;
    public int totalBoidCount = 100;
    public int[] rankCounts = {80, 15, 5}; // Count for each rank (rank 0, rank 1, rank 2)
    public float spawnRadius = 10f;

    [Header("Boid Settings")]
    public float boidSpeed = 5f;
    public float boidPerceptionRadius = 2.5f;
    public float cohesionWeight = 1f;
    public float separationWeight = 1.5f;
    public float alignmentWeight = 1f;
    public float chaseWeight = 2f; // Higher ranks chase lower ranks
    public float fleeWeight = 2f;  // Lower ranks flee from higher ranks
    
    [Header("Boundary Settings")]
    public float boundarySize = 20f;         // Size of the hard boundary
    public float softBoundaryOffset = 1f;    // How far from boundary to start turning (default 1 unit from hard boundary)
    public float boundaryTurnForce = 0.5f;   // Force applied to turn away from boundary



    // Native arrays for current frame data (persistent memory)
    public NativeArray<float3> boidPositions;      // Current boid positions
    public NativeArray<float3> boidVelocities;     // Current boid velocities
    public NativeArray<int> boidRanks;             // Boid ranks (0=lowest, higher=more dominant)

    // Double-buffered arrays for next frame data (prevents race conditions)
    public NativeArray<float3> newBoidPositions;     // Next frame boid positions
    public NativeArray<float3> newBoidVelocities;    // Next frame boid velocities
    private JobHandle boidHandle;
    
    private MaterialPropertyBlock[] rankPropertyBlocks;
    private ComputeBuffer[] rankPropertyBuffers;
    
    [Header("Mouse Following")]
    public bool isMouseInBoundary = false;
    public float mouseAttractionWeight = 2f;
    private Vector3 mouseWorldPosition;
    
    private struct InstanceData
    {
        public Vector4 color;
        public float fps;
    }
    
    void Start()
    {
        // Calculate total count from rank counts
        totalBoidCount = 0;
        for (int i = 0; i < rankCounts.Length; i++)
        {
            totalBoidCount += rankCounts[i];
        }

        startScript();
        
        // Initialize property blocks for each rank
        rankPropertyBlocks = new MaterialPropertyBlock[rankMaterials.Length];
        rankPropertyBuffers = new ComputeBuffer[rankMaterials.Length];
        
        for (int rankIndex = 0; rankIndex < rankMaterials.Length; rankIndex++)
        {
            if (rankIndex < rankCounts.Length && rankCounts[rankIndex] > 0)
            {
                rankPropertyBlocks[rankIndex] = new MaterialPropertyBlock();
                rankPropertyBuffers[rankIndex] = new ComputeBuffer(rankCounts[rankIndex], 5 * sizeof(float));
                
                var instanceData = new InstanceData[rankCounts[rankIndex]];
                for (int i = 0; i < rankCounts[rankIndex]; i++)
                {
                    instanceData[i].color = Random.ColorHSV();
                    instanceData[i].fps = Random.Range(3, 6);
                }
                
                rankPropertyBuffers[rankIndex].SetData(instanceData);
                rankPropertyBlocks[rankIndex].SetBuffer("_InstanceData", rankPropertyBuffers[rankIndex]);
            }
        }
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


    
    public void Reset(int[] newRankCounts)
    {
        // Complete any pending jobs
        boidHandle.Complete();
        
        // Calculate new total count
        int newTotalCount = 0;
        for (int i = 0; i < newRankCounts.Length; i++)
        {
            newTotalCount += newRankCounts[i];
        }
        
        // Create temporary arrays to preserve existing data
        int preserveCount = math.min(newTotalCount, totalBoidCount);
        NativeArray<float3> tempPositions = new NativeArray<float3>(preserveCount, Allocator.Temp);
        NativeArray<float3> tempVelocities = new NativeArray<float3>(preserveCount, Allocator.Temp);
        NativeArray<int> tempRanks = new NativeArray<int>(preserveCount, Allocator.Temp);
        NativeArray<Matrix4x4> tempMatrices = new NativeArray<Matrix4x4>(preserveCount, Allocator.Temp);
        
        // Copy existing data
        for (int i = 0; i < preserveCount; i++)
        {
            tempPositions[i] = boidPositions[i];
            tempVelocities[i] = boidVelocities[i];
            tempRanks[i] = boidRanks[i];
            tempMatrices[i] = nativeBoidMatrices[i];
        }
        
        // Safely dispose existing arrays
        SafeDispose();
        
        // Set new counts
        rankCounts = (int[])newRankCounts.Clone();
        totalBoidCount = newTotalCount;
        
        // Initialize new arrays
        InitializeArrays();
        InitializeBoundaryPoints();
        
        // Copy saved data back
        for (int i = 0; i < preserveCount; i++)
        {
            boidPositions[i] = tempPositions[i];
            boidVelocities[i] = tempVelocities[i];
            boidRanks[i] = tempRanks[i];
            nativeBoidMatrices[i] = tempMatrices[i];
        }
        
        // Spawn new boids if count increased
        for (int i = preserveCount; i < totalBoidCount; i++)
        {
            Vector3 randomPos = getRandomPosition(polygonPoints, antiPolygonPoints);
            boidPositions[i] = randomPos;
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
            boidVelocities[i] = new float3(randomDir.x, 0, randomDir.y) * boidSpeed;
            
            // Assign rank based on counts
            int currentRank = 0;
            int currentIndex = i;
            for (int rank = 0; rank < rankCounts.Length; rank++)
            {
                if (currentIndex < rankCounts[rank])
                {
                    currentRank = rank;
                    break;
                }
                currentIndex -= rankCounts[rank];
            }
            
            boidRanks[i] = currentRank;
            float scale = 6 + (currentRank * 4);
            nativeBoidMatrices[i] = Matrix4x4.TRS(randomPos, UnityEngine.Random.rotation, Vector3.one * scale);
        }
        
        // Initialize double buffer arrays
        boidPositions.CopyTo(newBoidPositions);
        boidVelocities.CopyTo(newBoidVelocities);
        
        // Dispose temporary arrays
        tempPositions.Dispose();
        tempVelocities.Dispose();
        tempRanks.Dispose();
        tempMatrices.Dispose();
    }

    void InitializeArrays()
    {
        boidMatrices = new Matrix4x4[totalBoidCount];
        nativeBoidMatrices = new NativeArray<Matrix4x4>(totalBoidCount, Allocator.Persistent);
        
        // Initialize main arrays with persistent allocator for long-term storage
        boidPositions = new NativeArray<float3>(totalBoidCount, Allocator.Persistent);
        boidVelocities = new NativeArray<float3>(totalBoidCount, Allocator.Persistent);
        boidRanks = new NativeArray<int>(totalBoidCount, Allocator.Persistent);

        // Initialize temporary arrays for double buffering
        newBoidPositions = new NativeArray<float3>(totalBoidCount, Allocator.Persistent);
        newBoidVelocities = new NativeArray<float3>(totalBoidCount, Allocator.Persistent);
        
        // Initialize property blocks for each rank
        rankPropertyBlocks = new MaterialPropertyBlock[rankMaterials.Length];
        rankPropertyBuffers = new ComputeBuffer[rankMaterials.Length];
        
        for (int rankIndex = 0; rankIndex < rankMaterials.Length; rankIndex++)
        {
            if (rankIndex < rankCounts.Length && rankCounts[rankIndex] > 0)
            {
                rankPropertyBlocks[rankIndex] = new MaterialPropertyBlock();
                rankPropertyBuffers[rankIndex] = new ComputeBuffer(rankCounts[rankIndex], 5 * sizeof(float));
                
                var instanceData = new InstanceData[rankCounts[rankIndex]];
                for (int i = 0; i < rankCounts[rankIndex]; i++)
                {
                    instanceData[i].color = Random.ColorHSV();
                    instanceData[i].fps = Random.Range(3, 6);
                }
                
                rankPropertyBuffers[rankIndex].SetData(instanceData);
                rankPropertyBlocks[rankIndex].SetBuffer("_InstanceData", rankPropertyBuffers[rankIndex]);
            }
        }
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
        int boidIndex = 0;
        
        // Spawn boids for each rank
        for (int rank = 0; rank < rankCounts.Length; rank++)
        {
            for (int i = 0; i < rankCounts[rank]; i++)
            {
                Vector3 randomPos = getRandomPosition(polygonPoints, antiPolygonPoints);
                boidPositions[boidIndex] = randomPos;
                Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
                boidVelocities[boidIndex] = new float3(randomDir.x, 0, randomDir.y) * boidSpeed;
                boidRanks[boidIndex] = rank;
                
                // Scale based on rank (higher rank = larger)
                float scale = 6 + (rank * 4);
                nativeBoidMatrices[boidIndex] = Matrix4x4.TRS(randomPos, UnityEngine.Random.rotation, Vector3.one * scale);
                
                boidIndex++;
            }
        }

        // Initialize double buffer arrays with starting values
        boidPositions.CopyTo(newBoidPositions);
        boidVelocities.CopyTo(newBoidVelocities);
    }

    /// <summary>
    /// Updates boid positions and rotations each frame
    /// </summary>
    void Update()
    {
        // Get mouse position in screen space
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, 0);
        
        if (plane.Raycast(ray, out float distance))
        {
           Vector3 tempMouseWorldPosition = ray.GetPoint(distance);
            
            // Check if mouse is inside polygon
            float2 mousePos2D = new float2(tempMouseWorldPosition.x, tempMouseWorldPosition.z);
            isMouseInBoundary = PolygonUtility.IsPointInPolygon(mousePos2D, polygonPoints) &&
                                !PolygonUtility.IsPointInPolygon(mousePos2D, antiPolygonPoints);
            
            // Update mouse position array
            mouseWorldPosition = isMouseInBoundary ? tempMouseWorldPosition : float3.zero;
        }
        
        UpdateBoidsPositions();
        UpdateTransforms();
    }

    public void UpdateBoidsPositions()
    {
        boidHandle.Complete();
        
        // Create rank-based boid update job
        RankBoidUpdateJobV10 boidJob = new RankBoidUpdateJobV10
        {
            deltaTime = Time.deltaTime,
            currentPositions = boidPositions,
            currentVelocities = boidVelocities,
            currentRanks = boidRanks,
            newPositions = newBoidPositions,
            newVelocities = newBoidVelocities,
            speed = boidSpeed,
            perceptionRadius = boidPerceptionRadius,
            cohesionWeight = cohesionWeight,
            separationWeight = separationWeight,
            alignmentWeight = alignmentWeight,
            chaseWeight = chaseWeight,
            fleeWeight = fleeWeight,
            boundarySize = boundarySize,
            softBoundaryOffset = softBoundaryOffset,
            boundaryPoints = boundaryPoints,
            boundaryTurnForce = boundaryTurnForce,
            antiBoundaryPoints = antiBoundaryPoints,
            antiBoundaryForce = antiBoundaryForce,
            nativeBoidMatrices = nativeBoidMatrices,
            mouseWorldPosition = new float3(mouseWorldPosition.x, mouseWorldPosition.y, mouseWorldPosition.z),
            isMouseActive = isMouseInBoundary,
            mouseAttractionWeight = mouseAttractionWeight,
        };

        // Schedule the job
        boidHandle = boidJob.Schedule(totalBoidCount, 64);

        // Wait for job to complete
        boidHandle.Complete();

        // Update arrays for next frame
        SwapArrays();
    }

    /// <summary>
    /// Swaps double buffered arrays to prepare for next frame
    /// </summary>
    void SwapArrays()
    {
        // Copy new positions and velocities to current arrays
        newBoidPositions.CopyTo(boidPositions);
        newBoidVelocities.CopyTo(boidVelocities);
    }

    /// <summary>
    /// Updates GameObject transforms with calculated positions
    /// </summary>
    void UpdateTransforms()
    {
        // Copy matrices from native array
        for (int i = 0; i < totalBoidCount; i++)
        {
            boidMatrices[i] = nativeBoidMatrices[i];
        }

        // Draw boids by rank
        int matrixIndex = 0;
        for (int rank = 0; rank < rankCounts.Length; rank++)
        {
            if (rankCounts[rank] > 0 && rank < rankMaterials.Length)
            {
                Matrix4x4[] rankMatrices = new Matrix4x4[rankCounts[rank]];
                Array.Copy(boidMatrices, matrixIndex, rankMatrices, 0, rankCounts[rank]);
                
                Graphics.DrawMeshInstanced(boidMesh, 0, rankMaterials[rank], rankMatrices, 
                                         rankMatrices.Length, rankPropertyBlocks[rank]);
                                         
                matrixIndex += rankCounts[rank];
            }
        }
    }

    void OnDestroy()
    {
        SafeDispose();

    }

    public void SafeDispose()
    {
        // Complete any pending jobs first
        boidHandle.Complete();

        // Dispose all NativeArrays if they exist
        if (boidPositions.IsCreated) boidPositions.Dispose();
        if (boidVelocities.IsCreated) boidVelocities.Dispose();
        if (boidRanks.IsCreated) boidRanks.Dispose();
        if (newBoidPositions.IsCreated) newBoidPositions.Dispose();
        if (newBoidVelocities.IsCreated) newBoidVelocities.Dispose();
        if (boundaryPoints.IsCreated) boundaryPoints.Dispose();
        if (antiBoundaryPoints.IsCreated) antiBoundaryPoints.Dispose();
        if (nativeBoidMatrices.IsCreated) nativeBoidMatrices.Dispose();
        
        // Dispose rank-based buffers
        if (rankPropertyBuffers != null)
        {
            for (int i = 0; i < rankPropertyBuffers.Length; i++)
            {
                if (rankPropertyBuffers[i] != null)
                {
                    rankPropertyBuffers[i].Release();
                    rankPropertyBuffers[i] = null;
                }
            }
        }
    }
}
[BurstCompile]
public struct RankBoidUpdateJobV10 : IJobParallelFor
{
    [ReadOnly] public NativeArray<float2> boundaryPoints;
    [ReadOnly] public NativeArray<float2> antiBoundaryPoints;
    [WriteOnly] public NativeArray<Matrix4x4> nativeBoidMatrices;
    public float antiBoundaryForce;
    
    // Time and position/velocity data
    public float deltaTime;
    [ReadOnly] public NativeArray<float3> currentPositions;
    [ReadOnly] public NativeArray<float3> currentVelocities;
    [ReadOnly] public NativeArray<int> currentRanks;
    [WriteOnly] public NativeArray<float3> newPositions;
    [WriteOnly] public NativeArray<float3> newVelocities;
    
    // Boundary parameters
    public float boundarySize;
    public float softBoundaryOffset;
    public float boundaryTurnForce;
    
    // Boid behavior parameters
    public float speed;
    public float perceptionRadius;
    public float cohesionWeight;
    public float separationWeight;
    public float alignmentWeight;
    public float chaseWeight;  // Higher ranks chase lower ranks
    public float fleeWeight;   // Lower ranks flee from higher ranks
    
    public float3 mouseWorldPosition;
    public bool isMouseActive;
    public float mouseAttractionWeight;
   
    public void Execute(int index)
    {
        float3 position = currentPositions[index];
        float3 velocity = currentVelocities[index];
        int myRank = currentRanks[index];

        // Initialize forces
        float3 cohesion = float3.zero;    
        float3 separation = float3.zero;   
        float3 alignment = float3.zero;    
        float3 rankInteraction = float3.zero; // Chase higher ranks, flee from lower ranks
        float3 avoidBoundary = float3.zero;
        float3 mouseAttraction = float3.zero;
        int neighborCount = 0;             

        // Calculate flocking behaviors and rank-based interactions
        for (int i = 0; i < currentPositions.Length; i++)
        {
            if (i == index) continue;

            float3 offset = currentPositions[i] - position;
            float sqrDst = math.lengthsq(offset);
            int otherRank = currentRanks[i];

            if (sqrDst < perceptionRadius * perceptionRadius)
            {
                // Standard flocking with same or similar ranks
                if (math.abs(myRank - otherRank) <= 1)
                {
                    cohesion += currentPositions[i];          
                    separation += -offset / math.sqrt(sqrDst);  
                    alignment += currentVelocities[i];         
                    neighborCount++;
                }
                
                // Rank-based interactions
                if (myRank < otherRank) // Other boid has higher rank (more dominant)
                {
                    // Flee from higher rank
                    rankInteraction += math.normalize(-offset) * (fleeWeight / math.sqrt(sqrDst));
                }
                else if (myRank > otherRank) // Other boid has lower rank (less dominant)
                {
                    // Chase lower rank
                    rankInteraction += math.normalize(offset) * (chaseWeight / (sqrDst + 1.0f));
                }
            }
        }

        if (neighborCount > 0)
        {
            cohesion = (cohesion / neighborCount - position) * cohesionWeight;
            separation = separation * separationWeight;
            alignment = (alignment / neighborCount) * alignmentWeight;
        }   

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
        float3 antiBoundaryForceLocal = float3.zero;
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
            
            if (dist < avoidanceRadius)
            {
                float2 awayDir = math.normalize(pos2D - closestPoint);
                float strength = boundaryTurnForce / (dist * dist);
                antiBoundaryForceLocal += new float3(awayDir.x, 0, awayDir.y) * strength;
            }
        }
        
        // Mouse attraction
        if (isMouseActive)
        {
            float3 toMouse = mouseWorldPosition - position;
            float distToMouse = math.length(toMouse);
                
            if (distToMouse > 0.1f)
            {
                mouseAttraction = math.normalize(toMouse) * mouseAttractionWeight;
            }
        }

        // Combine all forces
        velocity += cohesion + separation + alignment + rankInteraction + avoidBoundary + 
                   antiBoundaryForceLocal + (mouseAttraction * 50);

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

        // Update matrix with rank-based scale
        if (math.lengthsq(velocity) > 0.001f)
        {
            float scale = 6 + (myRank * 4); // Higher rank = larger size
            nativeBoidMatrices[index] = Matrix4x4.TRS(
                position, 
                Quaternion.LookRotation(velocity), 
                Vector3.one * scale
            );
        }
        
        newPositions[index] = position;
        newVelocities[index] = velocity;
    }
}
