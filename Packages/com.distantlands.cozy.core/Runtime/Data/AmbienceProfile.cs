//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace DistantLands.Cozy.Data
{

    [System.Serializable]
    [CreateAssetMenu(menuName = "Distant Lands/Cozy/Ambience Profile", order = 361)]
    public class AmbienceProfile : ScriptableObject
    {

        [Tooltip("Specifies the minimum length for this ambience profile.")]
        [MeridiemTimeAttriute]
        public float minTime = new MeridiemTime(0, 30);
        [Tooltip("Specifies the maximum length for this ambience profile.")]
        [MeridiemTimeAttriute]
        public float maxTime = new MeridiemTime(2, 30);
        [Tooltip("Multiplier for the computational chance that this ambience profile will play; 0 being never, and 2 being twice as likely as the average.")]
        [Range(0, 2)]
        public float likelihood = 1;
        [HideTitle(2)]
        public WeatherProfile[] dontPlayDuring;
        [ChanceEffector]
        public List<ChanceEffector> chances;


        [FX]
        public FXProfile[] FX;
        [UnityEngine.Range(0, 1)]
        public float FXVolume = 1;
        public bool useVFX;

        public float GetChance (CozyWeather weather)
        {

            float i = likelihood;

            foreach (ChanceEffector j in chances)
            {
                i *= j.GetChance(weather);
            }

            return i > 0 ? i : 0;

        }
        public void SetWeight(float weightVal)
        {
            foreach (FXProfile fx in FX)
                fx?.PlayEffect(weightVal);

        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AmbienceProfile))]
    [CanEditMultipleObjects]
    public class E_AmbienceProfile : Editor
    {

        SerializedProperty dontPlayDuring;
        SerializedProperty chances;
        SerializedProperty minTime;
        SerializedProperty maxTime;
        SerializedProperty particleFX;
        SerializedProperty soundFX;
        SerializedProperty likelihood;
        Vector2 scrollPos;
        AmbienceProfile prof;



        void OnEnable()
        {

            prof = (AmbienceProfile)target;

            dontPlayDuring = serializedObject.FindProperty("dontPlayDuring");
            chances = serializedObject.FindProperty("chances");
            particleFX = serializedObject.FindProperty("particleFX");
            soundFX = serializedObject.FindProperty("soundFX");
            likelihood = serializedObject.FindProperty("likelihood");
            minTime = serializedObject.FindProperty("minTime");
            maxTime = serializedObject.FindProperty("maxTime");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Undo.RecordObject(prof, prof.name + " Profile Changes");

            EditorGUILayout.LabelField("Forecasting Behaviours", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            Rect position = EditorGUILayout.GetControlRect();
            float startPos = position.width / 2.75f;
            var titleRect = new Rect(position.x, position.y, 70, position.height);
            EditorGUI.PrefixLabel(titleRect, new GUIContent("Ambience Length"));
            float min = minTime.floatValue;
            float max = maxTime.floatValue;
            EditorGUILayout.PropertyField(minTime);
            EditorGUILayout.PropertyField(maxTime);

            if (min > max)
                minTime.floatValue = max;

            EditorGUILayout.PropertyField(likelihood);

            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(dontPlayDuring, true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("chances"), new GUIContent("Chance Effectors"), true);
            EditorGUILayout.Space(10);
            EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("FX"));
            serializedObject.ApplyModifiedProperties();

        }
    }
#endif
}