//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using System.Collections.Generic;
using UnityEngine;
using DistantLands.Cozy.Data;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if COZY_URP || COZY_HDRP
using UnityEngine.Rendering;
#endif

namespace DistantLands.Cozy
{
    [ExecuteAlways]
    public class CozySatelliteModule : CozyModule
    {


        public SatelliteProfile[] satellites = new SatelliteProfile[0];
        [HideInInspector]
        public Transform satHolder = null;
        public bool hideInHierarchy = true;
        private Light moonLight;
        public int mainMoon;
        public bool useLight = true;
        public LightShadows moonlightShadows;
#if COZY_URP || COZY_HDRP
        public LensFlareComponentSRP moonLensFlare;
#endif


        public override void InitializeModule()
        {
            moonLight = weatherSphere.GetChild<Light>("Moon Light");
            moonLight.enabled = true;
            base.InitializeModule();
#if COZY_URP || COZY_HDRP
            if (weatherSphere.moonFlare.flare != null)
                if (moonLight.GetComponent<LensFlareComponentSRP>())
                    moonLensFlare = moonLight.GetComponent<LensFlareComponentSRP>();
                else
                    moonLensFlare = moonLight.gameObject.AddComponent<LensFlareComponentSRP>();
#endif
        }

        // Start is called before the first frame update
        void Awake()
        {
            UpdateSatellites();

        }

        // Update is called once per frame
        void Update()
        {

            if (weatherSphere.freezeUpdateInEditMode && !Application.isPlaying)
                return;

            if (satHolder == null)
            {
                UpdateSatellites();
            }
            if (moonLight == null)
            {
                moonLight = weatherSphere.GetChild<Light>("Moon Light");
            }

            if (satHolder.hideFlags == (HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild) && hideInHierarchy)
                UpdateSatellites();

            if (weatherSphere.cozyCamera)
                satHolder.position = weatherSphere.cozyCamera.transform.position;

            if (satellites != null)
                foreach (SatelliteProfile sat in satellites)
                {
                    if (!sat)
                        break;

                    if (sat.orbitRef == null)
                        UpdateSatellites();

                    if (sat.changedLastFrame == true)
                        UpdateSatellites();


                    if (sat.linkToDay && weatherSphere.timeModule)
                    {
                        float dec = sat.declination * Mathf.Sin(Mathf.PI * 2 * ((weatherSphere.modifiedDayPercentage + (float)((weatherSphere.timeModule.currentDay + sat.declinationPeriodOffset + weatherSphere.timeModule.GetDaysPerYear() * weatherSphere.timeModule.currentYear)) % sat.declinationPeriod) / sat.declinationPeriod));
                        sat.orbitRef.localEulerAngles = new Vector3(0, weatherSphere.sunDirection + sat.satelliteDirection, sat.satellitePitch + dec);
                        sat.satelliteRotation = (weatherSphere.modifiedDayPercentage + (float)((weatherSphere.timeModule.currentDay + sat.rotationPeriodOffset + weatherSphere.timeModule.GetDaysPerYear() * weatherSphere.timeModule.currentYear)) % sat.rotationPeriod) / sat.rotationPeriod * 360;
                        sat.orbitRef.GetChild(0).localEulerAngles = Vector3.right * ((360 * weatherSphere.modifiedDayPercentage) + sat.satelliteRotation + sat.orbitOffset);
                    }
                    else
                    {
                        sat.orbitRef.localEulerAngles = new Vector3(0, weatherSphere.sunDirection + sat.satelliteDirection, sat.satellitePitch);
                        sat.orbitRef.GetChild(0).localEulerAngles = Vector3.right * ((360 * weatherSphere.dayPercentage) + sat.orbitOffset);
                        sat.satelliteRotation += Time.deltaTime * sat.satelliteRotateSpeed;
                        sat.moonRef.localEulerAngles = sat.initialRotation + sat.satelliteRotateAxis.normalized * sat.satelliteRotation;
                    }
                }

            if (!moonLight)
                return;

            moonLight.enabled = weatherSphere.moonlightColor.grayscale > 0.05f && satellites.Length > 0 && useLight;

            moonLight.transform.forward = satellites[mainMoon].orbitRef.GetChild(0).forward;
            weatherSphere.moonDirection = -moonLight.transform.forward;
            Shader.SetGlobalVector("CZY_MoonDirection", -moonLight.transform.forward);

            if (mainMoon >= satellites.Length)
                mainMoon = satellites.Length - 1;

            float moonBrightness = Mathf.Clamp01(Mathf.Sin((weatherSphere.dayPercentage + 0.25f) * 2 * Mathf.PI) + 0.25f) * Mathf.Clamp01(4 * Vector3.Dot(moonLight.transform.forward, Vector3.down));
            moonLight.color = weatherSphere.moonlightColor * weatherSphere.sunFilter * moonBrightness;
#if COZY_HDRP
                moonLight.shadows = moonlightShadows;
                moonLight.shadows = LightShadows.None;
#else
            moonLight.shadows = moonlightShadows;
#endif
#if COZY_URP || COZY_HDRP
            if (moonLensFlare)
            {
                moonLensFlare.intensity = weatherSphere.moonFlare.flare ? moonBrightness : 0;
                moonLensFlare.lensFlareData = weatherSphere.moonFlare.flare;
                moonLensFlare.allowOffScreen = weatherSphere.moonFlare.allowOffscreen;
                moonLensFlare.radialScreenAttenuationCurve = weatherSphere.moonFlare.screenAttenuation;
                moonLensFlare.distanceAttenuationCurve = weatherSphere.moonFlare.screenAttenuation;
                moonLensFlare.scale = weatherSphere.moonFlare.scale;
                moonLensFlare.occlusionRadius = weatherSphere.moonFlare.occlusionRadius;
                moonLensFlare.useOcclusion = weatherSphere.moonFlare.useOcclusion;
            }
#endif


        }


