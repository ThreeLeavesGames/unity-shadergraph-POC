//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using UnityEngine;
using DistantLands.Cozy.Data;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DistantLands.Cozy
{
    [ExecuteAlways]
    public class CozyClimateModule : CozyModule, ICozyBiomeModule
    {

        public ClimateProfile climateProfile;
        public CozyWeather.ControlMethod controlMethod = CozyWeather.ControlMethod.profile;


        [Tooltip("Adds an offset to the local temperature. Useful for adding biomes or climate change by location or elevation")]
        public float localTemperatureFilter;
        [Tooltip("Adds an offset to the local precipitation. Useful for adding biomes or climate change by location or elevation")]
        public float localPrecipitationFilter;
        internal float temperatureOffset;
        internal float precipitationOffset;
        public float currentTemperature;
        public float currentPrecipitation;

        [Range(0, 1)]
        public float snowAmount;
        [SerializeField]
        private float m_SnowMeltSpeed = 0.35f;
        [Range(0, 1)]
        public float wetness;
        [SerializeField]
        private float m_DryingSpeed = 0.5f;
        public float snowSpeed;
        public float rainSpeed;


        public List<CozyClimateModule> biomes = new List<CozyClimateModule>();
        public bool isBiomeModule { get; set; }
        public float weight = 1;

        public override void InitializeModule()
        {
            isBiomeModule = GetComponent<CozyBiome>();
            if (isBiomeModule)
                return;
            base.InitializeModule();
            weatherSphere.climateModule = this;
            biomes = FindObjectsOfType<CozyClimateModule>().Where(x => x != this).ToList();

        }

        public override void CozyUpdateLoop()
        {
            ComputeBiomeWeights();



            snowAmount += Time.deltaTime * snowSpeed;

            if (snowSpeed <= 0)
                if (currentTemperature > 32)
                    snowAmount -= Time.deltaTime * m_SnowMeltSpeed * 0.03f;

            wetness += (Time.deltaTime * rainSpeed) + (-1 * m_DryingSpeed * 0.001f);

            snowAmount = Mathf.Clamp01(snowAmount);
            wetness = Mathf.Clamp01(wetness);

            if (controlMethod == CozyWeather.ControlMethod.profile)
            {
                if (!climateProfile)
                    return;

                currentTemperature = climateProfile.GetTemperature(weatherSphere) + localTemperatureFilter + temperatureOffset;
                currentPrecipitation = Mathf.Clamp(climateProfile.GetHumidity(weatherSphere) + localPrecipitationFilter + precipitationOffset, 0, 100);
            }

            foreach (CozyClimateModule biome in biomes)
            {
                currentTemperature = Mathf.Lerp(currentTemperature, biome.currentTemperature, biome.weight);
                currentPrecipitation = Mathf.Lerp(currentPrecipitation, biome.currentPrecipitation, biome.weight);
            }

            Shader.SetGlobalFloat("CZY_SnowAmount", snowAmount);
            Shader.SetGlobalFloat("CZY_WetnessAmount", wetness);
        }

        public override void FrameReset()
        {

            temperatureOffset = 0;
            precipitationOffset = 0;

            snowSpeed = 0;
            rainSpeed = 0;

        }

        public float GetTemperature()
        {
            if (controlMethod == CozyWeather.ControlMethod.native)
                return currentTemperature;
            else
                return climateProfile.GetTemperature(weatherSphere) + localTemperatureFilter;

        }

        public float GetTemperature(float inTicks)
        {

            return climateProfile.GetTemperature(weatherSphere, inTicks) + localTemperatureFilter;

        }

        public float GetPrecipitation()
        {

            return climateProfile.GetHumidity(weatherSphere) + localPrecipitationFilter;

        }

        public float GetPrecipitation(float inTicks)
        {

            return climateProfile.GetHumidity(weatherSphere, inTicks) + localPrecipitationFilter;
        }

        public override void DeinitializeModule()
        {
            base.DeinitializeModule();

            Shader.SetGlobalFloat("CZY_WindTime", 0);
            Shader.SetGlobalVector("CZY_WindDirection", Vector3.zero);

        }

        public void AddBiome()
        {
            weatherSphere = CozyWeather.instance;
            weatherSphere.climateModule.biomes = FindObjectsOfType<CozyClimateModule>().Where(x => x != weatherSphere.climateModule).ToList();
        }

        public void RemoveBiome()
        {
            weatherSphere.climateModule.biomes.Remove(this);
        }

        public void UpdateBiomeModule()
        {
            if (controlMethod == CozyWeather.ControlMethod.profile)
            {
                if (!climateProfile)
                    return;

                currentTemperature = climateProfile.GetTemperature(weatherSphere) + localTemperatureFilter + temperatureOffset;
                currentPrecipitation = Mathf.Clamp(climateProfile.GetHumidity(weatherSphere) + localPrecipitationFilter + precipitationOffset, 0, 100);
            }
        }

        public bool CheckBiome()
        {

            if (!weatherSphere.climateModule)
            {
                Debug.LogError("The atmosphere biome module requires the atmosphere module to be enabled on your weather sphere. Please add the atmosphere module before setting up your biome.");
                return false;
            }
            return true;
        }

        public void ComputeBiomeWeights()
        {

            float totalSystemWeight = 0;
            biomes.RemoveAll(x => x == null);

            foreach (CozyClimateModule biome in biomes)
            {
                if (biome != this)
                {
                    totalSystemWeight += biome.system.targetWeight;
                }
            }

            weight = Mathf.Clamp01(1 - (totalSystemWeight));
            totalSystemWeight += weight;

            foreach (CozyClimateModule biome in biomes)
            {
                if (biome.system != this)
                    biome.weight = biome.system.targetWeight / (totalSystemWeight == 0 ? 1 : totalSystemWeight);
            }

        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CozyClimateModule))]
    [CanEditMultipleObjects]
    public class E_ClimateModule : E_CozyModule, E_BiomeModule, IControlPanel
    {

        public static bool selection;
        public static bool settings;
        public static bool information;
        SerializedProperty profile;
        SerializedProperty controlMethod;
        SerializedProperty currentTemperature;
        SerializedProperty currentPrecipitation;
        public CozyClimateModule climateModule;

        public override GUIContent GetGUIContent()
        {

            //Place your module's GUI content here.
            return new GUIContent("    Climate", (Texture)Resources.Load("Climate"), "Control temperature and humidity.");

        }

        void OnEnable()
        {
            profile = serializedObject.FindProperty("climateProfile");
            controlMethod = serializedObject.FindProperty("controlMethod");
            currentTemperature = serializedObject.FindProperty("currentTemperature");
            currentPrecipitation = serializedObject.FindProperty("currentPrecipitation");
            climateModule = (CozyClimateModule)target;
        }

        public override void GetReportsInformation()
        {

            EditorGUILayout.LabelField(GetGUIContent(), EditorStyles.toolbar);
            EditorGUILayout.HelpBox("Currently the global ecosystem is running at " + Mathf.Round(climateModule.currentTemperature) + "°F or " + Mathf.Round((climateModule.currentTemperature - 32) * 5f / 9f) + "°C with a precipitation chance of " + Mathf.Round(climateModule.currentPrecipitation) + "%.\n" +
                    "Temperatures will " + (climateModule.currentTemperature > climateModule.GetTemperature(1) ? "drop" : "rise") + " tomorrow, bringing the temperature to " + Mathf.Round(climateModule.GetTemperature(1)) + "°F", MessageType.None);
            EditorGUILayout.HelpBox($"Snow Amount: {climateModule.snowAmount}\nWetness: {climateModule.wetness}", MessageType.None);


        }

        public override void OpenDocumentationURL()
        {
            Application.OpenURL("https://distant-lands.gitbook.io/cozy-stylized-weather-documentation/how-it-works/modules/climate-module");
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
                EditorGUILayout.PropertyField(controlMethod);
                if (controlMethod.intValue == 1)
                    EditorGUILayout.PropertyField(profile);
                EditorGUI.indentLevel--;

            }

            if (controlMethod.intValue == 1)
            {
                settings = EditorGUILayout.BeginFoldoutHeaderGroup(settings, new GUIContent("    Global Settings"), EditorUtilities.FoldoutStyle);
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (settings)
                {
                    EditorGUI.indentLevel++;
                    if (profile.objectReferenceValue)
                        CreateEditor(profile.objectReferenceValue).OnInspectorGUI();
                    else
                        EditorGUILayout.HelpBox("Assign a profile to begin editing the climate!", MessageType.Error);


                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SnowMeltSpeed"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DryingSpeed"));

                    EditorGUI.indentLevel--;

                }
            }

            information = EditorGUILayout.BeginFoldoutHeaderGroup(information, new GUIContent("    Current Information"), EditorUtilities.FoldoutStyle);
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (information)
            {

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(currentTemperature);
                EditorGUILayout.PropertyField(currentPrecipitation);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("snowAmount"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("wetness"));
                EditorGUI.indentLevel--;

            }

            serializedObject.ApplyModifiedProperties();

        }
        public void GetControlPanel()
        {
            EditorGUILayout.PropertyField(currentTemperature);
            EditorGUILayout.PropertyField(currentPrecipitation);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("snowAmount"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wetness"));
        }

        public void DrawBiomeReports()
        {
            EditorGUILayout.PropertyField(currentTemperature);
            EditorGUILayout.PropertyField(currentPrecipitation);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("snowAmount"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wetness"));
        }

        public void DrawInlineBiomeUI()
        {
            if (!target) return;
            serializedObject.Update();
            EditorGUILayout.PropertyField(controlMethod);
            if (controlMethod.intValue == 1)
            {
                EditorGUILayout.PropertyField(profile);


                EditorGUI.indentLevel++;
                if (profile.objectReferenceValue)
                    CreateEditor(profile.objectReferenceValue).OnInspectorGUI();
                else
                    EditorGUILayout.HelpBox("Assign a profile to begin editing the climate!", MessageType.Error);

            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(currentTemperature);
            EditorGUILayout.PropertyField(currentPrecipitation);

            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();

        }
    }
#endif
}