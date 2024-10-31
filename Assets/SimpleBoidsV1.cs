using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SimpleBoidsV1 : MonoBehaviour
{
    public List<GameObject> boundaryPointsGameObjects = new List<GameObject>();
    public List<Vector3> boundaryPoints = new List<Vector3>();

    [Header("Point Settings")]
    public GameObject pointPrefab;
    public int numberOfPoints = 100;
    public float moveSpeed = 5f;
    public float directionChangeTime = 2f;

    private List<Transform> points = new List<Transform>();
    private Vector3 centerPoint;
    private float[] nextDirectionChangeTime;
    private Vector3[] targetPositions;

    void Start()
    {
        InitializeBoundary();
        nextDirectionChangeTime = new float[numberOfPoints];
        targetPositions = new Vector3[numberOfPoints];
        SpawnPoints();
    }

    void InitializeBoundary()
    {
        boundaryPoints.Clear();
        foreach (var point in boundaryPointsGameObjects)
        {
            Vector3 position = point.transform.position;
            position.y = 0f;
            boundaryPoints.Add(position);
        }
        centerPoint = boundaryPoints.Aggregate(Vector3.zero, (sum, pos) => sum + pos) / boundaryPoints.Count;
        centerPoint.y = 0f;
    }

    void SpawnPoints()
    {
        for (int i = 0; i < numberOfPoints; i++)
        {
            Vector3 position = GetRandomPositionInBounds();
            GameObject point = Instantiate(pointPrefab, position, Quaternion.identity);
            points.Add(point.transform);
            SetNewRandomTarget(i);
        }
    }

    Vector3 GetRandomPositionInBounds()
    {
        Vector3 position = Vector3.zero;
        bool found = false;
        int attempts = 0;
        
        while (!found && attempts < 100)
        {
            // Find boundary extents
            float minX = boundaryPoints.Min(p => p.x);
            float maxX = boundaryPoints.Max(p => p.x);
            float minZ = boundaryPoints.Min(p => p.z);
            float maxZ = boundaryPoints.Max(p => p.z);

             // Generate random point within boundary rectangle
             position = new Vector3(
                Random.Range(minX, maxX),
                0f,
                Random.Range(minZ, maxZ)
            );
            if (IsPointInPolygon(position))
            {
                found = true;
            }
            attempts++;
        }
        
        return found ? position : centerPoint;
    }

    void Update()
    {
        UpdatePoints();
    }

    void UpdatePoints()
    {
        for (int i = 0; i < points.Count; i++)
        {
            if (Time.time >= nextDirectionChangeTime[i])
            {
                SetNewRandomTarget(i);
            }

            Transform point = points[i];
            Vector3 targetPosition = targetPositions[i];
            
            // Move towards target
            Vector3 direction = (targetPosition - point.position).normalized;
            Vector3 nextPosition = point.position + direction * moveSpeed * Time.deltaTime;
            nextPosition.y = 0f;

            // Check if next position is valid
            if (IsPointInPolygon(nextPosition))
            {
                point.position = nextPosition;
            }
            else
            {
                // If invalid, get new target
                SetNewRandomTarget(i);
            }
        }
    }

    void SetNewRandomTarget(int index)
    {
        targetPositions[index] = GetRandomPositionInBounds();
        nextDirectionChangeTime[index] = Time.time + directionChangeTime + Random.Range(-0.5f, 0.5f);
    }

    // Keep the original winding number algorithm unchanged
    bool IsPointInPolygon(Vector3 point)
    {
        int windingNumber = 0;
        
        for (int i = 0; i < boundaryPoints.Count; i++)
        {
            Vector3 current = boundaryPoints[i];
            Vector3 next = boundaryPoints[(i + 1) % boundaryPoints.Count];

            Vector2 p = new Vector2(point.x, point.z);
            Vector2 v1 = new Vector2(current.x, current.z);
            Vector2 v2 = new Vector2(next.x, next.z);

            windingNumber += CalculateWindingSegment(p, v1, v2);
        }

        return windingNumber != 0;
    }

    int CalculateWindingSegment(Vector2 point, Vector2 vertex1, Vector2 vertex2)
    {
        if (vertex1.y <= point.y)
        {
            if (vertex2.y > point.y)
            {
                if (IsLeft(vertex1, vertex2, point) > 0)
                {
                    return 1;
                }
            }
        }
        else
        {
            if (vertex2.y <= point.y)
            {
                if (IsLeft(vertex1, vertex2, point) < 0)
                {
                    return -1;
                }
            }
        }
        return 0;
    }

    float IsLeft(Vector2 vertex1, Vector2 vertex2, Vector2 point)
    {
        return ((vertex2.x - vertex1.x) * (point.y - vertex1.y) - 
                (point.x - vertex1.x) * (vertex2.y - vertex1.y));
    }

    void OnDrawGizmos()
    {
        if (boundaryPoints == null || boundaryPoints.Count == 0) return;

        Gizmos.color = Color.blue;
        for (int i = 0; i < boundaryPoints.Count; i++)
        {
            Gizmos.DrawLine(boundaryPoints[i], 
                boundaryPoints[(i + 1) % boundaryPoints.Count]);
        }

        // Draw targets in play mode
        if (Application.isPlaying && targetPositions != null)
        {
            Gizmos.color = Color.red;
            foreach (var target in targetPositions)
            {
                Gizmos.DrawWireSphere(target, 0.2f);
            }
        }
    }
}