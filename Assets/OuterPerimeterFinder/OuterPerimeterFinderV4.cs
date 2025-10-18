using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class OuterPerimeterFinderV4 : MonoBehaviour
{
    [Header("Boundary GameObjects")]
    public List<GameObject> outerBoundaryGameObjects = new List<GameObject>();
    public List<GameObject> innerBoundaryGameObjects1 = new List<GameObject>();
    public List<GameObject> innerBoundaryGameObjects2 = new List<GameObject>();
    
    [Header("Generated Points (Read Only)")]
    public List<Vector3> outerBoundaryPoints = new List<Vector3>();
    public List<Vector3> antiBoundaryPoints1 = new List<Vector3>();
    public List<Vector3> antiBoundaryPoints2 = new List<Vector3>();
    
    [Header("Target Boids Manager")]
    public BoidsManagerV12 boidsManager;
    
    [Header("Visualization Settings")]
    public float arrowLength = 1f;
    public float arrowHeadLength = 0.25f;
    public float arrowHeadAngle = 20f;
    public Color outerBoundaryColor = Color.yellow;
    public Color innerBoundary1Color = Color.red;
    public Color innerBoundary2Color = Color.magenta;
    public Color pointColor = Color.white;
    public float pointSize = 0.1f;
    
    void Awake()
    {
        GenerateAllBoundaryPoints();
        ApplyToBoidsManager();
    }
    
    void GenerateAllBoundaryPoints()
    {
        // Generate outer boundary points (main containment boundary)
        outerBoundaryPoints = GenerateBoundaryLoop(outerBoundaryGameObjects);
        
        // Generate first inner boundary points (anti-boundary/obstacles)
        antiBoundaryPoints1 = GenerateBoundaryLoop(innerBoundaryGameObjects1);
        
        // Generate second inner boundary points (anti-boundary/obstacles)
        antiBoundaryPoints2 = GenerateBoundaryLoop(innerBoundaryGameObjects2);
        
        Debug.Log($"Generated {outerBoundaryPoints.Count} outer boundary points, " +
                  $"{antiBoundaryPoints1.Count} inner boundary 1 points, " +
                  $"{antiBoundaryPoints2.Count} inner boundary 2 points");
    }
    
    List<Vector3> GenerateBoundaryLoop(List<GameObject> gameObjects)
    {
        List<Vector3> points = new List<Vector3>();
        
        if (gameObjects == null || gameObjects.Count == 0)
            return points;
        
        // Extract positions from GameObjects
        List<Vector3> positions = new List<Vector3>();
        foreach (var obj in gameObjects)
        {
            if (obj != null)
            {
                positions.Add(obj.transform.position);
            }
        }
        
        if (positions.Count < 3)
            return points;
        
        // Create set of available points
        HashSet<Vector3> availablePoints = new HashSet<Vector3>(positions);
        
        // Find the loop using nearest neighbor approach (like V3)
        if (availablePoints.Count > 0)
        {
            Vector3 startPoint = availablePoints.First();
            FindLoop(startPoint, availablePoints, points);
        }
        
        return points;
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
    
    public void ApplyToBoidsManager()
    {
        if (boidsManager == null)
        {
            Debug.LogWarning("No BoidsManager assigned to apply points to");
            return;
        }
        
        // Apply outer boundary points as main boundary
        boidsManager.polygonPoints = outerBoundaryPoints.ToArray();
        
        // Create separate arrays for each inner boundary set
        List<Vector3[]> antiBoundaryArrays = new List<Vector3[]>();
        
        if (antiBoundaryPoints1.Count > 0)
        {
            antiBoundaryArrays.Add(antiBoundaryPoints1.ToArray());
        }
        
        if (antiBoundaryPoints2.Count > 0)
        {
            antiBoundaryArrays.Add(antiBoundaryPoints2.ToArray());
        }
        
        // Apply as array of arrays
        boidsManager.antiPolygonPoints = antiBoundaryArrays.ToArray();
        
        // If the boids manager is already started, reinitialize boundary points
        if (Application.isPlaying)
        {
            boidsManager.startScript();
        }
        
        int totalAntiPoints = antiBoundaryPoints1.Count + antiBoundaryPoints2.Count;
        Debug.Log($"Applied {outerBoundaryPoints.Count} boundary points and " +
                  $"{totalAntiPoints} anti-boundary points ({antiBoundaryArrays.Count} separate arrays) to BoidsManager");
    }
    
    // Public methods for runtime updates
    public void RegenerateAndApply()
    {
        GenerateAllBoundaryPoints();
        ApplyToBoidsManager();
    }
    
    public Vector3[] GetOuterBoundaryPoints()
    {
        return outerBoundaryPoints.ToArray();
    }
    
    public Vector3[] GetCombinedAntiBoundaryPoints()
    {
        List<Vector3> combined = new List<Vector3>();
        combined.AddRange(antiBoundaryPoints1);
        combined.AddRange(antiBoundaryPoints2);
        return antiBoundaryPoints1.ToArray();
    }
    
    public Vector3[] GetAntiBoundaryPoints1()
    {
        return antiBoundaryPoints1.ToArray();
    }
    
    public Vector3[] GetAntiBoundaryPoints2()
    {
        return antiBoundaryPoints2.ToArray();
    }
    
    private void OnDrawGizmos()
    {
        // Draw outer boundary loop
        if (outerBoundaryPoints != null && outerBoundaryPoints.Count > 1)
        {
            DrawLoop(outerBoundaryPoints, outerBoundaryColor, "Outer");
        }

        // Draw first inner boundary loop
        if (antiBoundaryPoints1 != null && antiBoundaryPoints1.Count > 1)
        {
            DrawLoop(antiBoundaryPoints1, innerBoundary1Color, "Inner1");
        }
        
        // Draw second inner boundary loop
        if (antiBoundaryPoints2 != null && antiBoundaryPoints2.Count > 1)
        {
            DrawLoop(antiBoundaryPoints2, innerBoundary2Color, "Inner2");
        }
        
        // Draw GameObjects as wireframe cubes
        DrawGameObjectMarkers();
    }
    
    private void DrawLoop(List<Vector3> points, Color color, string label)
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
            UnityEditor.Handles.Label(points[i] + Vector3.up * 0.3f, $"{label}[{i}]");
        }
        #endif
    }
    
    private void DrawGameObjectMarkers()
    {
        // Draw outer boundary GameObjects
        Gizmos.color = outerBoundaryColor * 0.7f;
        foreach (var obj in outerBoundaryGameObjects)
        {
            if (obj != null)
            {
                Gizmos.DrawWireCube(obj.transform.position, Vector3.one * 0.5f);
            }
        }
        
        // Draw inner boundary GameObjects set 1
        Gizmos.color = innerBoundary1Color * 0.7f;
        foreach (var obj in innerBoundaryGameObjects1)
        {
            if (obj != null)
            {
                Gizmos.DrawWireCube(obj.transform.position, Vector3.one * 0.3f);
            }
        }
        
        // Draw inner boundary GameObjects set 2
        Gizmos.color = innerBoundary2Color * 0.7f;
        foreach (var obj in innerBoundaryGameObjects2)
        {
            if (obj != null)
            {
                Gizmos.DrawWireCube(obj.transform.position, Vector3.one * 0.3f);
            }
        }
    }
}