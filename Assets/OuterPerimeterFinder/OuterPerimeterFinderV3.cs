using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class OuterPerimeterFinderV3 : MonoBehaviour
{
     public List<GameObject> boundaryPointsGameObjects = new List<GameObject>();
    public List<Vector3> boundaryPoints = new List<Vector3>();
    public List<Vector3> loop1 = new List<Vector3>();
    public List<Vector3> loop2 = new List<Vector3>();
    
    // Visualization settings
    public float arrowLength = 1f;
    public float arrowHeadLength = 0.25f;
    public float arrowHeadAngle = 20f;
    public Color loop1Color = Color.green;
    public Color loop2Color = Color.blue;
    public Color pointColor = Color.red;
    public float pointSize = 0.1f;
    
    void Awake()
    {
        InitializeBoundary();
        FindTwoLoops();
    }
    
    void InitializeBoundary()
    {
        boundaryPoints.Clear();
        foreach (var obj in boundaryPointsGameObjects)
        {
            if (obj != null)
            {
                boundaryPoints.Add(obj.transform.position);
            }
        }
    }

    void FindTwoLoops()
    {
        // Clear previous loops
        loop1.Clear();
        loop2.Clear();

        // Create set of available points
        HashSet<Vector3> availablePoints = new HashSet<Vector3>(boundaryPoints);
        
        // Find first loop
        if (availablePoints.Count > 0)
        {
            Vector3 startPoint = availablePoints.First();
            FindLoop(startPoint, availablePoints, loop1);
            
            // Remove used points except the start point (as it was added twice to close the loop)
            if (loop1.Count > 0)
            {
                for (int i = 0; i < loop1.Count - 1; i++)
                {
                    availablePoints.Remove(loop1[i]);
                }
            }
        }

        // Find second loop from remaining points
        if (availablePoints.Count >= 3)
        {
            Vector3 startPoint = availablePoints.First();
            FindLoop(startPoint, availablePoints, loop2);
        }
    }

    void FindLoop(Vector3 startPoint, HashSet<Vector3> availablePoints, List<Vector3> loop)
    {
        Vector3 currentPoint = startPoint;
        loop.Add(currentPoint);
        
        while (true)
        {
            Vector3? nearestPoint = FindNearestPoint(currentPoint, startPoint, availablePoints, loop);
            if (!nearestPoint.HasValue) break;

            currentPoint = nearestPoint.Value;
            loop.Add(currentPoint);

            // If we're back to start point, close the loop
            if (currentPoint == startPoint)
            {
                break;
            }
        }
    }

    Vector3? FindNearestPoint(Vector3 currentPoint, Vector3 startPoint, HashSet<Vector3> availablePoints, List<Vector3> currentLoop)
    {
        float nearestDistance = float.MaxValue;
        Vector3? nearestPoint = null;

        foreach (Vector3 candidate in availablePoints)
        {
            // Skip if point is already in the loop (except start point for closing)
            if (currentLoop.Contains(candidate) && candidate != startPoint) continue;
            
            // If we haven't completed a minimum loop (3 points), don't allow returning to start
            if (candidate == startPoint && currentLoop.Count < 3) continue;

            float distance = Vector3.Distance(currentPoint, candidate);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPoint = candidate;
            }
        }

        return nearestPoint;
    }

    private void OnDrawGizmos()
    {
        if (boundaryPoints == null) return;

        // Draw all boundary points
        Gizmos.color = Color.yellow;
        foreach (var point in boundaryPoints)
        {
            Gizmos.DrawWireSphere(point, pointSize * 0.5f);
        }

        // Draw first loop
        if (loop1 != null && loop1.Count > 1)
        {
            DrawLoop(loop1, loop1Color);
        }

        // Draw second loop
        if (loop2 != null && loop2.Count > 1)
        {
            DrawLoop(loop2, loop2Color);
        }
    }

    private void DrawLoop(List<Vector3> points, Color color)
    {
        for (int i = 0; i < points.Count - 1; i++)
        {
            // Draw point
            Gizmos.color = pointColor;
            Gizmos.DrawSphere(points[i], pointSize);

            // Draw line with arrow
            Gizmos.color = color;
            Vector3 start = points[i];
            Vector3 end = points[i + 1];
            
            // Draw main line
            Gizmos.DrawLine(start, end);
            
            // Draw arrow
            Vector3 dir = (end - start).normalized;
            Vector3 arrowPos = Vector3.Lerp(start, end, 0.8f);
            
            Vector3 right = Quaternion.Euler(0, arrowHeadAngle, 0) * -dir;
            Vector3 left = Quaternion.Euler(0, -arrowHeadAngle, 0) * -dir;
            
            Gizmos.DrawRay(arrowPos, right * arrowHeadLength);
            Gizmos.DrawRay(arrowPos, left * arrowHeadLength);
        }

        #if UNITY_EDITOR
        for (int i = 0; i < points.Count; i++)
        {
            UnityEditor.Handles.Label(points[i] + Vector3.up * 0.2f, i.ToString());
        }
        #endif
    }
}