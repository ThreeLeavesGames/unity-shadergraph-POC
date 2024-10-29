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
    public class CozyWeatherModule : CozyModule, ICozyEcosystem, ICozyBiomeModule
    {
        public float cumulus;
        public float cirrus;
        public float altocumulus;
        public float cirrostratus;
        public float chemtrails;
        public float nimbus;
        public float nimbusHeight;
        public float nimbusVariation;
        public float borderHeight;
        public float borderEffect;
        public float borderVariation;
        public float fogDensity;
        public float weight = 1;

        public float filterSaturation;
        public float filterValue;
        public Color filterColor = Color.white;
        public Color sunFilter = Color.white;
        public Color cloudFilter = Color.white;

        public CozyEcosystem ecosystem;
        public CozyWeatherModule parentModule;
        public List<CozyWeatherModule> biomes = new List<CozyWeatherModule>();

        public CozyEcosystem Ecosystem { get => ecosystem; set => ecosystem = value; }
        public CozySystem LocalSystem { get => system; set => system = value; }

        public bool isBiomeModule { get; set; }

        [WeatherRelation]
        public List<WeatherRelation> currentWeatherProfiles = new List<WeatherRelation>();
        public FilterFX defaultFilter;
        public CloudFX defaultClouds;

        public void Awake()
        {
            if (!enabled)
                return;

            RunChecks();


            ecosystem.SetupEcosystem();
            ResetFilter();
            ResetClouds();

        }

        public override void InitializeModule()
        {
            isBiomeModule = GetComponent<CozyBiome>();

            if (isBiomeModule)
            {
                AddBiome();
                return;
            }
            base.InitializeModule();
            weatherSphere.weatherModule = this;
            biomes = FindObjectsOfType<CozyWeatherModule>().Where(x => x != this).ToList();

        }

        private void RunChecks()
        {

            weatherSphere ??= CozyWeather.instance;

            defaultClouds = (CloudFX)Resources.Load("Default Profiles/Default Clouds");
            defaultFilter = (FilterFX)Resources.Load("Default Profiles/Default Filter");

            ecosystem ??= new CozyEcosystem();

            if (system == weatherSphere)
                weatherSphere.weatherModule = this;
            ecosystem.weatherSphere = weatherSphere;
            ecosystem.system = system;
        }

        public void Start()
        {
            foreach (WeatherProfile profile in ecosystem.forecastProfile.profilesToForecast)
            {
                foreach (FXProfile fx in profile.FX)
                    fx?.InitializeEffect(weatherSphere);
            }
        }

        public override void UpdateWeatherWeights()
        {
            ecosystem.UpdateEcosystem();
            ManageGlobalEcosystem();
            UpdateWeatherByWeight();

        }
        public override void UpdateFXWeights()
        {
            foreach (WeatherRelation weather in currentWeatherProfiles)
            {
                weather.profile.SetWeatherWeight(weather.weight);
            }
        }

        public override void FrameReset()
        {
            ResetClouds();
            ResetFilter();
        }

        /// <summary>
        /// Calculates the weather color filter based on the currently active Filter FX profiles.
        /// </summary> 
        void ResetFilter()
        {
            if (ecosystem == null)
                return;


            filterSaturation = defaultFilter.filterSaturation;
            filterValue = defaultFilter.filterValue;
            filterColor = defaultFilter.filterColor;
            sunFilter = defaultFilter.sunFilter;
            cloudFilter = defaultFilter.cloudFilter;

        }

        /// <summary>
        /// Calculates the clouds based on the currently active Cloud FX profiles.
        /// </summary> 
        void ResetClouds()
        {
            if (ecosystem == null)
                return;

            cumulus = defaultClouds.cumulusCoverage;
            cirrus = defaultClouds.cirrusCoverage;
            altocumulus = defaultClouds.altocumulusCoverage;
            cirrostratus = defaultClouds.cirrostratusCoverage;
            chemtrails = defaultClouds.chemtrailCoverage;
            nimbus = defaultClouds.nimbusCoverage;
            nimbusHeight = defaultClouds.nimbusHeightEffect;
            nimbusVariation = defaultClouds.nimbusVariation;
            borderHeight = defaultClouds.borderHeight;
            borderEffect = defaultClouds.borderEffect;
            borderVariation = defaultClouds.borderVariation;
            fogDensity = defaultClouds.fogDensity;

        }

        /// <summary>
        /// Send all weather information to the main COZY Weather Sphere for rendering.
        ///</summary>
        public override void PropogateVariables()
        {
            weatherSphere.cumulus = cumulus;
            weatherSphere.cirrus = cirrus;
            weatherSphere.altocumulus = altocumulus;
            weatherSphere.cirrostratus = cirrostratus;
            weatherSphere.chemtrails = chemtrails;
            weatherSphere.nimbus = nimbus;
            weatherSphere.nimbusHeightEffect = nimbusHeight;
            weatherSphere.nimbusVariation = nimbusVariation;
            weatherSphere.borderHeight = borderHeight;
            weatherSphere.borderEffect = borderEffect;
            weatherSphere.borderVariation = borderVariation;
            weatherSphere.fogDensity = fogDensity;

            weatherSphere.filterSaturation = filterSaturation;
            weatherSphere.filterValue = filterValue;
            weatherSphere.filterColor = filterColor;
            weatherSphere.sunFilter = sunFilter;
            weatherSphere.cloudFilter = cloudFilter;
        }

        void ManageGlobalEcosystem()
        {
            if (system == null) RunChecks();

            currentWeatherProfiles.Clear();

            if (weight > 0)
                foreach (WeatherRelation weatherRelation in ecosystem.weightedWeatherProfiles)
                {
                    if (weatherRelation.weight == 0)
                    {
                        weatherRelation.profile.SetWeatherWeight(0);
                        continue;
                    }

                    if (currentWeatherProfiles.Find(x => x.profile == weatherRelation.profile) != null)
                    {
                        currentWeatherProfiles.Find(x => x.profile == weatherRelation.profile).weight += weatherRelation.weight * weight;
                        continue;
                    }

                    WeatherRelation l = new WeatherRelation();
                    l.profile = weatherRelation.profile;
                    l.weight = weatherRelation.weight * weight;
                    currentWeatherProfiles.Add(l);

                }

            foreach (CozyWeatherModule biome in biomes)
            {
                if (biome == null) continue;

                CozyEcosystem localEcosystem = biome.Ecosystem;

                if (biome.weight > 0)
                {
                    foreach (WeatherRelation weatherRelation in localEcosystem.weightedWeatherProfiles)
                    {
                        if (weatherRelation.weight == 0)
                        {
                            weatherRelation.profile.SetWeatherWeight(0);
                            continue;
                        }

                        if (currentWeatherProfiles.Find(x => x.profile == weatherRelation.profile) != null)
                        {
                            currentWeatherProfiles.Find(x => x.profile == weatherRelation.profile).weight += weatherRelation.weight * biome.weight;
                            continue;
                        }

                        WeatherRelation l = new WeatherRelation();
                        l.profile = weatherRelation.profile;
                        l.weight = weatherRelation.weight * biome.weight;
                        currentWeatherProfiles.Add(l);

                    }
                }
                else
                {
                    foreach (WeatherRelation i in localEcosystem.weightedWeatherProfiles)
                    {
                        i.profile.SetWeatherWeight(0);
                    }
                }
            }
        }

        void UpdateWeatherByWeight()
        {
            ComputeBiomeWeights();
            float weatherWeightAcrossSystems = 0;

            foreach (WeatherRelation i in currentWeatherProfiles) weatherWeightAcrossSystems += i.weight;

            if (weatherWeightAcrossSystems == 0)
                weatherWeightAcrossSystems = 1;

            foreach (WeatherRelation i in currentWeatherProfiles)
            {
                i.weight /= weatherWeightAcrossSystems;
                // i.profile.SetWeatherWeight(i.weight);

            }
        }

        public void AddBiome()
        {
            if (parentModule == null)
                parentModule = weatherSphere.weatherModule;

            weatherSphere = CozyWeather.instance;
            weatherSphere.GetModule<CozyWeatherModule>().biomes = FindObjectsOfType<CozyWeatherModule>().Where(x => x != weatherSphere.GetModule<CozyWeatherModule>()).ToList();
        }

        public void RemoveBiome()
        {
            parentModule?.biomes.Remove(this);
        }

        public void UpdateBiomeModule()
        {
            ecosystem.UpdateEcosystem();
        }

        public bool CheckBiome()
        {
            if (!CozyWeather.instance.weatherModule)
            {
                Debug.LogError("The weather biome module requires the weather module to be enabled on your weather sphere. Please add the weather module before setting up your biome.");
                return false;
            }
            return true;
        }

        public void ComputeBiomeWeights()
        {

            float totalSystemWeight = 0;
            biomes.RemoveAll(x => x == null);

            foreach (CozyWeatherModule biome in biomes)
            {
                if (biome != this)
                {
                    totalSystemWeight += biome.system.targetWeight;
                }
            }

            weight = Mathf.Clamp01(1 - (totalSystemWeight));
            totalSystemWeight += weight;

            foreach (CozyWeatherModule biome in biomes)
            {
                if (biome.system != this)
                    biome.weight = biome.system.targetWeight / (totalSystemWeight == 0 ? 1 : totalSystemWeight);
            }
        }
    }



