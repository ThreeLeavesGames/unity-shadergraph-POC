//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif



namespace DistantLands.Cozy.Data
{

    [System.Serializable]
    [CreateAssetMenu(menuName = "Distant Lands/Cozy/Material Manager Profile", order = 361)]
    public class MaterialManagerProfile : ScriptableObject
    {


        public Texture snowTexture;
        public float snowNoiseSize = 10;
        public Color snowColor = Color.white;
        public float puddleScale = 2;



        [System.Serializable]
        public class ModulatedValue
        {

            public bool expanded = false;
            public enum ModulationSource { dayPercent, yearPercent, precipitation, temperature, snowAmount, rainAmount }
            public enum ModulationTarget { terrainLayerColor, terrainLayerTint, materialColor, materialValue, globalColor, globalValue }
            [Tooltip("The source that will modulate the target.")]
            public ModulationSource modulationSource;
            [Tooltip("The target type that will be modulated.")]
            public ModulationTarget modulationTarget;
            [Tooltip("The gradient that will pass a color to the modulation target based on the modulation source.")]
            public Gradient mappedGradient;
            [Tooltip("The curve that will pass a float value to the modulation target based on the modulation source.")]
            public AnimationCurve mappedCurve;

            [Tooltip("The terrain layer that this profile impacts.")]
            public TerrainLayer targetLayer;
            [Tooltip("The material that this profile impacts.")]
            public Material targetMaterial;

            public string targetVariableName;


        }

        [ModulatedProperty]
        public ModulatedValue[] modulatedValues;

    }


#if UNITY_EDITOR
    [CustomEditor(typeof(MaterialManagerProfile))]
    [CanEditMultipleObjects]
    public class E_MaterialProfile : Editor
    {

        SerializedProperty terrainLayers;
        SerializedProperty seasonalMaterials;
        SerializedProperty seasonalValueMaterials;
        SerializedProperty snowTexture;
        SerializedProperty snowNoiseSize;
        SerializedProperty snowColor;
        SerializedProperty puddleScale;
        MaterialManagerProfile prof;
        SerializedProperty modulatedValues;

        public static bool modulatedValuesOpen;
        public static bool globalOpen;

        void OnEnable()
        {

            snowTexture = serializedObject.FindProperty("snowTexture");
            snowNoiseSize = serializedObject.FindProperty("snowNoiseSize");
            snowColor = serializedObject.FindProperty("snowColor");
            puddleScale = serializedObject.FindProperty("puddleScale");
            modulatedValues = serializedObject.FindProperty("modulatedValues");
            prof = (MaterialManagerProfile)target;

        }


        public override void OnInspectorGUI()
        {

            serializedObject.Update();
            modulatedValuesOpen = EditorGUILayout.BeginFoldoutHeaderGroup(modulatedValuesOpen, "    Modulated Values", EditorUtilities.FoldoutStyle);
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (modulatedValuesOpen)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < modulatedValues.arraySize; i++)
                {
                    EditorGUILayout.PropertyField(modulatedValues.GetArrayElementAtIndex(i));
                    if (modulatedValues.GetArrayElementAtIndex(i).FindPropertyRelative("expanded").boolValue)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Add New"))
                        {
                            modulatedValues.InsertArrayElementAtIndex(i + 1);
                        }
                        if (GUILayout.Button("Remove"))
                            modulatedValues.DeleteArrayElementAtIndex(i);

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space();
                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUI.indentLevel--;

                if (GUILayout.Button("Add New"))
                {
                    modulatedValues.InsertArrayElementAtIndex(modulatedValues.arraySize);
                }
            }



            globalOpen = EditorGUILayout.BeginFoldoutHeaderGroup(globalOpen, "    Global Values", EditorUtilities.FoldoutStyle);
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (globalOpen)
            {
                EditorGUILayout.Space();

                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Global Snow Settings", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(snowTexture);
                EditorGUILayout.PropertyField(snowNoiseSize);
                EditorGUILayout.PropertyField(snowColor);
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Global Rain Settings", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(puddleScale);
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }
            serializedObject.ApplyModifiedProperties();

        }


        public void DisplayInCozyWindow()
        {

            OnInspectorGUI();

        }

    }
#endif
}