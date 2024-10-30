using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Createbirds : MonoBehaviour
{
    public Mesh birdMesh;                   // The mesh for the bird
    public Material birdMaterial;           // The material for the bird
    private Matrix4x4[] matrices;           // Transformation matrices for instancing
    private int instanceCount = 5000;       // Number of instances to render
    public List<Transform> targetPositions;  // List of target positions for the birds
    public float speed = 2f;                 // Movement speed of the birds
    private Vector3[] currentTargets;        // Array to hold current targets for each bird
    private Vector3[] positions;              // Positions of each bird

    void Start()
    {
        matrices = new Matrix4x4[instanceCount];
        currentTargets = new Vector3[instanceCount];
        positions = new Vector3[instanceCount];

        // Set up transformation matrices for each bird and pick initial targets
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

            // Create transformation matrix for this instance
            matrices[i] = Matrix4x4.TRS(position, Quaternion.Euler(90, 0, 0), Vector3.one);
            positions[i] = position; // Store the initial position
            PickNewTarget(i);         // Pick an initial target for each bird
        }

        StartCoroutine(MoveBirds()); // Start moving the birds
    }

    private IEnumerator MoveBirds()
    {
        while (true)
        {
            for (int i = 0; i < instanceCount; i++)
            {
                MoveTowardsTarget(i); // Move each bird towards its target
            }

            // Update matrices based on new positions
            for (int i = 0; i < instanceCount; i++)
            {
                matrices[i] = Matrix4x4.TRS(positions[i], Quaternion.Euler(90, 0, 0), Vector3.one);
            }

            // Render all instances using GPU instancing
            Graphics.DrawMeshInstanced(birdMesh, 0, birdMaterial, matrices, instanceCount);
            yield return null; // Wait until the next frame before continuing
        }
    }

    private void MoveTowardsTarget(int index)
    {
        // Ensure the bird maintains a fixed Y-axis level
        Vector3 targetPosition = new Vector3(currentTargets[index].x, currentTargets[index].y, currentTargets[index].z);
        positions[index] = Vector3.MoveTowards(positions[index], targetPosition, speed * Time.deltaTime);

        // Check if the bird reached the target position
        if (Vector3.Distance(positions[index], targetPosition) < 0.1f)
        {
            PickNewTarget(index); // Pick a new target once the bird reaches the current one
        }
    }

    private void PickNewTarget(int index)
    {
        if (targetPositions.Count > 0)
        {
            int randomIndex = Random.Range(0, targetPositions.Count);
            currentTargets[index] = targetPositions[randomIndex].position;
        }
    }
}
