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
    private int instanceCount = 100;
    public List<Transform> targetPositions;
    public float speed = 2f;
    private Vector3[] positions;

    private NativeArray<Vector3> nativePositions;
    private NativeArray<Vector3> nativeCurrentTargets;
    private NativeArray<Vector3> nativeTargetPositions;
    private NativeArray<Matrix4x4> nativeMatrices;
    private int maxInstanceCount; // Maximum capacity for arrays

    void Start()
    {
        maxInstanceCount = instanceCount + 1000; // Add buffer for growth
        InitializeArrays();
        StartCoroutine(MoveBirds());
    }

    void InitializeArrays()
    {
        // Dispose of any existing native arrays if they are created
        DisposeArrays();

        // Allocate new arrays with maximum capacity
        matrices = new Matrix4x4[maxInstanceCount];
        positions = new Vector3[maxInstanceCount];

        nativePositions = new NativeArray<Vector3>(maxInstanceCount, Allocator.Persistent);
        nativeCurrentTargets = new NativeArray<Vector3>(maxInstanceCount, Allocator.Persistent);
        nativeMatrices = new NativeArray<Matrix4x4>(maxInstanceCount, Allocator.Persistent);
        nativeTargetPositions = new NativeArray<Vector3>(targetPositions.Count, Allocator.Persistent);

        // Copy target positions
        for (int i = 0; i < targetPositions.Count; i++)
        {
            nativeTargetPositions[i] = targetPositions[i].position;
        }

        // Initialize initial instances
        InitializeInstances(0, instanceCount);
    }

    private void InitializeInstances(int startIndex, int endIndex)
    {
        Vector3 position = CalculateStartPosition(startIndex);

        for (int i = startIndex; i < endIndex; i++)
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

            positions[i] = position;
            nativePositions[i] = position;

            int randomTargetIndex = Random.Range(0, targetPositions.Count);
            nativeCurrentTargets[i] = nativeTargetPositions[randomTargetIndex];

            nativeMatrices[i] = Matrix4x4.TRS(position, Quaternion.Euler(90, 0, 0), Vector3.one);
        }
    }

    private Vector3 CalculateStartPosition(int startIndex)
    {
        // Calculate the grid position based on the start index
        int rowIndex = startIndex / 100;
        return new Vector3(-198 * rowIndex, 2 * rowIndex, 0);
    }

    public void SetInstanceCount(int newCount)
    {
        if (newCount == instanceCount) return;

        // Check if we need to resize our arrays
        if (newCount > maxInstanceCount)
        {
            // Save existing data
            var oldPositions = nativePositions.ToArray();
            var oldTargets = nativeCurrentTargets.ToArray();
            var oldMatrices = nativeMatrices.ToArray();

            // Update maximum capacity
            maxInstanceCount = newCount + 1000;

            // Reinitialize arrays with new capacity
            DisposeArrays();
            InitializeArrays();

            // Restore existing data
            for (int i = 0; i < instanceCount; i++)
            {
                nativePositions[i] = oldPositions[i];
                nativeCurrentTargets[i] = oldTargets[i];
                nativeMatrices[i] = oldMatrices[i];
            }
        }

        // Initialize only the new instances if we're increasing
        if (newCount > instanceCount)
        {
            InitializeInstances(instanceCount, newCount);
        }

        instanceCount = newCount;
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
            MoveBirdsJob moveBirdsJob = new MoveBirdsJob
            {
                positions = nativePositions,
                currentTargets = nativeCurrentTargets,
                targetPositions = nativeTargetPositions,
                matrices = nativeMatrices,
                speed = speed,
                deltaTime = Time.deltaTime,
                randomSeed = (uint)Random.Range(1, 100000)
            };

            JobHandle moveBirdsJobHandle = moveBirdsJob.Schedule(instanceCount, 64);
            moveBirdsJobHandle.Complete();

            // Copy only the active instances for rendering
            for (int i = 0; i < instanceCount; i++)
            {
                matrices[i] = nativeMatrices[i];
            }

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

        public float speed;
        public float deltaTime;
        public uint randomSeed;

        public void Execute(int index)
        {
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