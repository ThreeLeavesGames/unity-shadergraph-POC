using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Controls the creation and movement of multiple bird instances using Unity's Job System for performance optimization
/// </summary>
public class CreateBirdsV2 : MonoBehaviour
{
    // References to the bird's visual components
    public Mesh birdMesh;                    // The 3D mesh used for each bird
    public Material birdMaterial;            // The material applied to each bird

    // Arrays to store transformation data
    private Matrix4x4[] matrices;            // Stores position, rotation, and scale for each bird
    private int instanceCount = 500;         // Current number of active bird instances
    private int totalCapacity = 0;           // Maximum capacity of the arrays
    public List<Transform> targetPositions;  // List of positions birds can fly towards
    public float speed = 2f;                // Movement speed of the birds
    private Vector3[] positions;             // Array to store bird positions

    // Native arrays for Unity Jobs System
    private NativeArray<Vector3> nativePositions;         // Bird positions in native format
    private NativeArray<Vector3> nativeCurrentTargets;    // Current target for each bird
    private NativeArray<Vector3> nativeTargetPositions;   // All possible target positions
    private NativeArray<Matrix4x4> nativeMatrices;        // Transformation matrices in native format
    private bool needsReinitialize = false;               // Flag to indicate if arrays need reinitialization

    /// <summary>
    /// Initialize the system and start the bird movement coroutine
    /// </summary>
    void Start()
    {
        totalCapacity = instanceCount;
        InitializeArrays();
        StartCoroutine(MoveBirds());
    }

    /// <summary>
    /// Initialize all arrays and copy target positions into native arrays
    /// </summary>
    void InitializeArrays()
    {
        // Clean up any existing native arrays
        DisposeArrays();

        // Initialize new arrays with current capacity
        matrices = new Matrix4x4[totalCapacity];
        positions = new Vector3[totalCapacity];

        // Create native arrays for the Jobs system
        nativePositions = new NativeArray<Vector3>(totalCapacity, Allocator.Persistent);
        nativeCurrentTargets = new NativeArray<Vector3>(totalCapacity, Allocator.Persistent);
        nativeMatrices = new NativeArray<Matrix4x4>(totalCapacity, Allocator.Persistent);
        nativeTargetPositions = new NativeArray<Vector3>(targetPositions.Count, Allocator.Persistent);

        // Copy target positions to native array
        for (int i = 0; i < targetPositions.Count; i++)
        {
            nativeTargetPositions[i] = targetPositions[i].position;
        }

        // Set up initial instance positions and data
        InitializeInstances(0, instanceCount);
        needsReinitialize = false;
    }

    /// <summary>
    /// Initialize a specific range of bird instances with positions and targets
    /// </summary>
    /// <param name="startIndex">Starting index for initialization</param>
    /// <param name="count">Number of instances to initialize</param>
    private void InitializeInstances(int startIndex, int count)
    {
        // Set initial position (continue from last position if not starting from 0)
        Vector3 position = Vector3.zero;
        if (startIndex > 0)
        {
            position = positions[startIndex - 1];
            position.x += 2;
            if (startIndex % 100 == 0)
            {
                position.x = 0;
                position.y += 2;
            }
        }

        int endIndex = startIndex + count;

        // Initialize each instance
        for (int i = startIndex; i < endIndex; i++)
        {
            // Create a new row every 100 birds
            if ((i - startIndex) % 100 == 0 && i != startIndex)
            {
                position.x = 0;
                position.y += 2;
            }

            // Set position and target for this instance
            positions[i] = position;
            nativePositions[i] = position;

            // Assign random target position
            int randomTargetIndex = Random.Range(0, targetPositions.Count);
            nativeCurrentTargets[i] = nativeTargetPositions[randomTargetIndex];

            // Create transformation matrix
            matrices[i] = Matrix4x4.TRS(position, Quaternion.Euler(90, 0, 0), Vector3.one);
            nativeMatrices[i] = matrices[i];

            position.x += 2; // Space birds 2 units apart
        }

        Debug.Log($"Initialized birds from {startIndex} to {endIndex - 1}");
    }

