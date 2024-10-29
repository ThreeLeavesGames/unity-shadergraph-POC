//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DistantLands.Cozy.Data
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Distant Lands/Cozy/Satellite Profile", order = 361)]
    public class SatelliteProfile : ScriptableObject
    {

        public GameObject satelliteReference;
        public Transform orbitRef;
        public Transform moonRef;
        public Light lightRef;
        public float size = 1;
        [Range(0, 1)]
        public float distance = 1;
        public bool autoScaleByDistance = true;
        public float orbitOffset;
        public Vector3 initialRotation;
        public float satelliteRotateSpeed;
        public bool linkToDay;
        public int rotationPeriod = 28;
        public int rotationPeriodOffset;
        public Vector3 satelliteRotateAxis;
        public float satelliteDirection;
        public float satelliteRotation;
        public float satellitePitch;
        public float declination;
        public int declinationPeriod;
        public int declinationPeriodOffset;
        public bool changedLastFrame;
        public bool open;

    }
#if UNITY_EDITOR
    [CustomEditor(typeof(SatelliteProfile))]
    [CanEditMultipleObjects]
    public class E_SatelliteProfile : Editor
    {


        public override void OnInspectorGUI()
        {

            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("satelliteReference"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("size"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("distance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoScaleByDistance"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("satelliteRotateAxis"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("linkToDay"));
            EditorGUI.indentLevel++;
            if (serializedObject.FindProperty("linkToDay").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationPeriod"));
                if (serializedObject.FindProperty("rotationPeriod").intValue <= 0)
                    serializedObject.FindProperty("rotationPeriod").intValue = 1;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationPeriodOffset"));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("declination"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("declinationPeriod"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("declinationPeriodOffset"));
                if (serializedObject.FindProperty("declinationPeriod").intValue <= 0)
                    serializedObject.FindProperty("declinationPeriod").intValue = 1;
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("satelliteRotateSpeed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("initialRotation"));
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("orbitOffset"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("satelliteDirection"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("satellitePitch"));

            if (serializedObject.hasModifiedProperties)
                serializedObject.FindProperty("changedLastFrame").boolValue = true;

            serializedObject.ApplyModifiedProperties();

        }

        public void NestedGUI()
        {

            serializedObject.Update();

            serializedObject.FindProperty("open").boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(serializedObject.FindProperty("open").boolValue, $"    {target.name}", EditorUtilities.FoldoutStyle);
            EditorGUILayout.EndFoldoutHeaderGroup();
            serializedObject.ApplyModifiedProperties();

            if (serializedObject.FindProperty("open").boolValue)
            {
                EditorGUI.indentLevel++;
                OnInspectorGUI();
                EditorGUI.indentLevel--;

            }

        }

    }

#endif
}