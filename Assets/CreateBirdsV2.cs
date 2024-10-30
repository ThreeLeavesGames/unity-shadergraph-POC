using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System.Collections;
using System.Collections.Generic;

public class CreateBirdsV2 : MonoBehaviour
{
    public Mesh birdMesh;
    public Material birdMaterial;
    private Matrix4x4[] matrices;
    private int instanceCount = 500;
    private int totalCapacity = 0;
    public List<Transform> targetPositions;
    public float speed = 2f;
    private Vector3[] positions;

    private NativeArray<Vector3> nativePositions;
    private NativeArray<Vector3> nativeCurrentTargets;
    private NativeArray<Vector3> nativeTargetPositions;
    private NativeArray<Matrix4x4> nativeMatrices;
    private bool needsReinitialize = false;

    void Start()
    {
        totalCapacity = instanceCount;
        InitializeArrays();
        StartCoroutine(MoveBirds());
    }

    void InitializeArrays()
    {
        // Dispose of any existing native arrays if they are created
        DisposeArrays();

        // Allocate new arrays with current capacity
        matrices = new Matrix4x4[totalCapacity];
        positions = new Vector3[totalCapacity];

        nativePositions = new NativeArray<Vector3>(totalCapacity, Allocator.Persistent);
        nativeCurrentTargets = new NativeArray<Vector3>(totalCapacity, Allocator.Persistent);
        nativeMatrices = new NativeArray<Matrix4x4>(totalCapacity, Allocator.Persistent);
        nativeTargetPositions = new NativeArray<Vector3>(targetPositions.Count, Allocator.Persistent);

        // Copy target positions
        for (int i = 0; i < targetPositions.Count; i++)
        {
            nativeTargetPositions[i] = targetPositions[i].position;
        }

        // Initialize initial instances
        InitializeInstances(0, instanceCount);
        needsReinitialize = false;
    }

    private void InitializeInstances(int startIndex, int count)
    {
        Vector3 position = Vector3.zero;
        if (startIndex > 0)
        {
            // Get the last position of the previous instance
            position = positions[startIndex - 1];
            // Move to the next grid position
            position.x += 2;
            if (startIndex % 100 == 0)
            {
                position.x = 0;
                position.y += 2;
            }
        }

        int endIndex = startIndex + count;

        for (int i = startIndex; i < endIndex; i++)
        {
            if ((i - startIndex) % 100 == 0 && i != startIndex)
            {
                position.x = 0;
                position.y += 2;
            }

            positions[i] = position;
            nativePositions[i] = position;

            int randomTargetIndex = Random.Range(0, targetPositions.Count);
            nativeCurrentTargets[i] = nativeTargetPositions[randomTargetIndex];

            matrices[i] = Matrix4x4.TRS(position, Quaternion.Euler(90, 0, 0), Vector3.one);
            nativeMatrices[i] = matrices[i];

            position.x += 2;
        }

        Debug.Log($"Initialized birds from {startIndex} to {endIndex - 1}");
    }

