using UnityEngine;


#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(BoidsWorldManagerV1))]
public class BoidsWorldManagerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BoidsWorldManagerV1 script = (BoidsWorldManagerV1)target;

        if(GUILayout.Button("Increase boids"))
        {
            script.IncreaseBoidsByRandomPoints(0,100,5);
        }
        if(GUILayout.Button("Decrease boids"))
        {
            script.DecreaseBoidsByRandomPoints(0,100,5);
        }
    }
}
#endif
