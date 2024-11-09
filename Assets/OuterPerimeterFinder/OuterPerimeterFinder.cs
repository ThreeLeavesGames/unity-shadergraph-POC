using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class OuterPerimeterFinder : MonoBehaviour
{
    public List<GameObject> boundaryPointsGameObjects = new List<GameObject>();
    
    public List<Vector3> boundaryPoints = new List<Vector3>();
    
    // Gizmo settings
    public float arrowLength = 1f;
    public float arrowHeadLength = 0.25f;
    public float arrowHeadAngle = 20f;
    private List<Vector3> outerPerimeterPoints;
    
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
        
        // Start with leftmost point
        Vector3 startPoint = allPoints.OrderBy(p => p.x).First();
        Vector3 currentPoint = startPoint;
        Vector3 nextPoint;
        
        do
        {
            hull.Add(currentPoint);
            nextPoint = allPoints[0];

            // Find point with smallest counterclockwise angle
            for (int i = 1; i < allPoints.Count; i++)
            {
                if (nextPoint == currentPoint || IsMoreCounterClockwise(currentPoint, allPoints[i], nextPoint))
                {
                    nextPoint = allPoints[i];
                }
            }

            currentPoint = nextPoint;
        }
        while (currentPoint != startPoint && hull.Count < allPoints.Count);

        return hull;
    }

    // Determines if point2 is more counterclockwise than candidatePoint relative to point1
    private static bool IsMoreCounterClockwise(Vector3 point1, Vector3 point2, Vector3 candidatePoint)
    {
        float crossProduct = (point2.x - point1.x) * (candidatePoint.z - point1.z) - 
                           (candidatePoint.x - point1.x) * (point2.z - point1.z);
        
        if (crossProduct == 0)
        {
            // If points are collinear, select the furthest point
            float d1 = (point2.x - point1.x) * (point2.x - point1.x) + 
                      (point2.z - point1.z) * (point2.z - point1.z);
            float d2 = (candidatePoint.x - point1.x) * (candidatePoint.x - point1.x) + 
                      (candidatePoint.z - point1.z) * (candidatePoint.z - point1.z);
            return d1 > d2;
        }
        
        return crossProduct > 0;
    }

    // Example usage in your circle class
    public List<Vector3> GetOuterPerimeter()
    {
        return FindOuterPerimeterPoints(boundaryPoints);
    }

    private void OnDrawGizmos()
    {
        if (outerPerimeterPoints == null || outerPerimeterPoints.Count == 0)
            return;

        Gizmos.color = Color.green;

        // Draw arrows between consecutive points
        for (int i = 0; i < outerPerimeterPoints.Count; i++)
        {
            Vector3 startPoint = outerPerimeterPoints[i];
            Vector3 endPoint = outerPerimeterPoints[(i + 1) % outerPerimeterPoints.Count];
            
            // Draw main line
            Gizmos.DrawLine(startPoint, endPoint);
            
            // Calculate arrow head
            Vector3 direction = (endPoint - startPoint).normalized;
            Vector3 right = Quaternion.Euler(0, arrowHeadAngle, 0) * -direction;
            Vector3 left = Quaternion.Euler(0, -arrowHeadAngle, 0) * -direction;
            
            // Draw arrow head
            Vector3 arrowTip = endPoint;
            Gizmos.DrawLine(arrowTip, arrowTip + right * arrowHeadLength);
            Gizmos.DrawLine(arrowTip, arrowTip + left * arrowHeadLength);
        }

        // Draw points
        foreach (Vector3 point in outerPerimeterPoints)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(point, 0.1f);
        }
    }
}