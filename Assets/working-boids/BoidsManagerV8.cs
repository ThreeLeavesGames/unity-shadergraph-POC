using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Random = UnityEngine.Random;


[Serializable]
public class BoidClass
{
    public int hierarchy;
    public int count;
    public float speed;
    public float perceptionRadius;
    public float cohesionWeight;
    public  float seperationWeight;
    public float alignmentWeight;
    public float avoidPredatorWeight;
    public float chaseWeight;
    public float seperationRadius;
    public bool isSoloBoid;
}
public struct BoidParameters
{
    public float speed;
    public float perceptionRadius;
    public float cohesionWeight;
    public float separationWeight;
    public float alignmentWeight;
    public float avoidPredatorWeight;
    public float chaseWeight;
    public float seperationRadius;
    public bool isSoloBoid;
    
}
public class BoidsManagerV8 : MonoBehaviour
{
    public BoidClass[] boids;
    public NativeArray<float3> positions;
    public NativeArray<float3> velocities;
    public NativeArray<float3> newPositions;
    public NativeArray<float3> newVelocities;
    [Header("Polygon Boundary")]
    public Vector3[] polygonPoints;   // Set this in inspector or via code
    private NativeArray<float2> boundaryPoints;  // For use in jobs
    [Header("Anti-Boundary Points")]
    public Vector3[] antiPolygonPoints;   // Set this in inspector or via code
    public NativeArray<float2> antiBoundaryPoints;
    [Header("Mesh Settings")]
    public Mesh boidMesh;
    public Material preyMaterial;
    public Material predatorMaterial;
    private Matrix4x4[] matrices;     
    public NativeArray<Matrix4x4> nativeMatrices;
    public float boundaryTurnForce = 0.5f;
    [SerializeField]private int totalCount = 0;
    [SerializeField]private int[] lengthArray;
    [SerializeField] private NativeArray<int> hierarchyArray;
    void Start()
    {
        
        InitializeBoundaryPoints();
        InitializeArrays();
        SpawnBoids();
    }
    
    void InitializeBoundaryPoints()
    {
        
        boundaryPoints  = new NativeArray<float2>(polygonPoints.Length, Allocator.Persistent);
        for (int i = 0; i < polygonPoints.Length; i++)
        {
            boundaryPoints[i] = new float2(polygonPoints[i].x, polygonPoints[i].z);
        }
        
        antiBoundaryPoints  = new NativeArray<float2>(antiPolygonPoints.Length, Allocator.Persistent);
        for (int i = 0; i < antiPolygonPoints.Length; i++)
        {
            antiBoundaryPoints[i] = new float2(antiPolygonPoints[i].x, antiPolygonPoints[i].z);
        }
    }
     void InitializeArrays()
    {
        lengthArray = new int[boids.Length];
        for (int i = 0; i < boids.Length; i++)
        {
            totalCount += boids[i].count;
            lengthArray[i] = boids[i].count;
        }
       
        hierarchyArray = new NativeArray<int>(totalCount, Allocator.Persistent);
        int currentIndex = 0;
        for (int i = 0; i < boids.Length; i++)
        {
            for (int j = 0; j < boids[i].count; j++)
            {
                hierarchyArray[currentIndex] = boids[i].hierarchy;
                currentIndex++;
            }
        }
       
        matrices = new Matrix4x4[totalCount];
        nativeMatrices = new NativeArray<Matrix4x4>(totalCount, Allocator.Persistent);
        
        positions = new NativeArray<float3>(totalCount, Allocator.Persistent);
        velocities = new NativeArray<float3>(totalCount, Allocator.Persistent);

        newPositions = new NativeArray<float3>(totalCount, Allocator.Persistent);
        newVelocities = new NativeArray<float3>(totalCount, Allocator.Persistent);
        
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
        for (int i = 0; i < totalCount; i++)
        {
            Vector3 randomPos = getRandomPosition(polygonPoints,antiPolygonPoints);
            positions[i] = randomPos;
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
            velocities[i] = new float3(randomDir.x, 0, randomDir.y) * 10f;
            nativeMatrices[i] = Matrix4x4.TRS(randomPos,  UnityEngine.Random.rotation, Vector3.one * 6);
        }
        positions.CopyTo(newPositions);
        velocities.CopyTo(newVelocities);
    }