#if UNITY_EDITOR
    [CustomEditor(typeof(CozyWeatherModule))]
    [CanEditMultipleObjects]
    public class E_CozyWeatherModule : E_CozyModule, E_BiomeModule, IControlPanel
    {

        SerializedProperty ecosystem;
        CozyWeatherModule weatherModule;
        public static bool selectionWindowIsOpen;
        public static bool currentWeatherWindowIsOpen;
        public static bool forecastWindowIsOpen;

        public override GUIContent GetGUIContent()
        {

            //Place your module's GUI content here.
            return new GUIContent("    Weather", (Texture)Resources.Load("Weather Profile-01"), "Manage weather, forecast and playback options.");

        }

        void OnEnable()
        {
            ecosystem = serializedObject.FindProperty("ecosystem");
            weatherModule = (CozyWeatherModule)target;
        }

        public override void GetReportsInformation()
        {
            EditorGUILayout.LabelField(GetGUIContent(), EditorStyles.toolbar);
            EditorGUILayout.HelpBox("Current Weather", MessageType.None);

            foreach (WeatherRelation w in weatherModule.currentWeatherProfiles)
                EditorGUILayout.HelpBox($"{w.profile.name} - Weight: {w.weight}", MessageType.None);

            if (weatherModule.ecosystem.currentForecast.Count == 0)
            {

                EditorGUILayout.HelpBox("No forecast information yet!", MessageType.None);

            }
            else
            {
                EditorGUILayout.HelpBox("Currently it is " + weatherModule.ecosystem.currentWeather.name, MessageType.None);


                for (int i = 0; i < weatherModule.ecosystem.currentForecast.Count; i++)
                {
                    if (weatherModule.ecosystem.weatherSelectionMode == CozyEcosystem.EcosystemStyle.forecast)
                        EditorGUILayout.HelpBox($"Starting at {weatherModule.ecosystem.currentForecast[i].startTime.ToString()} the weather will change to {weatherModule.ecosystem.currentForecast[i].profile.name} for {((MeridiemTime)weatherModule.ecosystem.currentForecast[i].duration).ToString()} or unitl {weatherModule.ecosystem.currentForecast[i].endTime.ToString()}.", MessageType.None, true);

                    if (weatherModule.ecosystem.weatherSelectionMode == CozyEcosystem.EcosystemStyle.dailyForecast)
                        EditorGUILayout.HelpBox($"In {i + 1} day{(i == 0 ? "" : "s")} the weather will change to {weatherModule.ecosystem.currentForecast[i].profile.name}.", MessageType.None, true);

                    EditorGUILayout.Space(2);

                }
            }
        }
        public override void OpenDocumentationURL()
        {
            Application.OpenURL("https://distant-lands.gitbook.io/cozy-stylized-weather-documentation/how-it-works/modules/weather-module");
        }

        public override void DisplayInCozyWindow()
        {
            EditorGUI.indentLevel = 0;
            serializedObject.Update();

            EcosystemEditor.DrawEditor(ecosystem);

            serializedObject.ApplyModifiedProperties();

        }
        public void GetControlPanel()
        {
            if ((CozyEcosystem.EcosystemStyle)ecosystem.FindPropertyRelative("weatherSelectionMode").enumValueIndex != CozyEcosystem.EcosystemStyle.manual)
            {
                if ((CozyEcosystem.EcosystemStyle)ecosystem.FindPropertyRelative("weatherSelectionMode").enumValueIndex == CozyEcosystem.EcosystemStyle.automatic)
                    EditorGUILayout.PropertyField(ecosystem.FindPropertyRelative("currentWeather"), new GUIContent("Current Weather"));
                else
                    EditorGUILayout.PropertyField(ecosystem.FindPropertyRelative("currentWeather"), new GUIContent("Preview Weather"));
            }
            else
                EditorGUILayout.PropertyField(ecosystem.FindPropertyRelative("weightedWeatherProfiles"), new GUIContent("Weather Ratios"));
        }

        public void DrawBiomeReports()
        {
            if (weatherModule.ecosystem.currentForecast.Count == 0)
            {
                EditorGUILayout.HelpBox("No forecast information yet!", MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox("Currently it is " + weatherModule.ecosystem.currentWeather.name, MessageType.None);

                for (int i = 0; i < weatherModule.ecosystem.currentForecast.Count; i++)
                {
                    if (weatherModule.ecosystem.weatherSelectionMode == CozyEcosystem.EcosystemStyle.forecast)
                        EditorGUILayout.HelpBox($"Starting at {weatherModule.ecosystem.currentForecast[i].startTime.ToString()} the weather will change to {weatherModule.ecosystem.currentForecast[i].profile.name} for {((MeridiemTime)weatherModule.ecosystem.currentForecast[i].duration).ToString()} or unitl {weatherModule.ecosystem.currentForecast[i].endTime.ToString()}.", MessageType.None, true);

                    if (weatherModule.ecosystem.weatherSelectionMode == CozyEcosystem.EcosystemStyle.dailyForecast)
                        EditorGUILayout.HelpBox($"In {i + 1} day{(i == 0 ? "" : "s")} the weather will change to {weatherModule.ecosystem.currentForecast[i].profile.name}.", MessageType.None, true);

                    EditorGUILayout.Space(2);

                }
            }
        }

        public void DrawInlineBiomeUI()
        {
            if (!target) return;
            serializedObject.Update();

            EditorGUI.indentLevel++;
            EcosystemEditor.DrawEditor(ecosystem);
            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}