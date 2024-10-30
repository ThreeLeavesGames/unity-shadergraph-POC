using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WaypointGroup
{
    public Vector3[] points;
    public int fishCount;  // How many fish should follow this path
}

public class OptimizedFishManager : MonoBehaviour
{
    [Header("Fish Settings")]
    public GameObject fishPrefab;
    public int totalFishCount = 300;

    [Header("Movement Settings")]
    public float baseSpeed = 5f;
    public float speedVariation = 2f;
    public float rotationSpeed = 3f;
    public float waypointRadius = 1f;

    // Divide fish into smaller groups to reduce calculations
    public int groupCount = 10;  // Creates 10 different paths
    private WaypointGroup[] waypointGroups;

    void Start()
    {
        InitializeWaypointGroups();
        SpawnFish();
    }

    void InitializeWaypointGroups()
    {
        waypointGroups = new WaypointGroup[groupCount];
        int fishPerGroup = totalFishCount / groupCount;

        for (int i = 0; i < groupCount; i++)
        {
            waypointGroups[i] = new WaypointGroup
            {
                points = GenerateWaypoints(3), // Use 3 points per group
                fishCount = fishPerGroup
            };
        }
    }

    Vector3[] GenerateWaypoints(int count)
    {
        Vector3[] points = new Vector3[count];
        float radius = Random.Range(10f, 20f);
        float yVariation = Random.Range(-5f, 5f);

        for (int i = 0; i < count; i++)
        {
            float angle = ((float)i / count) * 360f * Mathf.Deg2Rad;
            points[i] = new Vector3(
                Mathf.Cos(angle) * radius,
                yVariation + Random.Range(-2f, 2f),
                Mathf.Sin(angle) * radius
            );
        }
        return points;
    }

    void SpawnFish()
    {
        for (int groupIndex = 0; groupIndex < waypointGroups.Length; groupIndex++)
        {
            var group = waypointGroups[groupIndex];
            for (int i = 0; i < group.fishCount; i++)
            {
                Vector3 randomPos = group.points[0] + Random.insideUnitSphere * 2f;
                GameObject fish = Instantiate(fishPrefab, randomPos, Random.rotation);
                var movement = fish.AddComponent<SimpleFishMovement>();
                movement.Initialize(
                    group.points,
                    baseSpeed + Random.Range(-speedVariation, speedVariation),
                    rotationSpeed,
                    waypointRadius,
                    groupIndex * 0.1f // Offset to prevent synchronized movement
                );
            }
        }
    }
}

public class SimpleFishMovement : MonoBehaviour
{
    private Vector3[] waypoints;
    private int currentWaypointIndex;
    private float speed;
    private float rotationSpeed;
    private float waypointRadius;
    private float timeOffset;
    private Vector3 velocity;

    public void Initialize(Vector3[] points, float moveSpeed, float rotSpeed, float wpRadius, float offset)
    {
        waypoints = points;
        speed = moveSpeed;
        rotationSpeed = rotSpeed;
        waypointRadius = wpRadius;
        timeOffset = offset;
        currentWaypointIndex = 0;
    }

    void Update()
    {
        // Add slight movement variation using perlin noise
        float noiseX = Mathf.PerlinNoise(Time.time + timeOffset, 0) * 2f - 1f;
        float noiseY = Mathf.PerlinNoise(0, Time.time + timeOffset) * 2f - 1f;
        Vector3 noiseOffset = new Vector3(noiseX, noiseY, 0) * 0.3f;

        Vector3 targetPoint = waypoints[currentWaypointIndex] + noiseOffset;
        Vector3 direction = (targetPoint - transform.position).normalized;

        // Move towards target
        velocity = Vector3.Lerp(velocity, direction * speed, Time.deltaTime * 5f);
        transform.position += velocity * Time.deltaTime;

        // Rotate towards movement direction
        if (velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                Time.deltaTime * rotationSpeed);
        }

        // Check if reached waypoint
        if (Vector3.Distance(transform.position, targetPoint) < waypointRadius)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }
}