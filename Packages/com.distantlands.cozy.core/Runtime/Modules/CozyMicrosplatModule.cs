//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula


using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DistantLands.Cozy
{

    [ExecuteAlways]
    public class CozyMicrosplatModule : CozyModule
    {

        public enum UpdateFrequency { everyFrame, onAwake, viaScripting }
        public UpdateFrequency updateFrequency;

        [Header("Wetness")]
        public bool updateWetness = true;
        [Range(0f, 1f)]
        public float minWetness = 0f;
        [Range(0f, 1f)]
        public float maxWetness = 1f;
        [Header("Rain Ripples")]
        public bool updateRainRipples = true;
        [Header("Puddle Settings")]
        public bool updatePuddles = true;
        [Header("Stream Settings")]
        public bool updateStreams = true;
        [Header("Snow Settings")]
        public bool updateSnow = true;
        [Header("Wind Settings")]
        public bool updateWindStrength = true;

        private static readonly int GlobalSnowLevel = Shader.PropertyToID("_Global_SnowLevel");
        private static readonly int GlobalWetnessParams = Shader.PropertyToID("_Global_WetnessParams");
        private static readonly int GlobalPuddleParams = Shader.PropertyToID("_Global_PuddleParams");
        private static readonly int GlobalRainIntensity = Shader.PropertyToID("_Global_RainIntensity");
        private static readonly int GlobalStreamMax = Shader.PropertyToID("_Global_StreamMax");
        private static readonly int GlobalWindParticulateStrength = Shader.PropertyToID("_Global_WindParticulateStrength");
        private static readonly int GlobalSnowParticulateStrength = Shader.PropertyToID("_Global_SnowParticulateStrength");


        // Start is called before the first frame update
        public override void InitializeModule()
        {
            base.InitializeModule();
            
            if (GetComponent<CozyWeather>())
            {

                GetComponent<CozyWeather>().InitializeModule(typeof(CozyMicrosplatModule));
                DestroyImmediate(this);
                Debug.LogWarning("Add modules in the settings tab in COZY 2!");
                return;

            }
            if (updateFrequency == UpdateFrequency.onAwake)
            {
                UpdateShaderProperties();
            }
        }

        // Update is called once per frame
        private void Update()
        {
            
            if (weatherSphere.freezeUpdateInEditMode && !Application.isPlaying)
                return;
                
            if (updateFrequency == UpdateFrequency.everyFrame)
            {
                UpdateShaderProperties();
            }
        }

        public void UpdateShaderProperties()
        {

            if (weatherSphere.climateModule)
            {
                if (updateSnow)
                {
                    Shader.SetGlobalFloat(GlobalSnowLevel, weatherSphere.climateModule.snowAmount);
                }
                if (updateWetness)
                {
                    float currentWetness = Mathf.Clamp(weatherSphere.climateModule.wetness, minWetness, maxWetness);
                    Shader.SetGlobalVector(GlobalWetnessParams, new Vector2(minWetness, currentWetness));
                }
                if (updatePuddles)
                {
                    Shader.SetGlobalFloat(GlobalPuddleParams, weatherSphere.climateModule.wetness);
                }
                if (updateRainRipples)
                {
                    Shader.SetGlobalFloat(GlobalRainIntensity, weatherSphere.climateModule.wetness);
                }
                if (updateStreams)
                {
                    Shader.SetGlobalFloat(GlobalStreamMax, weatherSphere.climateModule.wetness);
                }
            }

            // if (weatherSphere.vfxModule)
            // {
            //     if (updateWindStrength)
            //     {
            //         Shader.SetGlobalFloat(GlobalWindParticulateStrength, weatherSphere.vfxModule.windManager.windSpeed);
            //     }
            //     if (updateSnow && updateWindStrength)
            //     {
            //         Shader.SetGlobalFloat(GlobalSnowParticulateStrength, weatherSphere.vfxModule.windManager.windSpeed);
            //     }
            // }
        }
    }

#if UNITY_EDITOR    
    [CustomEditor(typeof(CozyMicrosplatModule))]
    [CanEditMultipleObjects]
    public class E_MicrosplatIntegration : E_CozyModule
    {
        SerializedProperty updateFrequency;
        CozyMicrosplatModule module;


        void OnEnable()
        {

        }

        public override GUIContent GetGUIContent()
        {

            return new GUIContent("    MicroSplat", (Texture)Resources.Load("Integration"), "Links the COZY system with MicroSplat by Jason Booth.");

        }
        
        public override void OpenDocumentationURL()
        {
            Application.OpenURL("https://distant-lands.gitbook.io/cozy-stylized-weather-documentation/how-it-works/modules/microsplat-module");
        }

        public override void DisplayInCozyWindow()
        {
            EditorGUI.indentLevel = 0;
            serializedObject.Update();

            if (module == null)
                module = (CozyMicrosplatModule)target;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("updateWetness"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minWetness"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxWetness"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("updateRainRipples"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("updatePuddles"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("updateStreams"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("updateSnow"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("updateWindStrength"));
            EditorGUILayout.Space(20);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("updateFrequency"));
            serializedObject.ApplyModifiedProperties();


        }
    }
#endif
}