    void Update()
    {
        NativeArray<BoidParameters> boidParams = new NativeArray<BoidParameters>(boids.Length, Allocator.TempJob);
    
        for (int i = 0; i < boids.Length; i++)
        {
            boidParams[i] = new BoidParameters
            {
                speed = boids[i].speed,
                perceptionRadius = boids[i].perceptionRadius,
                cohesionWeight = boids[i].cohesionWeight,
                separationWeight = boids[i].seperationWeight,
                alignmentWeight = boids[i].alignmentWeight,
                avoidPredatorWeight = boids[i].avoidPredatorWeight,
                chaseWeight = boids[i].chaseWeight,
                seperationRadius = boids[i].seperationRadius,
                isSoloBoid = boids[i].isSoloBoid
            };
        }
        
        BoidUpdateJob boidJob = new BoidUpdateJob
        {
            deltaTime = Time.deltaTime,
            currentPositions = positions,
            currentVelocities = velocities,
            newPositions = newPositions,
            newVelocities = newVelocities,
            hierarchyArray = hierarchyArray,
            boidParams = boidParams,
            // speed = 10f,
            // perceptionRadius = boids[0].perceptionRadius,
            // cohesionWeight = boids[0].cohesionWeight,
            // separationWeight = boids[0].seperationWeight,
            // alignmentWeight = boids[0].alignmentWeight,
            // avoidPredatorWeight = boids[0].avoidPredatorWeight,
            // boundarySize = boundarySize,        
            // softBoundaryOffset = softBoundaryOffset,
            boundaryPoints = boundaryPoints,  
            boundaryTurnForce = boundaryTurnForce,
            antiBoundaryPoints = antiBoundaryPoints,
            // antiBoundaryForce = antiBoundaryForce,
            nativeMatrices = nativeMatrices,
        };
        JobHandle boidJobHandle = boidJob.Schedule(totalCount, 64);
        boidJobHandle.Complete();
        SwapArrays();
        UpdateTransforms();
    }
    void UpdateTransforms()
    {
        for (int i = 0; i < totalCount; i++)
        {
            matrices[i] = nativeMatrices[i];
        }
       
        Graphics.DrawMeshInstanced(boidMesh, 0, predatorMaterial, matrices,matrices.Length);

    }
    void SwapArrays()
    {
        // Copy new positions and velocities to current arrays
        newPositions.CopyTo(positions);
        newVelocities.CopyTo(velocities);
  
    }
    void OnDestroy()
    {
        SafeDispose();

    }

