//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DistantLands.Cozy
{
    public class SystemTimeModule : CozyTimeModule
    {

        [MeridiemTimeAttriute]
        [SerializeField]
        private float m_SystemTime = 0.5f;
        [SerializeField]
        private bool pauseTime;
        [Tooltip("How many times should the COZY day complete per real world day.")]
        public float timeMultiplier = 1;
        [Tooltip("How many times should the COZY year complete per real world year.")]
        public float dateMultiplier = 1;

        public enum TimeGatherMode { Local, UTC }

        public TimeGatherMode timeGatherMode;
        [Tooltip("Adds an offset to the gathered time in hours.")]
        public float hourOffset;

        internal override bool CheckIfModuleCanBeAdded(out string warning)
        {
            if (weatherSphere.moduleHolder.GetComponents<CozyTimeModule>().Length != 1)
            {
                warning = "Time Module";
                return false;
            }
            warning = "";
            return true;
        }

        public void Update()
        {
            if (weatherSphere.timeModule == null)
                weatherSphere.timeModule = this;

            if (!pauseTime)
            {
                if (timeGatherMode == TimeGatherMode.Local)
                {
                    m_SystemTime = (hourOffset * 3600000 + (float)DateTime.Now.TimeOfDay.TotalMilliseconds) * timeMultiplier / 86400000 % 1;
                    yearPercentage = (float)DateTime.Now.DayOfYear / 365 * dateMultiplier % 1;
                }
                else
                {
                    m_SystemTime = (hourOffset * 3600000 + (float)DateTime.UtcNow.TimeOfDay.TotalMilliseconds) * timeMultiplier / 86400000 % 1;
                    yearPercentage = (float)DateTime.UtcNow.DayOfYear / 365 * dateMultiplier % 1;
                }
                currentTime = m_SystemTime;
                modifiedDayPercentage = transit ? transit.ModifyDayPercentage(m_SystemTime) / 360 : m_SystemTime;



            }
        }

        public new float modifiedTimeSpeed
        {
            get
            {
                return timeMultiplier / 86400;
            }
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(SystemTimeModule))]
    [CanEditMultipleObjects]
    public class E_SystemTimeModule : E_CozyModule
    {

        SerializedProperty currentTimePercent;
        SerializedProperty timeMultiplier;
        SerializedProperty dateMultiplier;
        SerializedProperty pauseTime;
        SerializedProperty offset;
        SerializedProperty timeGatherMode;
        public static bool isSelectionWindowOpen;
        public static bool isCurrentSettingsWindowOpen;
        public static bool isLengthWindowOpen;
        public static bool isMovementWindowOpen;
        SystemTimeModule timeModule;

        public override GUIContent GetGUIContent()
        {

            //Place your module's GUI content here.
            return new GUIContent("    System Time", (Texture)Resources.Load("CozySystemTime"), "Link your in-game time to the real world time.");

        }
        
        public override void OpenDocumentationURL()
        {
            Application.OpenURL("https://distant-lands.gitbook.io/cozy-stylized-weather-documentation/how-it-works/modules/system-time-module");
        }
        public override void GetReportsInformation()
        {

            EditorGUILayout.LabelField(GetGUIContent(), EditorStyles.toolbar);
            EditorGUILayout.HelpBox("Currently it is " + timeModule.currentTime.ToString() + " on " + timeModule.MonthTitle(timeModule.GetCurrentYearPercentage()) + ".", MessageType.None, true);

        }

        void OnEnable()
        {
            timeModule = (SystemTimeModule)target;
            currentTimePercent = serializedObject.FindProperty("m_SystemTime");
            timeMultiplier = serializedObject.FindProperty("timeMultiplier");
            dateMultiplier = serializedObject.FindProperty("dateMultiplier");
            timeGatherMode = serializedObject.FindProperty("timeGatherMode");
            offset = serializedObject.FindProperty("hourOffset");
            pauseTime = serializedObject.FindProperty("pauseTime");

        }

        public override void DisplayInCozyWindow()
        {
            EditorGUI.indentLevel = 0;
            serializedObject.Update();

            EditorGUILayout.PropertyField(currentTimePercent);
            EditorGUILayout.PropertyField(pauseTime);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(timeGatherMode);
            EditorGUILayout.PropertyField(offset);
            EditorGUILayout.PropertyField(timeMultiplier);
            EditorGUILayout.PropertyField(dateMultiplier);

            serializedObject.ApplyModifiedProperties();

        }

    }
#endif
}