//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using UnityEngine;
using DistantLands.Cozy.Data;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DistantLands.Cozy
{
    [ExecuteAlways]
    public class CozyWindModule : CozyModule
    {

        public WindFX defaultWindProfile;
        public WindZone windZone;

        public float windSpeed;
        public float windChangeSpeed;
        public float windAmount;
        private Vector3 m_WindDirection;
        private float m_Seed;
        [Tooltip("Multiplies the total wind power by a coefficient.")]
        [Range(0, 2)]
        public float windMultiplier = 1;
        public bool useWindzone = true;
        public bool useShaderWind = true;
        private float m_WindTime;
        public List<WindFX> windFXes = new List<WindFX>();

        public Vector3 WindDirection
        {
            get { return m_WindDirection; }
            set { m_WindDirection = WindDirection; }

        }



        void Start()
        {
            weatherSphere.windModule = this;

            if (!defaultWindProfile)
                defaultWindProfile = (WindFX)Resources.Load("Default Wind");

            m_WindTime = 0;
            m_Seed = Random.value * 1000;


        }

        public override void CozyUpdateLoop()
        {

            float i = 360 * Mathf.PerlinNoise(m_Seed, Time.time * windChangeSpeed / 100000);
            m_WindDirection = new Vector3(Mathf.Sin(i), 0, Mathf.Cos(i));


            if (useWindzone)
            {

                if (windZone)
                {
                    windZone.transform.LookAt(transform.position + m_WindDirection, Vector3.up);
                    windZone.windMain = windAmount * windMultiplier;
                    windZone.windPulseFrequency = windSpeed;
                }
            }

            m_WindTime += Time.deltaTime * windSpeed;

            if (useShaderWind)
            {
                Shader.SetGlobalFloat("CZY_WindTime", m_WindTime);
                Shader.SetGlobalVector("CZY_WindDirection", m_WindDirection * windAmount * windMultiplier);
            }

        }

        public override void FrameReset()
        {
            if (defaultWindProfile)
            {
                windSpeed = defaultWindProfile.windSpeed;
                windAmount = defaultWindProfile.windAmount;
                windChangeSpeed = defaultWindProfile.windChangeSpeed;
            }
        }

        public override void DeinitializeModule()
        {
            base.DeinitializeModule();

            Shader.SetGlobalFloat("CZY_WindTime", 0);
            Shader.SetGlobalVector("CZY_WindDirection", Vector3.zero);

        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CozyWindModule))]
    [CanEditMultipleObjects]
    public class E_CozyWindModule : E_CozyModule
    {

        public static bool selection;
        public static bool settings;
        public static bool information;
        SerializedProperty profile;
        SerializedProperty windZone;
        SerializedProperty windMultiplier;
        SerializedProperty windSpeed;
        SerializedProperty windChangeSpeed;
        SerializedProperty windAmount;
        SerializedProperty useWindzone;
        SerializedProperty useShaderWind;

        public override GUIContent GetGUIContent()
        {

            //Place your module's GUI content here.
            return new GUIContent("    Wind", (Texture)Resources.Load("Wind Module"), "Control wind within the COZY system.");

        }

        void OnEnable()
        {
            profile = serializedObject.FindProperty("defaultWindProfile");
            windZone = serializedObject.FindProperty("windZone");
            windMultiplier = serializedObject.FindProperty("windMultiplier");
            windSpeed = serializedObject.FindProperty("windSpeed");
            windChangeSpeed = serializedObject.FindProperty("windChangeSpeed");
            windAmount = serializedObject.FindProperty("windAmount");
            useWindzone = serializedObject.FindProperty("useWindzone");
            useShaderWind = serializedObject.FindProperty("useShaderWind");
        }
        public override void GetReportsInformation()
        {

            EditorGUILayout.LabelField(GetGUIContent(), EditorStyles.toolbar);

            EditorGUILayout.HelpBox($"Current Wind Amount: {windAmount.floatValue} \n" +
            $"Current Wind Speed: {windSpeed.floatValue} \n" +
            $"Current Wind Change Speed: {windChangeSpeed.floatValue}", MessageType.None);

        }

        public override void OpenDocumentationURL()
        {
            Application.OpenURL("https://distant-lands.gitbook.io/cozy-stylized-weather-documentation/how-it-works/modules/wind-module");
        }

        public override void DisplayInCozyWindow()
        {
            EditorGUI.indentLevel = 0;
            serializedObject.Update();
            selection = EditorGUILayout.BeginFoldoutHeaderGroup(selection, new GUIContent("    Selection"), EditorUtilities.FoldoutStyle);
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (selection)
            {

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(profile);
                if (profile.objectReferenceValue)
                {
                    EditorGUI.indentLevel++;
                    CreateEditor(profile.objectReferenceValue).OnInspectorGUI();
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Space(20);
                EditorGUILayout.PropertyField(windZone);
                EditorGUI.indentLevel--;

            }

            settings = EditorGUILayout.BeginFoldoutHeaderGroup(settings, new GUIContent("    Global Settings"), EditorUtilities.FoldoutStyle);
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (settings)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(windMultiplier);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(useWindzone);
                EditorGUILayout.PropertyField(useShaderWind);

                EditorGUI.indentLevel--;

            }


            information = EditorGUILayout.BeginFoldoutHeaderGroup(information, new GUIContent("    Current Information"), EditorUtilities.FoldoutStyle);
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (information)
            {

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(windAmount);
                EditorGUILayout.PropertyField(windSpeed);
                EditorGUILayout.PropertyField(windChangeSpeed);
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;

            }

            serializedObject.ApplyModifiedProperties();

        }

    }
#endif
}