    /// <summary>
    /// Add more bird instances to the system, resizing arrays if necessary
    /// </summary>
    /// <param name="additionalCount">Number of new instances to add</param>
    public void AddInstances(int additionalCount)
    {
        if (additionalCount <= 0) return;

        int newTotalCount = instanceCount + additionalCount;
        Debug.Log($"Adding {additionalCount} instances. Current: {instanceCount}, New Total: {newTotalCount}");

        // Resize arrays if necessary
        if (newTotalCount > totalCapacity)
        {
            // Backup existing data
            Vector3[] oldPositions = new Vector3[instanceCount];
            Vector3[] oldTargets = new Vector3[instanceCount];
            Matrix4x4[] oldMatrices = new Matrix4x4[instanceCount];

            // Copy current data to backup
            for (int i = 0; i < instanceCount; i++)
            {
                oldPositions[i] = nativePositions[i];
                oldTargets[i] = nativeCurrentTargets[i];
                oldMatrices[i] = nativeMatrices[i];
            }

            // Update capacity and reinitialize arrays
            totalCapacity = newTotalCount;
            DisposeArrays();

            // Create new arrays with increased capacity
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

        // Update instance count and flag for reinitialization
        instanceCount = newTotalCount;
        needsReinitialize = true;

        Debug.Log($"Finished adding instances. Total count: {instanceCount}");
    }

    /// <summary>
    /// Clean up native arrays to prevent memory leaks
    /// </summary>
    private void DisposeArrays()
    {
        if (nativePositions.IsCreated) nativePositions.Dispose();
        if (nativeCurrentTargets.IsCreated) nativeCurrentTargets.Dispose();
        if (nativeMatrices.IsCreated) nativeMatrices.Dispose();
        if (nativeTargetPositions.IsCreated) nativeTargetPositions.Dispose();
    }

    /// <summary>
    /// Coroutine to continuously update and render bird positions
    /// </summary>
    private IEnumerator MoveBirds()
    {
        while (true)
        {
            // Check if arrays need to be resized
            if (needsReinitialize)
            {
                if (matrices.Length < instanceCount)
                {
                    matrices = new Matrix4x4[instanceCount];
                }
                needsReinitialize = false;
            }

            // Create and schedule the bird movement job
            MoveBirdsJob moveBirdsJob = new MoveBirdsJob
            {
                positions = nativePositions,
                currentTargets = nativeCurrentTargets,
                targetPositions = nativeTargetPositions,
                matrices = nativeMatrices,
                speed = speed,
                deltaTime = Time.deltaTime,
                randomSeed = (uint)Random.Range(1, 100000),
                instanceCount = instanceCount
            };

            // Execute the job and wait for completion
            JobHandle moveBirdsJobHandle = moveBirdsJob.Schedule(instanceCount, 64);
            moveBirdsJobHandle.Complete();

            // Copy updated matrices for rendering
            for (int i = 0; i < instanceCount; i++)
            {
                matrices[i] = nativeMatrices[i];
            }

            // Render all bird instances
            Graphics.DrawMeshInstanced(birdMesh, 0, birdMaterial, matrices, instanceCount);

            yield return null;
        }
    }

    /// <summary>
    /// Clean up native arrays when the component is destroyed
    /// </summary>
    void OnDestroy()
    {
        DisposeArrays();
    }

    /// <summary>
    /// Job struct for parallel processing of bird movement
    /// </summary>
    [BurstCompile]
    struct MoveBirdsJob : IJobParallelFor
    {
        public NativeArray<Vector3> positions;           // Current positions
        public NativeArray<Vector3> currentTargets;      // Current target positions
        [ReadOnly] public NativeArray<Vector3> targetPositions;  // All possible target positions
        public NativeArray<Matrix4x4> matrices;          // Transformation matrices
        public int instanceCount;                        // Current number of active instances

        public float speed;                              // Movement speed
        public float deltaTime;                          // Time since last frame
        public uint randomSeed;                          // Seed for random number generation

        /// <summary>
        /// Process movement for a single bird instance
        /// </summary>
        /// <param name="index">Index of the bird to process</param>
        public void Execute(int index)
        {
            if (index >= instanceCount) return; // Skip inactive instances

            // Move towards current target
            Vector3 targetPosition = currentTargets[index];
            positions[index] = Vector3.MoveTowards(positions[index], targetPosition, speed * deltaTime);

            // Check if target reached and assign new target if necessary
            if (Vector3.Distance(positions[index], targetPosition) < 0.1f)
            {
                Unity.Mathematics.Random random = new Unity.Mathematics.Random(randomSeed + (uint)index);
                int randomIndex = random.NextInt(0, targetPositions.Length);
                currentTargets[index] = targetPositions[randomIndex];
            }

            // Update transformation matrix
            matrices[index] = Matrix4x4.TRS(positions[index], Quaternion.Euler(90, 0, 0), Vector3.one);
        }
    }
}