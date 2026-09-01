using UnityEngine;

namespace RNBridge
{
    // Proves Unity can be told where an RN UI element sits on screen: RN
    // measures its own view (measureInWindow) and sends a screen-space point
    // here; this positions a marker on Unity's own Canvas at that point.
    public class UILayoutBridge : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private RectTransform marker;

        [System.Serializable]
        private struct ScreenPoint
        {
            public float x;
            public float y;
        }

        // Called from RN: postMessage('UILayoutBridge', 'SetMarkerScreenPoint', '{"x":123,"y":456}')
        // x/y are Android screen pixels, top-left origin, Y-down (RN's measureInWindow * PixelRatio).
        public void SetMarkerScreenPoint(string json)
        {
            var point = JsonUtility.FromJson<ScreenPoint>(json);
            var screenPoint = new Vector2(point.x, Screen.height - point.y);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out var localPoint))
            {
                marker.anchoredPosition = localPoint;
                marker.gameObject.SetActive(true);
            }
        }
    }
}