    public void SafeDispose()
    {
        
        // Dispose all NativeArrays if they exist
        if (positions.IsCreated) positions.Dispose();
        if (velocities.IsCreated) velocities.Dispose();
        if (newPositions.IsCreated) newVelocities.Dispose();
        if (nativeMatrices.IsCreated) nativeMatrices.Dispose();
        if (hierarchyArray.IsCreated) hierarchyArray.Dispose();
        if (boundaryPoints.IsCreated) boundaryPoints.Dispose();
        if (antiBoundaryPoints.IsCreated) antiBoundaryPoints.Dispose();
    }
}
[BurstCompile]
public struct BoidUpdateJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float2> boundaryPoints;
    [ReadOnly] public NativeArray<float2> antiBoundaryPoints;  
    [WriteOnly] public NativeArray<Matrix4x4> nativeMatrices;
    [ReadOnly] public NativeArray<BoidParameters> boidParams;
    public float deltaTime;                                    
    [ReadOnly] public NativeArray<float3> currentPositions;    
    [ReadOnly] public NativeArray<float3> currentVelocities;   
    [WriteOnly] public NativeArray<float3> newPositions;       
    [WriteOnly] public NativeArray<float3> newVelocities;      
    [ReadOnly] public NativeArray<int> hierarchyArray; 
    public float boundaryTurnForce;  

    public void Execute(int index)
    {
        float3 position = currentPositions[index];
        float3 velocity = currentVelocities[index];
        int hierarchy = hierarchyArray[index];
        BoidParameters parameters = boidParams[hierarchy];

        // Initialize forces
        float3 cohesion = float3.zero;
        float3 separation = float3.zero;
        float3 alignment = float3.zero;
        float3 predatorAvoidance = float3.zero;
        float3 preyDirection = float3.zero;
        int neighborCount = 0;
        float closestPreyDistance = float.MaxValue;

        // Calculate flocking and hierarchy behaviors
        for (int i = 0; i < currentPositions.Length; i++)
        {
            if (i == index) continue;

            float3 offset = currentPositions[i] - position;
            float sqrDst = math.lengthsq(offset);
            int otherHierarchy = hierarchyArray[i];

            // Same hierarchy - flock together
            if (otherHierarchy == hierarchy)
            {
                float3 normalizedOffset = math.normalize(offset);
                
                // Critical separation zone (very close boids)
                float criticalRadius = parameters.seperationRadius * 0.5f;
                if (sqrDst < criticalRadius * criticalRadius)
                {
                    float urgency = 1.0f - (sqrDst / (criticalRadius * criticalRadius));
                    separation += -normalizedOffset * (urgency * urgency * 3.0f); // Cubic falloff for urgent separation
                }
                // Normal separation zone
                else if (sqrDst < parameters.seperationRadius * parameters.seperationRadius)
                {
                    float separationStrength = 1.0f - (math.sqrt(sqrDst) / parameters.seperationRadius);
                    separation += -normalizedOffset * separationStrength;
                }
                // Flocking zone
                else if (sqrDst < parameters.perceptionRadius * parameters.perceptionRadius)
                {
                    // Only add flocking forces if we're outside separation radius
                    cohesion += currentPositions[i];
                    alignment += currentVelocities[i];
                    neighborCount++;
                }
            }
            // Higher hierarchy - avoid
            else if (otherHierarchy > hierarchy && sqrDst < parameters.perceptionRadius * parameters.perceptionRadius * 4)
            {
                predatorAvoidance += -math.normalize(offset) * (parameters.avoidPredatorWeight / math.sqrt(sqrDst));
            }
            // Lower hierarchy - chase only closest
            else if (otherHierarchy < hierarchy && sqrDst < parameters.perceptionRadius * parameters.perceptionRadius * 2)
            {
                if (sqrDst < closestPreyDistance)
                {
                    closestPreyDistance = sqrDst;
                    preyDirection = math.normalize(offset);
                }
            }
        }

        // Calculate boundary forces
        float2 pos2D = new float2(position.x, position.z);
        float3 boundaryForce = float3.zero;
        
        // Main boundary
        boundaryForce += CalculateBoundaryForce(pos2D, parameters.perceptionRadius * 2, boundaryPoints);
        // Anti-boundary
        boundaryForce += CalculateBoundaryForce(pos2D, parameters.perceptionRadius * 2, antiBoundaryPoints);
        boundaryForce *= boundaryTurnForce;

        // Combine forces with weights and priorities
        float3 finalForce = float3.zero;

        // Add separation force (highest priority)
        finalForce += separation * parameters.separationWeight * 2.5f;
        // Reduce other forces when separation is strong
        float separationMagnitude = math.length(separation);
        float forceDampening = math.max(0.2f, 1.0f - separationMagnitude);
        // Add predator avoidance if present
        if (math.lengthsq(predatorAvoidance) > 0.001f)
        {
            finalForce += predatorAvoidance * 2;
        }

        // Add flocking forces if we have neighbors
        if (neighborCount > 0)
        {
            finalForce += ((cohesion / neighborCount) - position) * parameters.cohesionWeight * forceDampening;
            finalForce += (alignment / neighborCount) * parameters.alignmentWeight * forceDampening;
        }

        // Add chase behavior only if we're not avoiding or separating
        if (closestPreyDistance < float.MaxValue && 
            math.lengthsq(predatorAvoidance) < 0.001f && 
            separationMagnitude < 0.1f)
        {
            finalForce += preyDirection * parameters.chaseWeight * forceDampening;
        }

        // Add boundary force with dampening
        finalForce += boundaryForce * forceDampening;

        // Smoother velocity transition with faster response to separation
        float smoothing = (separationMagnitude > 0.5f) ? 0.5f : 0.1f;
        velocity = math.lerp(velocity, finalForce, deltaTime * smoothing * 10);

        // Normalize and apply speed with context-sensitive adjustments
        float targetSpeed = parameters.speed;
        if (math.lengthsq(predatorAvoidance) > 0.001f)
        {
            targetSpeed *= 1.5f; // Faster when avoiding predators
        }
        else if (closestPreyDistance < float.MaxValue)
        {
            targetSpeed *= 1.2f; // Slightly faster when chasing
        }

        if (math.lengthsq(velocity) > 0.001f)
        {
            velocity = math.normalize(velocity) * targetSpeed;
        }

        // Update position
        velocity.y = 0;
        float3 nextPosition = position + velocity * deltaTime;
        nextPosition.y = 0;

        // Update matrices with smooth rotation
        if (math.lengthsq(velocity) > 0.001f)
        {
            float3 smoothedDir = math.lerp(
                math.normalize(currentVelocities[index]), 
                math.normalize(velocity), 
                deltaTime * 5
            );
            
            nativeMatrices[index] = Matrix4x4.TRS(
                nextPosition,
                Quaternion.LookRotation(smoothedDir),
                Vector3.one * (6 * (hierarchy + 1))
            );
        }

        newPositions[index] = nextPosition;
        newVelocities[index] = velocity;
    }

    private float3 CalculateBoundaryForce(float2 position, float radius, NativeArray<float2> points)
    {
        float3 force = float3.zero;

        for (int i = 0; i < points.Length; i++)
        {
            float2 lineStart = points[i];
            float2 lineEnd = points[(i + 1) % points.Length];
            float2 line = lineEnd - lineStart;
            float len = math.length(line);
            float2 lineDir = line / len;
            float t = math.dot(position - lineStart, lineDir);
            t = math.clamp(t, 0, len);
            float2 closestPoint = lineStart + lineDir * t;
            float dist = math.distance(position, closestPoint);

            if (dist < radius)
            {
                float2 awayDir = math.normalize(position - closestPoint);
                float weight = 1 - (dist / radius);
                weight = weight * weight; // Squared falloff
                force += new float3(awayDir.x, 0, awayDir.y) * weight;
            }
        }

        return force;
    }
}
