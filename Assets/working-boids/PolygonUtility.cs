using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public static class PolygonUtility
{
    public static bool IsPointInPolygon(float2 point,Vector3[] boundaryPoints)
    {
        int winding = 0;
        int vertexCount = boundaryPoints.Length;

        for (int i = 0; i < vertexCount; i++)
        {
            int nextIndex = (i + 1) % vertexCount;
            float2 vertex1 = new float2(boundaryPoints[i].x, boundaryPoints[i].z);
            float2 vertex2 = new float2(boundaryPoints[nextIndex].x, boundaryPoints[nextIndex].z);

            if (vertex1.y <= point.y)
            {
                if (vertex2.y > point.y && IsLeft(vertex1, vertex2, point) > 0)
                    winding++;
            }
            else
            {
                if (vertex2.y <= point.y && IsLeft(vertex1, vertex2, point) < 0)
                    winding--;
            }
        }

        return winding != 0;
    }

    private static float IsLeft(float2 p0, float2 p1, float2 point)
    {
        return (p1.x - p0.x) * (point.y - p0.y) - (point.x - p0.x) * (p1.y - p0.y);
    }
}