        public void UpdateSatellites()
        {
            if (weatherSphere == null)
                weatherSphere = CozyWeather.instance;

            Transform oldHolder = null;


            if (satHolder)
            {
                oldHolder = satHolder;
            }

            satHolder = new GameObject("Cozy Satellites").transform;
            if (hideInHierarchy)
                satHolder.gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.HideInHierarchy;
            else
                satHolder.gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;



            if (satellites != null)
                foreach (SatelliteProfile i in satellites)
                {
                    InitializeSatellite(i);
                }

            if (oldHolder)
                DestroyImmediate(oldHolder.gameObject);

        }

        public void DestroySatellites()
        {

            if (satHolder)
                DestroyImmediate(satHolder.gameObject);

        }

        public void DestroySatellite(SatelliteProfile sat)
        {

            if (sat.orbitRef)
                DestroyImmediate(sat.orbitRef.gameObject);

        }

        public override void DeinitializeModule()
        {
            moonLight.enabled = false;
            DestroySatellites();
            Shader.SetGlobalVector("CZY_MoonDirection", Vector3.down);
        }

        public void InitializeSatellite(SatelliteProfile sat)
        {


            float dist = 0;

            if (weatherSphere.lockToCamera != CozyWeather.LockToCameraStyle.DontLockToCamera && weatherSphere.cozyCamera)
                dist = .92f * weatherSphere.cozyCamera.farClipPlane * sat.distance;
            else
                dist = .92f * 1000 * sat.distance * weatherSphere.transform.localScale.x;

            sat.orbitRef = new GameObject(sat.name).transform;
            sat.orbitRef.parent = satHolder;
            sat.orbitRef.transform.localPosition = Vector3.zero;
            var orbitArm = new GameObject("Orbit Arm");
            orbitArm.transform.parent = sat.orbitRef;
            orbitArm.transform.localPosition = Vector3.zero;
            orbitArm.transform.localEulerAngles = Vector3.zero;
            sat.moonRef = Instantiate(sat.satelliteReference, Vector3.forward * dist, Quaternion.identity, sat.orbitRef.GetChild(0)).transform;
            sat.moonRef.transform.localPosition = -Vector3.forward * dist;
            sat.moonRef.transform.localEulerAngles = sat.initialRotation;
            sat.moonRef.transform.localScale = sat.satelliteReference.transform.localScale * sat.size * (sat.autoScaleByDistance ? dist / 1000 : 1);
            sat.orbitRef.localEulerAngles = new Vector3(0, sat.satelliteDirection, sat.satellitePitch);
            sat.orbitRef.GetChild(0).localEulerAngles = Vector3.right * ((360 * weatherSphere.dayPercentage) + sat.orbitOffset);

            //             if (sat.useLight)
            //             {
            //                 var obj = new GameObject("Light");
            //                 obj.transform.parent = sat.orbitRef.GetChild(0);
            //                 sat.lightRef = obj.AddComponent<Light>();
            //                 sat.lightRef.transform.localEulerAngles = new Vector3(0, 0, 0);
            //                 sat.lightRef.transform.localPosition = -Vector3.forward * dist;
            //                 sat.lightRef.type = LightType.Directional;
            //                 sat.lightRef.shadows = sat.castShadows;
            // #if COZY_URP
            //                 if (sat.flare.flare)
            //                 {
            //                     sat.flareRef = obj.AddComponent<LensFlareComponentSRP>();
            //                     sat.flareRef.intensity = sat.flare.intensity;
            //                     sat.flareRef.lensFlareData = sat.flare.flare;
            //                     sat.flareRef.allowOffScreen = sat.flare.allowOffscreen;
            //                     sat.flareRef.radialScreenAttenuationCurve = sat.flare.screenAttenuation;
            //                     sat.flareRef.distanceAttenuationCurve = sat.flare.screenAttenuation;
            //                     sat.flareRef.scale = sat.flare.scale;
            //                     sat.flareRef.occlusionRadius = sat.flare.occlusionRadius;
            //                     sat.flareRef.useOcclusion = sat.flare.useOcclusion;
            //                 }
            // #else
            //                 if (sat.flare)
            //                     sat.lightRef.flare = sat.flare;
            // #endif

            //             }
            //             if (mainMoon)
            //             {
            //                 sat.orbitRef.GetChild(0).gameObject.AddComponent<CozySetMoonDirection>();
            //             }

            sat.changedLastFrame = false;
        }

