//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using UnityEngine;
#if THE_VEGETATION_ENGINE
using TheVegetationEngine;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DistantLands.Cozy
{

    [ExecuteAlways]
    public class CozyTVEModule : CozyModule
    {

        public enum UpdateFrequency { everyFrame, onAwake, viaScripting }
        public UpdateFrequency updateFrequency;

#if THE_VEGETATION_ENGINE

        public TVEGlobalControl globalControl;
        public TVEGlobalMotion globalMotion;

#endif

        // Start is called before the first frame update
        void Awake()
        {

            InitializeModule();

#if THE_VEGETATION_ENGINE
            if (updateFrequency == UpdateFrequency.onAwake)
                UpdateTVE();
#endif

        }

#if THE_VEGETATION_ENGINE
        public override void InitializeModule()
        {

            if (!enabled)
                return;

            base.InitializeModule();

            if (!weatherSphere)
            {
                enabled = false;
                return;
            }

            if (!globalControl)
                globalControl = FindObjectOfType<TVEGlobalControl>();

            if (!globalControl)
            {
                enabled = false;
                return;
            }

            if (!globalMotion)
                globalMotion = FindObjectOfType<TVEGlobalMotion>();

            if (!globalMotion)
            {
                enabled = false;
                return;
            }


            globalControl.mainLight = weatherSphere.sunLight;


        }


        // Update is called once per frame
        void Update()
        {

            if (weatherSphere.freezeUpdateInEditMode && !Application.isPlaying)
                return;
                
            if (updateFrequency == UpdateFrequency.everyFrame)
                UpdateTVE();



        }

        public void UpdateTVE()
        {

            if (weatherSphere.climateModule)
            {
                globalControl.globalWetness = weatherSphere.climateModule.wetness;
                globalControl.globalOverlay = weatherSphere.climateModule.snowAmount;
            }

            globalControl.seasonControl = weatherSphere.timeModule.yearPercentage * 4;

            if (weatherSphere.windModule)
            {
                globalMotion.windPower = weatherSphere.windModule.windAmount;
                globalMotion.transform.LookAt(globalMotion.transform.position + weatherSphere.windModule.WindDirection, Vector3.up);
            }
        }

#endif
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(CozyTVEModule))]
    [CanEditMultipleObjects]
    public class E_TVEIntegration : E_CozyModule
    {
        SerializedProperty updateFrequency;
        CozyTVEModule module;


        void OnEnable()
        {

        }

        public override GUIContent GetGUIContent()
        {

            return new GUIContent("    TVE", (Texture)Resources.Load("Boxophobic"), "Links the COZY system with the vegetation engine by BOXOPHOBIC.");

        }

        public override void OpenDocumentationURL()
        {
            Application.OpenURL("https://distant-lands.gitbook.io/cozy-stylized-weather-documentation/how-it-works/modules/the-vegetation-engine-tve-module");
        }

        public override void DisplayInCozyWindow()
        {
            EditorGUI.indentLevel = 0;
            serializedObject.Update();

            if (module == null)
                module = (CozyTVEModule)target;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("updateFrequency"));
            serializedObject.ApplyModifiedProperties();

#if THE_VEGETATION_ENGINE
            if (!module.globalControl || !module.globalMotion)
            {
                EditorGUILayout.Space(20);
                EditorGUILayout.HelpBox("Make sure that you have active TVE Global Motion and TVE Global Control objects in your scene!", MessageType.Warning);

            }
#else
            EditorGUILayout.Space(20);
            EditorGUILayout.HelpBox("The Vegetation Engine is not currently in this project! Please make sure that it has been properly downloaded before using this module.", MessageType.Warning);

#endif
        }
    }
#endif
}