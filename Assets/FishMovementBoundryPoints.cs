using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FishMovementBoundryPoints : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField] private float swimSpeed = 15f;
    [SerializeField] private float turnSpeed = 3f;
    [SerializeField] private float wanderRadius = 20f;
    [SerializeField] private float boundaryPadding = 5f;
    
    [Header("Natural Movement")]
    [SerializeField] private float directionChangeInterval = 1.5f;
    [SerializeField] private float boundaryAvoidanceStrength = 3f;
    public List<GameObject> boundaryPointsGameObjects = new List<GameObject>();
    
    public List<Vector3> boundaryPoints = new List<Vector3>();
    private Vector3 targetPoint;
    private float directionTimer;
    private Vector3 centerPoint;
    private float boundaryRadius;
    private bool isMoving = false;

    void Start()
    {
        InitializeBoundary();
        SetInitialPosition();
        isMoving = true;
    }

    void InitializeBoundary()
    {
        boundaryPoints.Clear();
        for (int i = 0; i < boundaryPointsGameObjects.Count; i++)
        {
            boundaryPoints.Add(boundaryPointsGameObjects[i].transform.position);
        }
        CalculatePondMetrics();
    }

    void SetInitialPosition()
    {
        transform.position = centerPoint;
        SetNewRandomTarget();
    }

    void Update()
    {
        if (!isMoving) return;

        UpdateTimer();
        UpdateMovement();
    }

    void UpdateTimer()
    {
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0)
        {
            SetNewRandomTarget();
            directionTimer = Random.Range(directionChangeInterval * 0.5f, directionChangeInterval * 1.5f);
        }
    }

    void UpdateMovement()
    {
        Vector3 currentPosition = transform.position;
        Vector3 moveDirection = (targetPoint - currentPosition).normalized;
        
        // Calculate next position
        Vector3 nextPosition = currentPosition + moveDirection * swimSpeed * Time.deltaTime;
        nextPosition.y = currentPosition.y;

        // Check if next position is inside using winding number
        if (IsPointInPolygon(nextPosition))
        {
            transform.position = nextPosition;
        }
        else
        {
            // If outside, steer back towards center
            Vector3 toCenter = (centerPoint - currentPosition).normalized;
            transform.position += toCenter * swimSpeed * Time.deltaTime;
            SetNewRandomTarget(); // Get new target when hitting boundary
        }

        // Update rotation
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    bool IsPointInPolygon(Vector3 point)
    {
        int windingNumber = 0;
        
        for (int i = 0; i < boundaryPoints.Count; i++)
        {
            Vector3 current = boundaryPoints[i];
            Vector3 next = boundaryPoints[(i + 1) % boundaryPoints.Count];

            // Convert to 2D by ignoring Y (assuming boundary is on XZ plane)
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

    void SetNewRandomTarget()
    {
        for (int attempts = 0; attempts < 10; attempts++)
        {
            float randomAngle = Random.Range(0f, 360f);
            float randomDistance = Random.Range(wanderRadius * 0.3f, wanderRadius);
            Vector3 randomDirection = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
            Vector3 newTarget = centerPoint + randomDirection * randomDistance;
            newTarget.y = transform.position.y;

            // Only set target if it's inside the polygon
            if (IsPointInPolygon(newTarget))
            {
                targetPoint = newTarget;
                return;
            }
        }

        // Fallback to center if no valid point found
        targetPoint = centerPoint;
    }

    void CalculatePondMetrics()
    {
        if (boundaryPoints.Count == 0)
        {
            Debug.LogError("No boundary points found!");
            return;
        }

        centerPoint = boundaryPoints.Aggregate(Vector3.zero, (sum, point) => sum + point) / boundaryPoints.Count;
        boundaryRadius = boundaryPoints.Max(point => Vector3.Distance(centerPoint, point));
    }

    void OnDrawGizmos()
    {
        if (boundaryPoints == null || boundaryPoints.Count == 0) return;

        // Draw boundary
        Gizmos.color = Color.blue;
        for (int i = 0; i < boundaryPoints.Count; i++)
        {
            Gizmos.DrawSphere(boundaryPoints[i], 0.5f);
            Gizmos.DrawLine(boundaryPoints[i], 
                boundaryPoints[(i + 1) % boundaryPoints.Count]);
        }

        if (Application.isPlaying)
        {
            // Draw target and debug info
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetPoint, 0.5f);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetPoint);
            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(centerPoint, 1f);
        }
    }
}