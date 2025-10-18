using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Random = UnityEngine.Random;



public class BoidsManagerV13 : MonoBehaviour
{
    [Header("Polygon Boundary")]
    public Vector3[] polygonPoints;   // Set this in inspector or via code
    private NativeArray<float2> boundaryPoints;  // For use in jobs
    
    [Header("Boundary Settings")]
    public float boundaryTurnForce = 0.5f;   // Force applied to turn away from boundary (V7 working value)
    public bool debugBoundaryForces = false; // Show boundary force vectors in scene view
    
    [Header("Anti-Boundary Points")]
    public Vector3[][] antiPolygonPoints;   // Array of arrays of anti-boundary points
    public NativeArray<float2>[] antiBoundaryPointsArrays;
    public float[] antiBoundaryForces;
    
    // Flattened arrays for job system
    private NativeArray<float2> allAntiBoundaryPoints;
    private NativeArray<int> antiBoundaryStartIndices;
    private NativeArray<int> antiBoundaryCounts;
    private NativeArray<float> nativeAntiBoundaryForces;

    [Header("Mesh Settings")]
    public Mesh boidMesh;
    public Material[] rankMaterials; // Array of materials for different ranks
    private Matrix4x4[] boidMatrices;     
    public NativeArray<Matrix4x4> nativeBoidMatrices;
    
    [Header("Spawn Settings")]
    public GameObject boidPrefab;
    public int totalBoidCount = 100;
    public int[] rankCounts = {80, 15, 5}; // Count for each rank (rank 0, rank 1, rank 2)

    [Header("Per-Rank Boid Settings")] public RankBoidSettings[] rankSettings = new RankBoidSettings[]
    {
        new RankBoidSettings
        {
            speed = 4f, perceptionRadius = 2f, cohesionWeight = 1.2f, separationWeight = 1.8f, alignmentWeight = 1f,
            chaseWeight = 0.5f, fleeWeight = 3f, scale = 4f, debugColor = Color.green
        },
        new RankBoidSettings
        {
            speed = 6f, perceptionRadius = 3f, cohesionWeight = 1f, separationWeight = 1.5f, alignmentWeight = 1.2f,
            chaseWeight = 1.5f, fleeWeight = 2f, scale = 8f, debugColor = Color.yellow
        },
        new RankBoidSettings
        {
            speed = 8f, perceptionRadius = 4f, cohesionWeight = 0.8f, separationWeight = 1.2f, alignmentWeight = 1.5f,
            chaseWeight = 3f, fleeWeight = 1f, scale = 12f, debugColor = Color.red
        }
    };
    

    
    [Header("Natural Movement")]
    [Range(1f, 20f)] public float rotationSmoothness = 8f;   // How smoothly boids rotate (higher = smoother)
    [Range(1f, 20f)] public float maxVelocityChange = 5f;    // Maximum velocity change per frame (prevents sudden turns)
    
    [Header("Escape Detection")]
    public bool enableEscapeDetection = true;              // Enable automatic detection and fixing of escaped boids
    [Range(10, 120)] public int detectionFrequency = 30;   // Check every N frames (30 = ~2x per second at 60fps)
    public bool teleportEscapedBoids = true;              // Teleport escaped boids to valid positions
    public bool logEscapeEvents = false;                   // Debug logging of escape events
    private int frameCounter = 0;                          // Frame counter for detection timing

    // Native arrays for current frame data (persistent memory)
    public NativeArray<float3> boidPositions;      // Current boid positions
    public NativeArray<float3> boidVelocities;     // Current boid velocities
    public NativeArray<int> boidRanks;             // Boid ranks (0=lowest, higher=more dominant)
    public NativeArray<quaternion> boidRotations;  // Current boid rotations for smooth interpolation

    // Double-buffered arrays for next frame data (prevents race conditions)
    public NativeArray<float3> newBoidPositions;     // Next frame boid positions
    public NativeArray<float3> newBoidVelocities;    // Next frame boid velocities
    public NativeArray<quaternion> newBoidRotations; // Next frame boid rotations
    private JobHandle boidHandle;
    
    // Persistent rank settings to avoid per-frame allocation
    private NativeArray<RankSettingsData> persistentRankSettings;
    private bool rankSettingsNeedUpdate = true;
    
    private MaterialPropertyBlock[] rankPropertyBlocks;
    private ComputeBuffer[] rankPropertyBuffers;
    
    // Memory pooling for matrix arrays to reduce GC pressure
    private Dictionary<int, Matrix4x4[]> matrixPools = new Dictionary<int, Matrix4x4[]>();
    
    // Performance profiling
    [Header("Performance Metrics")]
    [SerializeField] private float currentFrameTime;
    [SerializeField] private float averageFrameTime;
    private float frameTimeSum;
    private int frameCount;
    
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
        
