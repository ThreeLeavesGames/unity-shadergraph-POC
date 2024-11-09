using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class OuterPerimeterFinderV2 : MonoBehaviour
{
    public List<GameObject> boundaryPointsGameObjects = new List<GameObject>();
    public List<Vector3> boundaryPoints = new List<Vector3>();
    public List<Vector3> outerPerimeterPoints;
    
    // Gizmo settings
    public float arrowLength = 1f;
    public float arrowHeadLength = 0.25f;
    public float arrowHeadAngle = 20f;
    public Color lineColor = Color.green;
    public Color pointColor = Color.red;
    public float pointSize = 0.1f;
    
    void Start()
    {
        InitializeBoundary();
        outerPerimeterPoints = GetOuterPerimeter();
    }
    
    void InitializeBoundary()
    {
        boundaryPoints.Clear();
        for (int i = 0; i < boundaryPointsGameObjects.Count; i++)
        {
            boundaryPoints.Add(boundaryPointsGameObjects[i].transform.position);
        }
    }

    public static List<Vector3> FindOuterPerimeterPoints(List<Vector3> allPoints)
    {
        List<Vector3> hull = new List<Vector3>();
        HashSet<Vector3> unusedPoints = new HashSet<Vector3>(allPoints);
        
        if (allPoints.Count < 3) return allPoints;
        
        // Start with leftmost point
        Vector3 currentPoint = allPoints.OrderBy(p => p.x).First();
        hull.Add(currentPoint);
        unusedPoints.Remove(currentPoint);
        
        while (unusedPoints.Count > 0)
        {
            Vector3 nearestPoint = Vector3.zero;
            float nearestDistance = float.MaxValue;
            
            foreach (Vector3 candidatePoint in unusedPoints)
            {
                float distance = Vector3.Distance(currentPoint, candidatePoint);
                if (distance < nearestDistance)
                {
                    // Check if this connection would cross any existing lines
                    bool crosses = false;
                    for (int i = 1; i < hull.Count; i++)
                    {
                        if (LinesIntersect(currentPoint, candidatePoint, hull[i-1], hull[i]))
                        {
                            crosses = true;
                            break;
                        }
                    }
                    
                    if (!crosses)
                    {
                        nearestDistance = distance;
                        nearestPoint = candidatePoint;
                    }
                }
            }
            
            // If we found a valid next point
            if (nearestDistance != float.MaxValue)
            {
                hull.Add(nearestPoint);
                unusedPoints.Remove(nearestPoint);
                currentPoint = nearestPoint;
            }
            else
            {
                // If we can't find a valid next point, break to prevent infinite loop
                break;
            }
        }
        
        // Close the loop by connecting back to the first point
        if (hull.Count > 2)
        {
            hull.Add(hull[0]);
        }
        
        return hull;
    }
    
    private static bool LinesIntersect(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        Vector2 a = new Vector2(p1.x, p1.z);
        Vector2 b = new Vector2(p2.x, p2.z);
        Vector2 c = new Vector2(p3.x, p3.z);
        Vector2 d = new Vector2(p4.x, p4.z);
        
        float denominator = ((b.x - a.x) * (d.y - c.y)) - ((b.y - a.y) * (d.x - c.x));
        if (denominator == 0)
            return false;
            
        float numerator1 = ((a.y - c.y) * (d.x - c.x)) - ((a.x - c.x) * (d.y - c.y));
        float numerator2 = ((a.y - c.y) * (b.x - a.x)) - ((a.x - c.x) * (b.y - a.y));
        
        if (denominator == 0) return numerator1 == 0 && numerator2 == 0;
        
        float r = numerator1 / denominator;
        float s = numerator2 / denominator;
        
        return (r > 0 && r < 1) && (s > 0 && s < 1);
    }

    public List<Vector3> GetOuterPerimeter()
    {
        var result = FindOuterPerimeterPoints(boundaryPoints);
        Debug.Log($"Found {result.Count} boundary points");
        for (int i = 0; i < result.Count; i++)
        {
            Debug.Log($"Point {i}: {result[i]}");
        }
        return result;
    }

    private void OnDrawGizmos()
    {
        // Debug - draw all boundary points
        Gizmos.color = Color.yellow;
        foreach (var point in boundaryPoints)
        {
            Gizmos.DrawWireSphere(point, pointSize * 0.5f);
        }

        // If we don't have the outer perimeter points yet, return
        if (outerPerimeterPoints == null || outerPerimeterPoints.Count == 0)
        {
            return;
        }

        // Draw the boundary lines and points
        for (int i = 0; i < outerPerimeterPoints.Count - 1; i++)  // Changed to Count - 1 since we're adding closing point
        {
            // Draw point
            Gizmos.color = pointColor;
            Gizmos.DrawSphere(outerPerimeterPoints[i], pointSize);

            // Draw line to next point
            Gizmos.color = lineColor;
            Vector3 currentPoint = outerPerimeterPoints[i];
            Vector3 nextPoint = outerPerimeterPoints[i + 1];
            
            // Draw the main line
            Gizmos.DrawLine(currentPoint, nextPoint);
            
            // Draw arrow head
            Vector3 direction = (nextPoint - currentPoint).normalized;
            Vector3 arrowPos = Vector3.Lerp(currentPoint, nextPoint, 0.8f);
            
            Vector3 right = Quaternion.Euler(0, arrowHeadAngle, 0) * -direction;
            Vector3 left = Quaternion.Euler(0, -arrowHeadAngle, 0) * -direction;
            
            Gizmos.DrawRay(arrowPos, right * arrowHeadLength);
            Gizmos.DrawRay(arrowPos, left * arrowHeadLength);
        }

        // Debug - draw index numbers
        #if UNITY_EDITOR
        for (int i = 0; i < outerPerimeterPoints.Count; i++)
        {
            UnityEditor.Handles.Label(outerPerimeterPoints[i] + Vector3.up * 0.2f, i.ToString());
        }
        #endif
    }
}