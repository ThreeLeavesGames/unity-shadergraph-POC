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

    void Start()
    {
        // Initialize boundary points
        for (int i = 0; i < boundaryPointsGameObjects.Count; i++)
        {
            boundaryPoints.Add(boundaryPointsGameObjects[i].transform.position);
        }
        
        CalculatePondMetrics();
        
        // Set initial position if not already in pond
        if (!IsPointInPond(transform.position))
        {
            transform.position = centerPoint;
        }
        
        SetNewRandomTarget();
    }

    void CalculatePondMetrics()
    {
        centerPoint = boundaryPoints.Aggregate(Vector3.zero, (sum, point) => sum + point) / boundaryPoints.Count;
        boundaryRadius = boundaryPoints.Max(point => Vector3.Distance(centerPoint, point));
    }

    void Update()
    {
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0)
        {
            SetNewRandomTarget();
            directionTimer = Random.Range(directionChangeInterval * 0.5f, directionChangeInterval * 1.5f);
        }

        // Calculate desired direction
        Vector3 moveDirection = (targetPoint - transform.position).normalized;
        Vector3 avoidanceForce = CalculateBoundaryAvoidance();
        Vector3 finalDirection = (moveDirection + avoidanceForce * boundaryAvoidanceStrength).normalized;

        // Only rotate and move if we have a valid direction
        if (finalDirection != Vector3.zero)
        {
            // Keep Y position constant
            float currentY = transform.position.y;
            
            // Update position
            Vector3 newPosition = transform.position + finalDirection * swimSpeed * Time.deltaTime;
            newPosition.y = currentY;
            
            // Check if new position is valid
            if (IsPointInPond(newPosition))
            {
                transform.position = newPosition;
            }
            else
            {
                // If invalid, move towards center instead
                Vector3 toCenterDirection = (centerPoint - transform.position).normalized;
                transform.position += toCenterDirection * swimSpeed * Time.deltaTime;
            }

            // Update rotation
            Quaternion targetRotation = Quaternion.LookRotation(finalDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    Vector3 CalculateBoundaryAvoidance()
    {
        Vector3 avoidanceForce = Vector3.zero;
        Vector3 nextPosition = transform.position + transform.forward * boundaryPadding;
        
        if (!IsPointInPond(nextPosition))
        {
            avoidanceForce = (centerPoint - transform.position).normalized;
            // Add some random variation to avoid getting stuck
            avoidanceForce += Random.insideUnitSphere * 0.3f;
        }
        
        return avoidanceForce.normalized;
    }

    bool IsPointInPond(Vector3 point)
    {
        int intersections = 0;
        for (int i = 0; i < boundaryPoints.Count; i++)
        {
            Vector3 vert1 = boundaryPoints[i];
            Vector3 vert2 = boundaryPoints[(i + 1) % boundaryPoints.Count];
            
            if (IsIntersecting(point, point + Vector3.right * 1000f, vert1, vert2))
            {
                intersections++;
            }
        }
        
        return intersections % 2 == 1;
    }

    void SetNewRandomTarget()
    {
        for (int i = 0; i < 10; i++) // Try up to 10 times to find valid point
        {
            Vector3 randomDirection = Random.insideUnitSphere;
            randomDirection.y = 0; // Keep movement in XZ plane
            randomDirection.Normalize();
            
            Vector3 potentialTarget = transform.position + randomDirection * Random.Range(wanderRadius * 0.2f, wanderRadius);
            potentialTarget.y = transform.position.y; // Maintain Y position
            
            if (IsPointInPond(potentialTarget))
            {
                targetPoint = potentialTarget;
                return;
            }
        }
        
        // If no valid point found, move towards center
        targetPoint = Vector3.Lerp(transform.position, centerPoint, 0.5f);
    }

    private bool IsIntersecting(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
    {
        // Line segment intersection check (keeping y constant)
        float denominator = (point2.x - point1.x) * (point4.z - point3.z) - 
                          (point2.z - point1.z) * (point4.x - point3.x);
        
        if (denominator == 0) return false;
        
        float ua = ((point4.x - point3.x) * (point1.z - point3.z) - 
                   (point4.z - point3.z) * (point1.x - point3.x)) / denominator;
        float ub = ((point2.x - point1.x) * (point1.z - point3.z) - 
                   (point2.z - point1.z) * (point1.x - point3.x)) / denominator;
        
        return ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1;
    }
}