        // Initialize multiple anti-boundary arrays
        if (antiPolygonPoints != null && antiPolygonPoints.Length > 0)
        {
            antiBoundaryPointsArrays = new NativeArray<float2>[antiPolygonPoints.Length];
            
            // Initialize forces array if not set
            if (antiBoundaryForces == null || antiBoundaryForces.Length != antiPolygonPoints.Length)
            {
                antiBoundaryForces = new float[antiPolygonPoints.Length];
                for (int i = 0; i < antiBoundaryForces.Length; i++)
                {
                    antiBoundaryForces[i] = 1.0f; // Default anti-boundary force
                }
            }
            
            // Calculate total points and create flattened arrays
            int totalPoints = 0;
            List<int> startIndices = new List<int>();
            List<int> counts = new List<int>();
            
            for (int arrayIndex = 0; arrayIndex < antiPolygonPoints.Length; arrayIndex++)
            {
                if (antiPolygonPoints[arrayIndex] != null && antiPolygonPoints[arrayIndex].Length > 0)
                {
                    startIndices.Add(totalPoints);
                    counts.Add(antiPolygonPoints[arrayIndex].Length);
                    totalPoints += antiPolygonPoints[arrayIndex].Length;
                    
                    antiBoundaryPointsArrays[arrayIndex] = new NativeArray<float2>(antiPolygonPoints[arrayIndex].Length, Allocator.Persistent);
                    for (int i = 0; i < antiPolygonPoints[arrayIndex].Length; i++)
                    {
                        antiBoundaryPointsArrays[arrayIndex][i] = new float2(antiPolygonPoints[arrayIndex][i].x, antiPolygonPoints[arrayIndex][i].z);
                    }
                }
                else
                {
                    startIndices.Add(totalPoints);
                    counts.Add(0);
                }
            }
            
            // Create flattened arrays for job system
            allAntiBoundaryPoints = new NativeArray<float2>(totalPoints, Allocator.Persistent);
            antiBoundaryStartIndices = new NativeArray<int>(startIndices.Count, Allocator.Persistent);
            antiBoundaryCounts = new NativeArray<int>(counts.Count, Allocator.Persistent);
            nativeAntiBoundaryForces = new NativeArray<float>(antiBoundaryForces.Length, Allocator.Persistent);
            
            int flatIndex = 0;
            for (int arrayIndex = 0; arrayIndex < antiPolygonPoints.Length; arrayIndex++)
            {
                antiBoundaryStartIndices[arrayIndex] = startIndices[arrayIndex];
                antiBoundaryCounts[arrayIndex] = counts[arrayIndex];
                nativeAntiBoundaryForces[arrayIndex] = antiBoundaryForces[arrayIndex];
                
                if (antiPolygonPoints[arrayIndex] != null && antiPolygonPoints[arrayIndex].Length > 0)
                {
                    for (int i = 0; i < antiPolygonPoints[arrayIndex].Length; i++)
                    {
                        allAntiBoundaryPoints[flatIndex++] = new float2(antiPolygonPoints[arrayIndex][i].x, antiPolygonPoints[arrayIndex][i].z);
                    }
                }
            }
        }
        
