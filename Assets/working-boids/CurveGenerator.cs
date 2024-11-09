using UnityEngine;

public enum HillShape
{
    SineInOut,
    SineIn,
    SineOut,
    SineOutBack,
    CircInOut,
    CircIn,
    CircOut,
    ExpoInOut,
    ExpoIn,
    ExpoOut
}

public class CurveGenerator : MonoBehaviour
{
    [Header("Curve Settings")]
    public HillShape curveType = HillShape.CircIn;
    public float curveDepth = 1f;
    public int resolution = 20;
    public bool visualizeCurve = true;
    [Tooltip("1 for right bend, -1 for left bend")]
    public float curveDirection = 1f;  // Add this line

    [Header("Point References")]
    public GameObject startPoint;
    public GameObject endPoint;

    private float[] depthCurvePoints;
    private Vector3[] curvePositions;

    void Start()
    {
        GenerateCurvePoints();
    }

    public void GenerateCurvePoints()
    {
        if (startPoint == null || endPoint == null) return;

        depthCurvePoints = new float[resolution];
        curvePositions = new Vector3[resolution];

        Vector3 start = startPoint.transform.position;
        Vector3 end = endPoint.transform.position;

        Vector3 direction = end - start;
        // Multiply perpendicular by curveDirection to control bend direction
        Vector3 perpendicular = new Vector3(-direction.z, 0, direction.x).normalized * curveDirection;

        for (int i = 0; i < resolution; i++)
        {
            float t = (float)i / (resolution - 1);

            Vector3 basePosition = Vector3.Lerp(start, end, t);

            float curveScale = 1f - (4f * (t - 0.5f) * (t - 0.5f));
            float offset = EaseCurve(
                curveType,
                0f,
                curveDepth,
                i,
                resolution - 1
            ) * curveScale;

            curvePositions[i] = basePosition + (perpendicular * offset);
            depthCurvePoints[i] = offset;
        }
    }

    public static float EaseCurve(HillShape type, float b, float c, float t, float d)
    {
        if (type == HillShape.SineInOut)
        {
            return -c / 2f * (Mathf.Cos(3.14159274f * t / d) - 1f) + b;
        }
        if (type == HillShape.SineIn)
        {
            return -c * Mathf.Cos(t / d * 1.57079637f) + c + b;
        }
        if (type == HillShape.SineOut)
        {
            return c * Mathf.Sin(t / d * 1.57079637f) + b;
        }
        if (type == HillShape.SineOutBack)
        {
            if (t < d / 2f)
            {
                return c * Mathf.Sin(t * 2f / d * 1.57079637f) + b;
            }
            return c * Mathf.Sin((d - t) * 2f / d * 1.57079637f) + b;
        }
        else if (type == HillShape.CircInOut)
        {
            if ((t /= d / 2f) < 1f)
            {
                return -c / 2f * (Mathf.Sqrt(1f - t * t) - 1f) + b;
            }
            return c / 2f * (Mathf.Sqrt(1f - (t -= 2f) * t) + 1f) + b;
        }
        else
        {
            if (type == HillShape.CircIn)
            {
                return -c * (Mathf.Sqrt(1f - (t /= d) * t) - 1f) + b;
            }
            if (type == HillShape.CircOut)
            {
                return c * Mathf.Sqrt(1f - (t = t / d - 1f) * t) + b;
            }
            if (type == HillShape.ExpoInOut)
            {
                if (t == 0f) return b;
                if (t == d) return b + c;
                if ((t /= d / 2f) < 1f)
                {
                    return c / 2f * Mathf.Pow(2f, 10f * (t - 1f)) + b;
                }
                return c / 2f * (-Mathf.Pow(2f, -10f * (t -= 1f)) + 2f) + b;
            }
            else
            {
                if (type == HillShape.ExpoIn)
                {
                    return (t != 0f) ? (c * Mathf.Pow(2f, 10f * (t / d - 1f)) + b) : b;
                }
                if (type == HillShape.ExpoOut)
                {
                    return (t != d) ? (c * (-Mathf.Pow(2f, -10f * t / d) + 1f) + b) : (b + c);
                }
                return -c / 2f * (Mathf.Cos(3.14159274f * t / d) - 1f) + b;
            }
        }
    }

    void OnDrawGizmos()
    {
        // Draw reference points
        if (startPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(startPoint.transform.position, 0.3f);
        }
        if (endPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(endPoint.transform.position, 0.3f);
        }

        // Draw curve
        if (!visualizeCurve || curvePositions == null || curvePositions.Length < 2)
            return;

        Gizmos.color = Color.green;
        for (int i = 0; i < curvePositions.Length - 1; i++)
        {
            Gizmos.DrawLine(curvePositions[i], curvePositions[i + 1]);
        }

        Gizmos.color = Color.red;
        foreach (Vector3 pos in curvePositions)
        {
            Gizmos.DrawSphere(pos, 0.1f);
        }
    }

    // Optional: Update in real-time
    void Update()
    {
        if (startPoint != null && endPoint != null)
        {
            GenerateCurvePoints();
        }
    }

    public Vector3[] GetCurvePositions()
    {
        return curvePositions;
    }
}