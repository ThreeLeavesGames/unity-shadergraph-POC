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
            script.IncreaseBoidsByRandomPoints(0,500,20);
        }
        if(GUILayout.Button("Decrease boids"))
        {
            script.DecreaseBoidsByRandomPoints(1,500,20);
        }
    }
}
#endif
