using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using System;

public class Createbirds : MonoBehaviour
{
    public Mesh birdMesh;                   // The mesh for the bird
    public Material birdMaterial;           // The material for the bird
    private Matrix4x4[] matrices;           // Transformation matrices for instancing
    private int instanceCount = 30000;       // Number of instances to render
    public List<Transform> targetPositions; // List of target positions for the birds
    public float speed = 2f;                // Movement speed of the birds
    private Vector3[] positions;            // Positions of each bird

    private NativeArray<Vector3> nativePositions;
    private NativeArray<Vector3> nativeCurrentTargets;
    private NativeArray<Vector3> nativeTargetPositions;
    private NativeArray<Matrix4x4> nativeMatrices;

    void Start()
    {
        matrices = new Matrix4x4[instanceCount];
        positions = new Vector3[instanceCount];

        // Allocate native arrays
        nativePositions = new NativeArray<Vector3>(instanceCount, Allocator.Persistent);
        nativeCurrentTargets = new NativeArray<Vector3>(instanceCount, Allocator.Persistent);
        nativeMatrices = new NativeArray<Matrix4x4>(instanceCount, Allocator.Persistent);
        nativeTargetPositions = new NativeArray<Vector3>(targetPositions.Count, Allocator.Persistent);

        // Copy target positions from MonoBehaviour list to NativeArray
        for (int i = 0; i < targetPositions.Count; i++)
        {
            nativeTargetPositions[i] = targetPositions[i].position;
        }

        // Initialize positions and targets
        Vector3 position = new Vector3(0, 0, 0);
        for (int i = 0; i < instanceCount; i++)
        {
            if (i % 100 == 0)
            {
                position.x -= 198;
                position.y += 2;
            }
            else
            {
                position.x += 2;
            }

            // Set initial positions and pick random initial targets
            positions[i] = position;
            nativePositions[i] = position;

            int randomTargetIndex = UnityEngine.Random.Range(0, targetPositions.Count);
            nativeCurrentTargets[i] = nativeTargetPositions[randomTargetIndex];

            // Initialize matrices
            nativeMatrices[i] = Matrix4x4.TRS(position, Quaternion.Euler(90, 0, 0), Vector3.one);
        }

        StartCoroutine(MoveBirds()); // Start moving the birds
    }

    private IEnumerator MoveBirds()
    {
        while (true)
        {
            // Schedule the job for moving birds
            MoveBirdsJob moveBirdsJob = new MoveBirdsJob
            {
                positions = nativePositions,
                currentTargets = nativeCurrentTargets,
                targetPositions = nativeTargetPositions,
                matrices = nativeMatrices,
                speed = speed,
                deltaTime = Time.deltaTime,
                randomSeed = (uint)UnityEngine.Random.Range(1, 100000) // Random seed for the job
            };

            JobHandle moveBirdsJobHandle = moveBirdsJob.Schedule(instanceCount, 64);
            moveBirdsJobHandle.Complete(); // Ensure job is done before continuing

            // Copy matrices back to the main thread for rendering
            for (int i = 0; i < instanceCount; i++)
            {
                matrices[i] = nativeMatrices[i]; // Update the matrix for rendering
            }

            // Render all instances using GPU instancing
            Graphics.DrawMeshInstanced(birdMesh, 0, birdMaterial, matrices, instanceCount);
            yield return null; // Wait until the next frame
        }
    }

    void OnDestroy()
    {
        // Dispose of native arrays to prevent memory leaks
        if (nativePositions.IsCreated) nativePositions.Dispose();
        if (nativeCurrentTargets.IsCreated) nativeCurrentTargets.Dispose();
        if (nativeMatrices.IsCreated) nativeMatrices.Dispose();
        if (nativeTargetPositions.IsCreated) nativeTargetPositions.Dispose();
    }

    [BurstCompile]
    struct MoveBirdsJob : IJobParallelFor
    {
        public NativeArray<Vector3> positions;
        public NativeArray<Vector3> currentTargets;
        [ReadOnly] public NativeArray<Vector3> targetPositions;
        public NativeArray<Matrix4x4> matrices;

        public float speed;
        public float deltaTime;
        public uint randomSeed; // For safe random number generation inside the job

        private Vector3 pickNewTarget(int index)
        {
            // Pick a new target from the available target positions
            Unity.Mathematics.Random random = new Unity.Mathematics.Random(randomSeed + (uint)index);
            int randomIndex = random.NextInt(0, targetPositions.Length);

            


            return targetPositions[randomIndex];
        }

        public void Execute(int index)
        {
            Vector3 targetPosition = currentTargets[index];
            positions[index] = Vector3.MoveTowards(positions[index], targetPosition, speed * deltaTime);

            // Check if the bird reached the target
            if (Vector3.Distance(positions[index], targetPosition) < 0.1f)
            {
                currentTargets[index] = pickNewTarget(index);

                float currentPosx = positions[index].x;
                float currentPosz = positions[index].z;
                float targetPosx = currentTargets[index].x;
                float targetPosz = currentTargets[index].z;

                if(currentPosx == currentPosx || currentPosz == targetPosz)
                {
                    currentTargets[index] = pickNewTarget(index);

                }

            }

            // Set transformation matrix with rotation (90, 0, 0)
            matrices[index] = Matrix4x4.TRS(positions[index], Quaternion.Euler(90, 0, 0), Vector3.one);
        }
    }
}
