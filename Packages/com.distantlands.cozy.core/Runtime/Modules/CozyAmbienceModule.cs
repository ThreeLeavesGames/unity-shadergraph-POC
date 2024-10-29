//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using System.Collections;
using DistantLands.Cozy.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DistantLands.Cozy
{

    [ExecuteAlways]
    public class CozyAmbienceModule : CozyModule, ICozyBiomeModule
    {

        public AmbienceProfile[] ambienceProfiles = new AmbienceProfile[0];

        [System.Serializable]
        public class WeightedAmbience
        {
            public AmbienceProfile ambienceProfile;
            [Range(0, 1)]
            public float weight;
            public bool transitioning;
            public IEnumerator Transition(float value, float time)
            {
                transitioning = true;
                float t = 0;
                float start = weight;

                while (t < time)
                {

                    float div = (t / time);
                    yield return new WaitForEndOfFrame();

                    weight = Mathf.Lerp(start, value, div);
                    t += Time.deltaTime;

                }

                weight = value;
                ambienceProfile.SetWeight(weight);
                transitioning = false;

            }
        }

        public List<WeightedAmbience> weightedAmbience = new List<WeightedAmbience>();

        public AmbienceProfile currentAmbienceProfile;
        public AmbienceProfile ambienceChangeCheck;
        public float timeToChangeProfiles = 7;
        private float m_AmbienceTimer;
        public float weight;

        public List<CozyAmbienceModule> biomes = new List<CozyAmbienceModule>();
        public CozyAmbienceModule parentModule;
        public bool isBiomeModule { get; set; }

        void Start()
        {
            if (!enabled)
                return;

            if (isBiomeModule)
                return;

            foreach (AmbienceProfile profile in ambienceProfiles)
            {
                foreach (FXProfile fx in profile.FX)
                    fx?.InitializeEffect(weatherSphere);
            }

            if (Application.isPlaying)
            {
                SetNextAmbience();
                weightedAmbience = new List<WeightedAmbience>() { new WeightedAmbience() { weight = 1, ambienceProfile = currentAmbienceProfile } };
            }

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
            biomes = FindObjectsOfType<CozyAmbienceModule>().Where(x => x != this).ToList();
        }

        public void FindAllAmbiences()
        {

            List<AmbienceProfile> profiles = new List<AmbienceProfile>();

            foreach (AmbienceProfile i in EditorUtilities.GetAllInstances<AmbienceProfile>())
                if (i.name != "Default Ambience")
                    profiles.Add(i);

            foreach (AmbienceProfile profile in ambienceProfiles)
            {
                foreach (FXProfile fx in profile.FX)
                    fx?.InitializeEffect(weatherSphere);
            }

            ambienceProfiles = profiles.ToArray();

        }

        // Update is called once per frame
        public override void UpdateWeatherWeights()
        {
            if (Application.isPlaying)
            {
                if (ambienceChangeCheck != currentAmbienceProfile)
                {
                    SetAmbience(currentAmbienceProfile);
                }

                if (weatherSphere.timeModule)
                    m_AmbienceTimer -= Time.deltaTime * weatherSphere.timeModule.modifiedTimeSpeed;
                else
                    m_AmbienceTimer -= Time.deltaTime * 0.005f;

                if (m_AmbienceTimer <= 0)
                {
                    SetNextAmbience();
                }

                foreach (WeightedAmbience i in weightedAmbience)
                {
                    i.weight = i.weight * weight;
                }

                weightedAmbience.RemoveAll(x => x.weight == 0 && x.transitioning == false);

            }

            ComputeBiomeWeights();
            // ManageBiomeWeights();
        }

        public override void UpdateFXWeights()
        {
            foreach (WeightedAmbience weather in weightedAmbience)
            {
                weather.ambienceProfile.SetWeight(weather.weight);
            }
        }
        public void UpdateBiomeModule()
        {

            currentAmbienceProfile.SetWeight(weight);

        }

        public void SetNextAmbience()
        {

            currentAmbienceProfile = WeightedRandom(ambienceProfiles.ToArray());

        }

        public void SetAmbience(AmbienceProfile profile)
        {

            currentAmbienceProfile = profile;
            ambienceChangeCheck = currentAmbienceProfile;

            if (weightedAmbience.Find(x => x.ambienceProfile == profile) == null)
                weightedAmbience.Add(new WeightedAmbience() { weight = 0, ambienceProfile = profile, transitioning = true });

            foreach (WeightedAmbience j in weightedAmbience)
            {

                if (j.ambienceProfile == profile)
                    StartCoroutine(j.Transition(1, timeToChangeProfiles));
                else
                    StartCoroutine(j.Transition(0, timeToChangeProfiles));

            }

            m_AmbienceTimer += Random.Range(currentAmbienceProfile.minTime, currentAmbienceProfile.maxTime);
        }

        public void SetAmbience(AmbienceProfile profile, float timeToChange)
        {

            currentAmbienceProfile = profile;
            ambienceChangeCheck = currentAmbienceProfile;

            if (weightedAmbience.Find(x => x.ambienceProfile == profile) == null)
                weightedAmbience.Add(new WeightedAmbience() { weight = 0, ambienceProfile = profile, transitioning = true });

            foreach (WeightedAmbience j in weightedAmbience)
            {

                if (j.ambienceProfile == profile)
                    StartCoroutine(j.Transition(1, timeToChange));
                else
                    StartCoroutine(j.Transition(0, timeToChange));

            }

            m_AmbienceTimer += Random.Range(currentAmbienceProfile.minTime, currentAmbienceProfile.maxTime);
        }

        public void SkipTime(float timeToSkip) => m_AmbienceTimer -= timeToSkip;

        public AmbienceProfile WeightedRandom(AmbienceProfile[] profiles)
        {
            AmbienceProfile i = null;
            List<float> floats = new List<float>();
            float totalChance = 0;

            foreach (AmbienceProfile k in profiles)
            {
                float chance;

                if (weatherSphere.weatherModule)
                    if (k.dontPlayDuring.Contains(weatherSphere.weatherModule.ecosystem.currentWeather))
                        chance = 0;
                    else
                        chance = k.GetChance(weatherSphere);
                else
                    chance = k.GetChance(weatherSphere);

                floats.Add(chance);
                totalChance += chance;
            }

            if (totalChance == 0)
            {
                i = (AmbienceProfile)Resources.Load("Default Ambience");
                Debug.LogWarning("Could not find a suitable ambience given the current selected profiles and chance effectors. Defaulting to an empty ambience.");
                return i;
            }

            float selection = Random.Range(0, totalChance);

            int m = 0;
            float l = 0;

            while (l <= selection)
            {
                if (selection >= l && selection < l + floats[m])
                {
                    i = profiles[m];
                    break;
                }
                l += floats[m];
                m++;

            }

            if (!i)
            {
                i = profiles[0];
            }

            return i;
        }

        public float GetTimeTillNextAmbience() => m_AmbienceTimer;

        public void AddBiome()
        {
            weatherSphere = CozyWeather.instance;

            if (parentModule == null)
                parentModule = weatherSphere.GetModule<CozyAmbienceModule>();

            weatherSphere.GetModule<CozyAmbienceModule>().biomes = FindObjectsOfType<CozyAmbienceModule>().Where(x => x != weatherSphere.GetModule<CozyAmbienceModule>()).ToList();

        }

        public bool CheckBiome()
        {

            if (!CozyWeather.instance.GetModule<CozyAmbienceModule>())
            {
                Debug.LogError("The ambience biome module requires the ambience module to be enabled on your weather sphere. Please add the ambience module before setting up your biome.");
                return false;
            }
            return true;
        }

        public void RemoveBiome()
        {
            parentModule.biomes.Remove(this);
        }

        public void ComputeBiomeWeights()
        {
            float totalSystemWeight = 0;
            biomes.RemoveAll(x => x == null);

            foreach (CozyAmbienceModule biome in biomes)
            {
                if (biome != this)
                {
                    totalSystemWeight += biome.system.targetWeight;
                }
            }

            weight = Mathf.Clamp01(1 - (totalSystemWeight));
            totalSystemWeight += weight;

            foreach (CozyAmbienceModule biome in biomes)
            {
                if (biome.system != this)
                    biome.weight = biome.system.targetWeight / (totalSystemWeight == 0 ? 1 : totalSystemWeight);
            }
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CozyAmbienceModule))]
    [CanEditMultipleObjects]
    public class E_AmbienceManager : E_CozyModule, E_BiomeModule, IControlPanel
    {


        protected static bool profileSettings;
        protected static bool currentInfo;
        CozyAmbienceModule ambienceManager;


        public override GUIContent GetGUIContent()
        {

            return new GUIContent("    Ambience", (Texture)Resources.Load("Ambience Profile"), "Controls a secondary weather system that runs parallel to the main system allowing for ambient noises and FX.");

        }

        void OnEnable()
        {

            if (target)
                ambienceManager = (CozyAmbienceModule)target;

        }
        public override void GetReportsInformation()
        {

            EditorGUILayout.LabelField(GetGUIContent(), EditorStyles.toolbar);

            EditorGUILayout.HelpBox("Current Ambiences", MessageType.None);
            foreach (CozyAmbienceModule.WeightedAmbience w in ambienceManager.weightedAmbience)
                EditorGUILayout.HelpBox($"{w.ambienceProfile.name} - Weight: {w.weight}", MessageType.None);

        }

        public void GetControlPanel()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("currentAmbienceProfile"));
        }

        public override void OpenDocumentationURL()
        {
            Application.OpenURL("https://distant-lands.gitbook.io/cozy-stylized-weather-documentation/how-it-works/modules/ambience-module");
        }

        public override void DisplayInCozyWindow()
        {
            serializedObject.Update();
            EditorGUI.indentLevel = 0;

            if (ambienceManager == null)
                if (target)
                    ambienceManager = (CozyAmbienceModule)target;
                else
                    return;

            profileSettings = EditorGUILayout.BeginFoldoutHeaderGroup(profileSettings, "    Forecast Settings", EditorUtilities.FoldoutStyle);
            EditorGUI.EndFoldoutHeaderGroup();
            if (profileSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ambienceProfiles"));
                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.Space();
                if (GUILayout.Button("Add all ambience profiles"))
                    ambienceManager.FindAllAmbiences();
                EditorGUI.indentLevel--;

            }


            currentInfo = EditorGUILayout.BeginFoldoutHeaderGroup(currentInfo, "    Current Information", EditorUtilities.FoldoutStyle);
            EditorGUI.EndFoldoutHeaderGroup();
            if (currentInfo)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("currentAmbienceProfile"));
                if (Application.isPlaying)
                    EditorGUILayout.HelpBox(ambienceManager.currentAmbienceProfile.name + " will be playing for the next " + ((MeridiemTime)ambienceManager.GetTimeTillNextAmbience()).ToString() + " of in-game time.", MessageType.None, true);

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        public void DrawBiomeReports()
        {
            EditorGUILayout.HelpBox($"Current ambience is {ambienceManager.currentAmbienceProfile.name}", MessageType.None, true);
        }

        public void DrawInlineBiomeUI()
        {
            if (target == null) return;
            serializedObject.Update();

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("currentAmbienceProfile"));
            EditorGUI.indentLevel--;


            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}