        // Initialize persistent rank settings
        InitializePersistentRankSettings();
    }
    
    void InitializePersistentRankSettings()
    {
        if (persistentRankSettings.IsCreated)
            persistentRankSettings.Dispose();
        
        persistentRankSettings = new NativeArray<RankSettingsData>(rankSettings.Length, Allocator.Persistent);
        rankSettingsNeedUpdate = true;
        UpdatePersistentRankSettings();
    }
    
    // Helper method to visualize the boundary in the editor
    void OnDrawGizmos()
    {
        // Draw outer boundary (main containment)
        if (polygonPoints != null && polygonPoints.Length > 2)
        {
            Gizmos.color = Color.yellow; // Yellow = outer boundary
            for (int i = 0; i < polygonPoints.Length; i++)
            {
                Vector3 current = polygonPoints[i];
                Vector3 next = polygonPoints[(i + 1) % polygonPoints.Length];
                Gizmos.DrawLine(current, next);
                
                // Draw boundary influence radius if debugging
                if (debugBoundaryForces && Application.isPlaying)
                {
                    Gizmos.color = Color.yellow * 0.3f;
                    Vector3 lineCenter = (current + next) * 0.5f;
                    float maxRadius = rankSettings.Length > 0 ? rankSettings[0].perceptionRadius * 2f : 5f;
                    Gizmos.DrawWireSphere(lineCenter, maxRadius);
                }
            }
        }
        
        // Draw multiple inner boundaries (obstacles)
        if (antiPolygonPoints != null)
        {
            Color[] colors = { Color.red, Color.magenta, Color.cyan, Color.yellow };
            
            for (int arrayIndex = 0; arrayIndex < antiPolygonPoints.Length; arrayIndex++)
            {
                Vector3[] points = antiPolygonPoints[arrayIndex];
                if (points != null && points.Length > 2)
                {
                    Gizmos.color = colors[arrayIndex % colors.Length];
                    
                    for (int i = 0; i < points.Length; i++)
                    {
                        Vector3 current = points[i];
                        Vector3 next = points[(i + 1) % points.Length];
                        Gizmos.DrawLine(current, next);
                        
                        // Draw anti-boundary influence radius if debugging
                        if (debugBoundaryForces && Application.isPlaying)
                        {
                            Gizmos.color = colors[arrayIndex % colors.Length] * 0.3f;
                            Vector3 lineCenter = (current + next) * 0.5f;
                            float maxRadius = rankSettings.Length > 0 ? rankSettings[0].perceptionRadius : 2.5f;
                            Gizmos.DrawWireSphere(lineCenter, maxRadius);
                        }
                    }
                }
            }
        }
        
        // Draw boundary forces for first few boids if debugging
        if (debugBoundaryForces && Application.isPlaying && boidPositions.IsCreated)
        {
            Gizmos.color = Color.magenta;
            int debugCount = math.min(5, totalBoidCount); // Only show first 5 boids
            for (int i = 0; i < debugCount; i++)
            {
                Vector3 pos = boidPositions[i];
                Gizmos.DrawWireSphere(pos, 0.2f);
                
                // Show perception radius
                if (i < boidRanks.Length && boidRanks[i] < rankSettings.Length)
                {
                    Gizmos.color = Color.green * 0.2f;
                    float perceptionRadius = rankSettings[boidRanks[i]].perceptionRadius;
                    Gizmos.DrawWireSphere(pos, perceptionRadius);
                }
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
        
        // Ensure persistent rank settings are initialized
        if (!persistentRankSettings.IsCreated)
            InitializePersistentRankSettings();
        
        // Reorganize boids to be in rank order
        int targetIndex = 0;
        
        // First pass: place existing boids in rank order
        for (int rank = 0; rank < rankCounts.Length; rank++)
        {
            int boidsPlacedForRank = 0;
            
            // Look for existing boids of this rank
            for (int i = 0; i < preserveCount && boidsPlacedForRank < rankCounts[rank]; i++)
            {
                if (tempRanks[i] == rank)
                {
                    boidPositions[targetIndex] = tempPositions[i];
                    boidVelocities[targetIndex] = tempVelocities[i];
                    boidRanks[targetIndex] = rank;
                    nativeBoidMatrices[targetIndex] = tempMatrices[i];
                    targetIndex++;
                    boidsPlacedForRank++;
                }
            }
            
            // Fill remaining slots for this rank with new boids
            while (boidsPlacedForRank < rankCounts[rank] && targetIndex < totalBoidCount)
            {
                Vector3 randomPos = getRandomPosition(polygonPoints, antiPolygonPoints);
                boidPositions[targetIndex] = randomPos;
                Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
                
                // Use rank-specific speed and scale
                float rankSpeed = (rank < rankSettings.Length) ? rankSettings[rank].speed : 5f;
                float scale = (rank < rankSettings.Length) ? rankSettings[rank].scale : (6 + rank * 4);
                
                boidVelocities[targetIndex] = new float3(randomDir.x, 0, randomDir.y) * rankSpeed;
                boidRanks[targetIndex] = rank;
                nativeBoidMatrices[targetIndex] = Matrix4x4.TRS(randomPos, UnityEngine.Random.rotation, Vector3.one * scale);
                
                targetIndex++;
                boidsPlacedForRank++;
            }
        }
        
        // Initialize double buffer arrays
        boidPositions.CopyTo(newBoidPositions);
        boidVelocities.CopyTo(newBoidVelocities);
        boidRotations.CopyTo(newBoidRotations);
        
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
        boidRotations = new NativeArray<quaternion>(totalBoidCount, Allocator.Persistent);

        // Initialize temporary arrays for double buffering
        newBoidPositions = new NativeArray<float3>(totalBoidCount, Allocator.Persistent);
        newBoidVelocities = new NativeArray<float3>(totalBoidCount, Allocator.Persistent);
        newBoidRotations = new NativeArray<quaternion>(totalBoidCount, Allocator.Persistent);
        
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

    private Vector3 getRandomPosition(Vector3[] outerBoundary, Vector3[][] innerBoundaries)
    {
        float minX = float.MaxValue, minZ = float.MaxValue;
        float maxX = float.MinValue, maxZ = float.MinValue;
    
        for (int j = 0; j < outerBoundary.Length; j++)
        {
            minX = math.min(minX, outerBoundary[j].x);
            maxX = math.max(maxX, outerBoundary[j].x);
            minZ = math.min(minZ, outerBoundary[j].z);
            maxZ = math.max(maxZ, outerBoundary[j].z);
        }
        
        float2 testPoint;
        do
        {
            bool insideAnyInnerBoundary;
            do
            {
                float randX = UnityEngine.Random.Range(minX, maxX);
                float randZ = UnityEngine.Random.Range(minZ, maxZ);
                testPoint = new float2(randX, randZ);
                
                // Check if point is inside any inner boundary (obstacle)
                insideAnyInnerBoundary = false;
                if (innerBoundaries != null)
                {
                    foreach (Vector3[] innerBoundary in innerBoundaries)
                    {
                        if (innerBoundary != null && innerBoundary.Length > 2 &&
                            PolygonUtility.IsPointInPolygon(testPoint, innerBoundary))
                        {
                            insideAnyInnerBoundary = true;
                            break;
                        }
                    }
                }
            } while (insideAnyInnerBoundary);
              
        } while (!PolygonUtility.IsPointInPolygon(testPoint, outerBoundary));

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
                boidRanks[boidIndex] = rank; 
                
                // Use rank-specific speed and scale
                float rankSpeed = (rank < rankSettings.Length) ? rankSettings[rank].speed : 5f;
                float scale = (rank < rankSettings.Length) ? rankSettings[rank].scale : (6 + rank * 4);
                
                boidVelocities[boidIndex] = new float3(randomDir.x, 0, randomDir.y) * rankSpeed;
                
                // Initialize rotation based on initial velocity with safety checks
                quaternion initialRotation = quaternion.identity;
                float3 initialVel = boidVelocities[boidIndex];
                if (math.lengthsq(initialVel) > 0.001f)
                {
                    float3 normalizedVel = math.normalize(initialVel);
                    if (math.isfinite(normalizedVel.x) && math.isfinite(normalizedVel.y) && math.isfinite(normalizedVel.z))
                    {
                        initialRotation = quaternion.LookRotation(normalizedVel, new float3(0, 1, 0));
                        
                        // Validate and normalize
                        if (!math.isfinite(initialRotation.value.x) || !math.isfinite(initialRotation.value.y) || 
                            !math.isfinite(initialRotation.value.z) || !math.isfinite(initialRotation.value.w))
                        {
                            initialRotation = quaternion.identity;
                        }
                        else
                        {
                            initialRotation = math.normalize(initialRotation);
                        }
                    }
                }
                
                boidRotations[boidIndex] = initialRotation;
                nativeBoidMatrices[boidIndex] = Matrix4x4.TRS(randomPos, initialRotation, Vector3.one * scale);
                
                boidIndex++;
            }
        }

        // Initialize double buffer arrays with starting values and validate rotations
        boidPositions.CopyTo(newBoidPositions);
        boidVelocities.CopyTo(newBoidVelocities);
        
        // Copy and validate rotations
        for (int i = 0; i < boidRotations.Length; i++)
        {
            quaternion rot = boidRotations[i];
            if (!math.isfinite(rot.value.x) || !math.isfinite(rot.value.y) || 
                !math.isfinite(rot.value.z) || !math.isfinite(rot.value.w))
            {
                boidRotations[i] = quaternion.identity;
            }
            else
            {
                boidRotations[i] = math.normalize(rot);
            }
        }
        boidRotations.CopyTo(newBoidRotations);
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
            
            // Check if mouse is inside outer boundary
            bool insideOuterBoundary = PolygonUtility.IsPointInPolygon(mousePos2D, polygonPoints);
            
            // Check if mouse is inside any inner boundary (obstacle)
            bool insideAnyInnerBoundary = false;
            if (antiPolygonPoints != null)
            {
                foreach (Vector3[] innerBoundary in antiPolygonPoints)
                {
                    if (innerBoundary != null && innerBoundary.Length > 2 &&
                        PolygonUtility.IsPointInPolygon(mousePos2D, innerBoundary))
                    {
                        insideAnyInnerBoundary = true;
                        break;
                    }
                }
            }
            
            isMouseInBoundary = insideOuterBoundary && !insideAnyInnerBoundary;
            
            // Update mouse position array
            mouseWorldPosition = isMouseInBoundary ? tempMouseWorldPosition : float3.zero;
        }
        
        UpdateBoidsPositions();
        UpdateTransforms();
        
        // Handle escaped boids detection
        if (enableEscapeDetection)
        {
            frameCounter++;
            if (frameCounter >= detectionFrequency)
            {
                frameCounter = 0;
                HandleEscapedBoids();
            }
        }
    }

    /// <summary>
    /// Detects and handles escaped boids (outside outer boundary or inside obstacles)
    /// Called every detectionFrequency frames for optimization
    /// </summary>
    void HandleEscapedBoids()
    {
        int escapedCount = 0;
        
        for (int i = 0; i < totalBoidCount; i++)
        {
            float2 pos2D = new float2(boidPositions[i].x, boidPositions[i].z);
            bool needsRelocation = false;
            
            // Check if outside outer boundary
            bool outsideOuter = !PolygonUtility.IsPointInPolygon(pos2D, polygonPoints);
            
            if (outsideOuter)
            {
                needsRelocation = true;
                if (logEscapeEvents)
                    Debug.Log($"Boid {i} (rank {boidRanks[i]}) escaped outer boundary at {boidPositions[i]}");
            }
            else
            {
                // Check if inside any obstacle (only if inside outer boundary)
                if (antiPolygonPoints != null)
                {
                    foreach (Vector3[] obstacle in antiPolygonPoints)
                    {
                        if (obstacle != null && obstacle.Length > 2 &&
                            PolygonUtility.IsPointInPolygon(pos2D, obstacle))
                        {
                            needsRelocation = true;
                            if (logEscapeEvents)
                                Debug.Log($"Boid {i} (rank {boidRanks[i]}) entered obstacle at {boidPositions[i]}");
                            break;
                        }
                    }
                }
            }
            
            if (needsRelocation)
            {
                escapedCount++;
                
                if (teleportEscapedBoids)
                {
                    // Teleport to valid random position
                    Vector3 newPos = getRandomPosition(polygonPoints, antiPolygonPoints);
                    boidPositions[i] = newPos;
                    newBoidPositions[i] = newPos;
                    
                    // Reset velocity to prevent immediate re-escape
                    Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
                    int rank = boidRanks[i];
                    float speed = (rank < rankSettings.Length) ? rankSettings[rank].speed : 5f;
                    
                    // Start with reduced speed to prevent immediate re-escape
                    boidVelocities[i] = new float3(randomDir.x, 0, randomDir.y) * speed * 0.3f;
                    newBoidVelocities[i] = boidVelocities[i];
                    
                    // Reset rotation
                    quaternion newRotation = quaternion.LookRotation(boidVelocities[i], new float3(0, 1, 0));
                    boidRotations[i] = newRotation;
                    newBoidRotations[i] = newRotation;
                    
                    if (logEscapeEvents)
                        Debug.Log($"Relocated boid {i} to {newPos}");
                }
            }
        }
        
        if (escapedCount > 0 && logEscapeEvents)
            Debug.Log($"HandleEscapedBoids: Found and handled {escapedCount} escaped boids");
    }

    void UpdatePersistentRankSettings()
    {
        if (!rankSettingsNeedUpdate) return;
        
        // Initialize or resize persistent array if needed
        if (!persistentRankSettings.IsCreated || persistentRankSettings.Length != rankSettings.Length)
        {
            if (persistentRankSettings.IsCreated)
                persistentRankSettings.Dispose();
            
            persistentRankSettings = new NativeArray<RankSettingsData>(rankSettings.Length, Allocator.Persistent);
        }
        
        // Update the persistent array
        for (int i = 0; i < rankSettings.Length; i++)
        {
            persistentRankSettings[i] = new RankSettingsData
            {
                speed = rankSettings[i].speed,
                perceptionRadius = rankSettings[i].perceptionRadius,
                cohesionWeight = rankSettings[i].cohesionWeight,
                separationWeight = rankSettings[i].separationWeight,
                alignmentWeight = rankSettings[i].alignmentWeight,
                chaseWeight = rankSettings[i].chaseWeight,
                fleeWeight = rankSettings[i].fleeWeight,
                scale = rankSettings[i].scale
            };
        }
        
        rankSettingsNeedUpdate = false;
    }
    
    /// <summary>
    /// Call this when rank settings are modified to trigger update
    /// </summary>
    public void MarkRankSettingsForUpdate()
    {
        rankSettingsNeedUpdate = true;
    }
    
    public void UpdateBoidsPositions()
    {
        float startTime = Time.realtimeSinceStartup;
        
        boidHandle.Complete();
        
        // Ensure persistent rank settings are initialized
        if (!persistentRankSettings.IsCreated)
            InitializePersistentRankSettings();
        
        // Update persistent rank settings only when needed
        UpdatePersistentRankSettings();
        
        // Create rank-based boid update job
        RankBoidUpdateJobV13 boidJob = new RankBoidUpdateJobV13
        {
            deltaTime = Time.deltaTime,
            currentPositions = boidPositions,
            currentVelocities = boidVelocities,
            currentRanks = boidRanks,
            currentRotations = boidRotations,
            newPositions = newBoidPositions,
            newVelocities = newBoidVelocities,
            newRotations = newBoidRotations,
            rankSettingsData = persistentRankSettings,
            boundaryPoints = boundaryPoints,
            boundaryTurnForce = boundaryTurnForce,
            allAntiBoundaryPoints = allAntiBoundaryPoints,
            antiBoundaryStartIndices = antiBoundaryStartIndices,
            antiBoundaryCounts = antiBoundaryCounts,
            antiBoundaryForces = nativeAntiBoundaryForces,
            nativeBoidMatrices = nativeBoidMatrices,
            mouseWorldPosition = new float3(mouseWorldPosition.x, mouseWorldPosition.y, mouseWorldPosition.z),
            isMouseActive = isMouseInBoundary,
            mouseAttractionWeight = mouseAttractionWeight,
            rotationSmoothness = rotationSmoothness,
            maxVelocityChange = maxVelocityChange,
        };

        // Calculate optimal batch size based on boid count
        int batchSize = Mathf.Max(1, Mathf.Min(totalBoidCount / 4, 128));
        
        // Schedule the job
        boidHandle = boidJob.Schedule(totalBoidCount, batchSize);

        // Wait for job to complete
        boidHandle.Complete();
        
        // Using persistent array - no disposal needed per frame

        // Update arrays for next frame
        SwapArrays();
        
        // Performance profiling
        currentFrameTime = (Time.realtimeSinceStartup - startTime) * 1000f; // Convert to ms
        frameTimeSum += currentFrameTime;
        frameCount++;
        
        if (frameCount >= 60) // Update average every 60 frames
        {
            averageFrameTime = frameTimeSum / frameCount;
            frameTimeSum = 0f;
            frameCount = 0;
        }
    }

    /// <summary>
    /// Swaps double buffered arrays to prepare for next frame
    /// </summary>
    void SwapArrays()
    {
        // Copy new positions, velocities, and rotations to current arrays
        newBoidPositions.CopyTo(boidPositions);
        newBoidVelocities.CopyTo(boidVelocities);
        newBoidRotations.CopyTo(boidRotations);
    }

    /// <summary>
    /// Gets a pooled matrix array of the specified size to reduce GC pressure
    /// </summary>
    private Matrix4x4[] GetPooledMatrixArray(int size)
    {
        if (!matrixPools.ContainsKey(size) || matrixPools[size] == null)
        {
            matrixPools[size] = new Matrix4x4[size];
        }
        return matrixPools[size];
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

        // Draw boids by rank (efficient method - assumes boids are stored in rank order)
        int matrixIndex = 0;
        for (int rank = 0; rank < rankCounts.Length && rank < rankMaterials.Length; rank++)
        {
            if (rankCounts[rank] > 0)
            {
                Matrix4x4[] rankMatrices = GetPooledMatrixArray(rankCounts[rank]);
                System.Array.Copy(boidMatrices, matrixIndex, rankMatrices, 0, rankCounts[rank]);
                
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
        if (boidRotations.IsCreated) boidRotations.Dispose();
        if (newBoidPositions.IsCreated) newBoidPositions.Dispose();
        if (newBoidVelocities.IsCreated) newBoidVelocities.Dispose();
        if (newBoidRotations.IsCreated) newBoidRotations.Dispose();
        if (boundaryPoints.IsCreated) boundaryPoints.Dispose();
        // Dispose multiple anti-boundary arrays
        if (antiBoundaryPointsArrays != null)
        {
            for (int i = 0; i < antiBoundaryPointsArrays.Length; i++)
            {
                if (antiBoundaryPointsArrays[i].IsCreated)
                    antiBoundaryPointsArrays[i].Dispose();
            }
        }
        
        // Dispose flattened arrays
        if (allAntiBoundaryPoints.IsCreated) allAntiBoundaryPoints.Dispose();
        if (antiBoundaryStartIndices.IsCreated) antiBoundaryStartIndices.Dispose();
        if (antiBoundaryCounts.IsCreated) antiBoundaryCounts.Dispose();
        if (nativeAntiBoundaryForces.IsCreated) nativeAntiBoundaryForces.Dispose();
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
        
        // Dispose persistent rank settings
        if (persistentRankSettings.IsCreated)
            persistentRankSettings.Dispose();
    }
}


[BurstCompile]
public struct RankBoidUpdateJobV13 : IJobParallelFor
{
    [ReadOnly] public NativeArray<float2> boundaryPoints;
    [ReadOnly] public NativeArray<float2> allAntiBoundaryPoints; // Flattened array
    [ReadOnly] public NativeArray<int> antiBoundaryStartIndices;  // Start index of each array
    [ReadOnly] public NativeArray<int> antiBoundaryCounts;       // Count of points in each array
    [ReadOnly] public NativeArray<float> antiBoundaryForces;
    [WriteOnly] public NativeArray<Matrix4x4> nativeBoidMatrices;
    
    // Time and position/velocity data
    public float deltaTime;
    [ReadOnly] public NativeArray<float3> currentPositions;
    [ReadOnly] public NativeArray<float3> currentVelocities;
    [ReadOnly] public NativeArray<int> currentRanks;
    [ReadOnly] public NativeArray<quaternion> currentRotations;
    [WriteOnly] public NativeArray<float3> newPositions;
    [WriteOnly] public NativeArray<float3> newVelocities;
    [WriteOnly] public NativeArray<quaternion> newRotations;
    
    // Boundary parameters
    public float boundaryTurnForce;
    
    // Per-rank settings
    [ReadOnly] public NativeArray<RankSettingsData> rankSettingsData;
    
    public float3 mouseWorldPosition;
    public bool isMouseActive;
    public float mouseAttractionWeight;
    
    // Natural movement parameters
    public float rotationSmoothness;
    public float maxVelocityChange;
   
    private bool IsPointInPolygonWinding(float2 point, NativeArray<float2> polygon)
    {
        if (polygon.Length < 3) return false;
        
        int winding = 0;
        
        for (int i = 0; i < polygon.Length; i++)
        {
            float2 vertex1 = polygon[i];
            float2 vertex2 = polygon[(i + 1) % polygon.Length];
            
            if (vertex1.y <= point.y)
            {
                if (vertex2.y > point.y && IsLeftOfLine(vertex1, vertex2, point) > 0)
                    winding++;
            }
            else
            {
                if (vertex2.y <= point.y && IsLeftOfLine(vertex1, vertex2, point) < 0)
                    winding--;
            }
        }
        
        return winding != 0;
    }
    
    private float IsLeftOfLine(float2 lineStart, float2 lineEnd, float2 point)
    {
        return (lineEnd.x - lineStart.x) * (point.y - lineStart.y) - (point.x - lineStart.x) * (lineEnd.y - lineStart.y);
    }
    
    
    public void Execute(int index)
    {
        float3 position = currentPositions[index];
        float3 velocity = currentVelocities[index];
        int myRank = currentRanks[index];
        quaternion currentRotation = currentRotations[index];
        
        // Get rank-specific settings
        RankSettingsData mySettings = (myRank < rankSettingsData.Length) ? 
            rankSettingsData[myRank] : rankSettingsData[0];

        // Initialize forces
        float3 cohesion = float3.zero;    
        float3 separation = float3.zero;   
        float3 alignment = float3.zero;    
        float3 rankInteraction = float3.zero;
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

            if (sqrDst < mySettings.perceptionRadius * mySettings.perceptionRadius)
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
                    rankInteraction += math.normalize(-offset) * (mySettings.fleeWeight / math.sqrt(sqrDst));
                }
                else if (myRank > otherRank) // Other boid has lower rank (less dominant)
                {
                    // Chase lower rank
                    rankInteraction += math.normalize(offset) * (mySettings.chaseWeight / (sqrDst + 1.0f));
                }
            }
        }

        if (neighborCount > 0)
        {
            cohesion = (cohesion / neighborCount - position) * mySettings.cohesionWeight;
            separation = separation * mySettings.separationWeight;
            alignment = (alignment / neighborCount) * mySettings.alignmentWeight;
        }   

        // BOUNDARY LOGIC FROM V7 (WORKING VERSION)
        // Calculate boundary line avoidance (outer boundary)
        float2 pos2D = new float2(position.x, position.z);
        float avoidanceRadius = mySettings.perceptionRadius * 2; // Distance at which to start avoiding boundaries

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
        
        // Calculate anti-boundary line avoidance (inner obstacles) - multiple arrays
        float2 pos2DAnti = new float2(position.x, position.z);
        float avoidanceRadiusAnti = mySettings.perceptionRadius; // Radius for anti-boundary
        float3 antiBoundaryForceLocal = float3.zero;

        // Process each anti-boundary array separately
        for (int arrayIndex = 0; arrayIndex < antiBoundaryStartIndices.Length; arrayIndex++)
        {
            int startIndex = antiBoundaryStartIndices[arrayIndex];
            int count = antiBoundaryCounts[arrayIndex];
            float currentForce = antiBoundaryForces[arrayIndex];
            
            if (count <= 0) continue;
            
            for (int i = 0; i < count; i++)
            {
                int currentIdx = startIndex + i;
                int nextIdx = startIndex + ((i + 1) % count);
                
                float2 lineStart = allAntiBoundaryPoints[currentIdx];
                float2 lineEnd = allAntiBoundaryPoints[nextIdx];
            
                // Find closest point on line segment
                float2 line = lineEnd - lineStart;
                float len = math.length(line);
                if (len < 0.001f) continue; // Skip degenerate lines
                
                float2 lineDir = line / len;
            
                float t = math.dot(pos2DAnti - lineStart, lineDir);
                t = math.clamp(t, 0, len);
            
                float2 closestPoint = lineStart + lineDir * t;
                float dist = math.distance(pos2DAnti, closestPoint);
            
                // Apply avoidance force if close to line
                if (dist < avoidanceRadiusAnti && dist > 0.001f)
                {
                    float2 awayDir = math.normalize(pos2DAnti - closestPoint);
                    // Use force specific to this anti-boundary array
                    float strength = currentForce / (dist * dist);
                    antiBoundaryForceLocal += new float3(awayDir.x, 0, awayDir.y) * strength;
                }
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

        // Calculate desired velocity from all forces
        float3 desiredVelocity = velocity + cohesion + separation + alignment + rankInteraction + avoidBoundary + 
                               antiBoundaryForceLocal + (mouseAttraction * 50);

        // Limit velocity change per frame for natural movement
        float3 velocityChange = desiredVelocity - velocity;
        float velocityChangeMagnitude = math.length(velocityChange);
        if (velocityChangeMagnitude > maxVelocityChange)
        {
            velocityChange = math.normalize(velocityChange) * maxVelocityChange;
        }
        
        velocity = velocity + velocityChange;

        // Normalize and apply rank-specific speed
        velocity.y = 0;
        if (math.lengthsq(velocity) > 0.001f)
        {
            velocity = math.normalize(velocity) * mySettings.speed;
        }

        // Update position
        float3 nextPosition = position + velocity * deltaTime;
        nextPosition.y = 0;
        
        // Simple position update like V7 - let forces handle boundaries
        position = nextPosition;
        
        // Final velocity normalization with minimum speed preservation
        float currentSpeed = math.length(velocity);
        if (currentSpeed < 0.001f)
        {
            // If velocity is nearly zero, give it a random direction
            velocity = new float3(
                math.sin(index * 0.123f + deltaTime),
                0,
                math.cos(index * 0.456f + deltaTime)
            ) * mySettings.speed;
        }
        else
        {
            velocity = math.normalize(velocity) * mySettings.speed;
        }
        velocity.y = 0;

        // Smooth rotation interpolation with safety checks
        quaternion targetRotation = currentRotation;
        if (math.lengthsq(velocity) > 0.001f)
        {
            // Ensure velocity is normalized and valid
            float3 normalizedVelocity = math.normalize(velocity);
            if (math.isfinite(normalizedVelocity.x) && math.isfinite(normalizedVelocity.y) && math.isfinite(normalizedVelocity.z))
            {
                targetRotation = quaternion.LookRotation(normalizedVelocity, new float3(0, 1, 0));
                
                // Validate target rotation
                if (!math.isfinite(targetRotation.value.x) || !math.isfinite(targetRotation.value.y) || 
                    !math.isfinite(targetRotation.value.z) || !math.isfinite(targetRotation.value.w))
                {
                    targetRotation = currentRotation; // Fallback to current rotation
                }
            }
        }
        
        // Validate current rotation
        if (!math.isfinite(currentRotation.value.x) || !math.isfinite(currentRotation.value.y) || 
            !math.isfinite(currentRotation.value.z) || !math.isfinite(currentRotation.value.w))
        {
            currentRotation = quaternion.identity;
        }
        
        // Normalize quaternions to prevent drift
        currentRotation = math.normalize(currentRotation);
        targetRotation = math.normalize(targetRotation);
        
        // Smoothly interpolate to target rotation
        float rotationSpeed = math.clamp(rotationSmoothness * deltaTime, 0f, 1f);
        quaternion smoothRotation = math.slerp(currentRotation, targetRotation, rotationSpeed);
        
        // Final validation and normalization
        smoothRotation = math.normalize(smoothRotation);
        
        // Validate smooth rotation before using
        if (!math.isfinite(smoothRotation.value.x) || !math.isfinite(smoothRotation.value.y) || 
            !math.isfinite(smoothRotation.value.z) || !math.isfinite(smoothRotation.value.w))
        {
            smoothRotation = quaternion.identity; // Safe fallback
        }
        
        // Update matrix with validated smooth rotation
        nativeBoidMatrices[index] = Matrix4x4.TRS(
            position, 
            smoothRotation, 
            Vector3.one * mySettings.scale
        );
        
        newPositions[index] = position;
        newVelocities[index] = velocity;
        newRotations[index] = smoothRotation;
    }
}