        void Reset()
        {
            List<SatelliteProfile> profiles = new List<SatelliteProfile>
            {
                Resources.Load("Profiles/Satellites/Stylized Moon") as SatelliteProfile
            };
            satellites = profiles.ToArray();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CozySatelliteModule))]
    [CanEditMultipleObjects]
    public class E_SatelliteManager : E_CozyModule
    {

        CozySatelliteModule t;
        static bool manageSatellites;

        private void OnEnable()
        {
            t = (CozySatelliteModule)target;
        }

        public override GUIContent GetGUIContent()
        {

            return new GUIContent("    Satellites", (Texture)Resources.Load("CozyMoon"), "Manage satellites and moons within the COZY system.");

        }

        public override void OpenDocumentationURL()
        {
            Application.OpenURL("https://distant-lands.gitbook.io/cozy-stylized-weather-documentation/how-it-works/modules/satellite-module");
        }


        public override void DisplayInCozyWindow()
        {
            EditorGUI.indentLevel = 0;
            serializedObject.Update();
            manageSatellites = EditorGUILayout.BeginFoldoutHeaderGroup(manageSatellites, new GUIContent("    Manage Satellites"), EditorUtilities.FoldoutStyle);
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (manageSatellites)
            {
                List<string> moonNames = new List<string>();
                foreach (SatelliteProfile satelliteProfile in t.satellites)
                {
                    moonNames.Add(satelliteProfile.name);
                }

                EditorGUILayout.Space();
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("satellites"), true);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("hideInHierarchy"));
                t.mainMoon = Mathf.Clamp(EditorGUILayout.Popup("Main Moon", t.mainMoon, moonNames.ToArray()), 0, t.satellites.Length);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("useLight"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("moonlightShadows"));
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel++;
            if (t.satellites != null)
                foreach (SatelliteProfile i in t.satellites)
                {
                    if (i)
                        (CreateEditor(i) as E_SatelliteProfile).NestedGUI();
                }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            if (GUILayout.Button("Refresh Satellites"))
                ((CozySatelliteModule)target).UpdateSatellites();

            serializedObject.ApplyModifiedProperties();


        }

    }

#endif
}