    public void AddInstances(int additionalCount)
    {
        if (additionalCount <= 0) return;

        int newTotalCount = instanceCount + additionalCount;
        Debug.Log($"Adding {additionalCount} instances. Current: {instanceCount}, New Total: {newTotalCount}");

        // If we need more capacity
        if (newTotalCount > totalCapacity)
        {
            // Save existing data
            Vector3[] oldPositions = new Vector3[instanceCount];
            Vector3[] oldTargets = new Vector3[instanceCount];
            Matrix4x4[] oldMatrices = new Matrix4x4[instanceCount];

            // Copy existing data
            for (int i = 0; i < instanceCount; i++)
            {
                oldPositions[i] = nativePositions[i];
                oldTargets[i] = nativeCurrentTargets[i];
                oldMatrices[i] = nativeMatrices[i];
            }

            // Update capacity
            totalCapacity = newTotalCount;

            // Reinitialize arrays with new capacity
            DisposeArrays();

            // Reallocate arrays with new capacity
            matrices = new Matrix4x4[totalCapacity];
            positions = new Vector3[totalCapacity];

            nativePositions = new NativeArray<Vector3>(totalCapacity, Allocator.Persistent);
            nativeCurrentTargets = new NativeArray<Vector3>(totalCapacity, Allocator.Persistent);
            nativeMatrices = new NativeArray<Matrix4x4>(totalCapacity, Allocator.Persistent);
            nativeTargetPositions = new NativeArray<Vector3>(targetPositions.Count, Allocator.Persistent);

            // Restore target positions
            for (int i = 0; i < targetPositions.Count; i++)
            {
                nativeTargetPositions[i] = targetPositions[i].position;
            }

            // Restore existing instance data
            for (int i = 0; i < instanceCount; i++)
            {
                positions[i] = oldPositions[i];
                nativePositions[i] = oldPositions[i];
                nativeCurrentTargets[i] = oldTargets[i];
                matrices[i] = oldMatrices[i];
                nativeMatrices[i] = oldMatrices[i];
            }

            Debug.Log($"Reallocated arrays with new capacity: {totalCapacity}");
        }

        // Initialize new instances
        InitializeInstances(instanceCount, additionalCount);

        // Update instance count
        instanceCount = newTotalCount;
        needsReinitialize = true;

        Debug.Log($"Finished adding instances. Total count: {instanceCount}");
    }

    private void DisposeArrays()
    {
        if (nativePositions.IsCreated) nativePositions.Dispose();
        if (nativeCurrentTargets.IsCreated) nativeCurrentTargets.Dispose();
        if (nativeMatrices.IsCreated) nativeMatrices.Dispose();
        if (nativeTargetPositions.IsCreated) nativeTargetPositions.Dispose();
    }

    private IEnumerator MoveBirds()
    {
        while (true)
        {
            if (needsReinitialize)
            {
                // Update matrices array size if needed
                if (matrices.Length < instanceCount)
                {
                    matrices = new Matrix4x4[instanceCount];
                }
                needsReinitialize = false;
            }

            MoveBirdsJob moveBirdsJob = new MoveBirdsJob
            {
                positions = nativePositions,
                currentTargets = nativeCurrentTargets,
                targetPositions = nativeTargetPositions,
                matrices = nativeMatrices,
                speed = speed,
                deltaTime = Time.deltaTime,
                randomSeed = (uint)Random.Range(1, 100000),
                instanceCount = instanceCount // Pass the current instance count to the job
            };

            JobHandle moveBirdsJobHandle = moveBirdsJob.Schedule(instanceCount, 64);
            moveBirdsJobHandle.Complete();

            // Copy only the active instances for rendering
            for (int i = 0; i < instanceCount; i++)
            {
                matrices[i] = nativeMatrices[i];
            }

            // Draw all instances
            Graphics.DrawMeshInstanced(birdMesh, 0, birdMaterial, matrices, instanceCount);

            yield return null;
        }
    }

    void OnDestroy()
    {
        DisposeArrays();
    }

    [BurstCompile]
    struct MoveBirdsJob : IJobParallelFor
    {
        public NativeArray<Vector3> positions;
        public NativeArray<Vector3> currentTargets;
        [ReadOnly] public NativeArray<Vector3> targetPositions;
        public NativeArray<Matrix4x4> matrices;
        public int instanceCount; // Added instance count to the job

        public float speed;
        public float deltaTime;
        public uint randomSeed;

        public void Execute(int index)
        {
            if (index >= instanceCount) return; // Skip if beyond current instance count

            Vector3 targetPosition = currentTargets[index];
            positions[index] = Vector3.MoveTowards(positions[index], targetPosition, speed * deltaTime);

            if (Vector3.Distance(positions[index], targetPosition) < 0.1f)
            {
                Unity.Mathematics.Random random = new Unity.Mathematics.Random(randomSeed + (uint)index);
                int randomIndex = random.NextInt(0, targetPositions.Length);
                currentTargets[index] = targetPositions[randomIndex];
            }

            matrices[index] = Matrix4x4.TRS(positions[index], Quaternion.Euler(90, 0, 0), Vector3.one);
        }
    }
}