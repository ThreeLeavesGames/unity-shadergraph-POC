//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DistantLands.Cozy.Data;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DistantLands.Cozy
{
    public class CozyEventModule : CozyModule
    {


        public UnityEvent onDawn;
        public UnityEvent onMorning;
        public UnityEvent onDay;
        public UnityEvent onAfternoon;
        public UnityEvent onEvening;
        public UnityEvent onTwilight;
        public UnityEvent onNight;
        public UnityEvent onNewMinute;
        public UnityEvent onNewHour;
        public UnityEvent onNewDay;
        public UnityEvent onNewYear;
        public UnityEvent onWeatherProfileChange;

        [System.Serializable]
        public class CozyEvent
        {

            public EventFX fxReference;
            public UnityEvent onPlay;
            public UnityEvent onStop;

        }

        public CozyEvent[] cozyEvents;


        public override void InitializeModule()
        {
            base.InitializeModule();

            if (GetComponent<CozyWeather>())
            {

                GetComponent<CozyWeather>().InitializeModule(typeof(CozyEventModule));
                DestroyImmediate(this);
                Debug.LogWarning("Add modules in the settings tab in COZY 2!");
                return;

            }

            if (Application.isPlaying)
            {
                foreach (CozyEvent i in cozyEvents)
                {
                    if (i.fxReference)
                    {
                        i.fxReference.onCall += i.onPlay.Invoke;
                        i.fxReference.onEnd += i.onStop.Invoke;
                    }
                }

                StartCoroutine(Refresh());
            }

        }

        public override void DeinitializeModule()
        {
            base.DeinitializeModule();

            if (Application.isPlaying)
            {
                foreach (CozyEvent i in cozyEvents)
                {
                    if (i.fxReference)
                    {
                        i.fxReference.onCall -= i.onPlay.Invoke;
                        i.fxReference.onEnd -= i.onStop.Invoke;
                    }
                }

                CozyWeather.Events.onDawn -= onDawn.Invoke;
                CozyWeather.Events.onMorning -= onMorning.Invoke;
                CozyWeather.Events.onDay -= onDay.Invoke;
                CozyWeather.Events.onAfternoon -= onAfternoon.Invoke;
                CozyWeather.Events.onEvening -= onEvening.Invoke;
                CozyWeather.Events.onTwilight -= onTwilight.Invoke;
                CozyWeather.Events.onNight -= onNight.Invoke;
                CozyWeather.Events.onNewMinute -= onNewMinute.Invoke;
                CozyWeather.Events.onNewHour -= onNewHour.Invoke;
                CozyWeather.Events.onNewDay -= onNewDay.Invoke;
                CozyWeather.Events.onNewYear -= onNewYear.Invoke;
                CozyWeather.Events.onWeatherChange -= onWeatherProfileChange.Invoke;

            }
        }

        public IEnumerator Refresh()
        {

            yield return new WaitForEndOfFrame();

            CozyWeather.Events.onDawn += onDawn.Invoke;
            CozyWeather.Events.onMorning += onMorning.Invoke;
            CozyWeather.Events.onDay += onDay.Invoke;
            CozyWeather.Events.onAfternoon += onAfternoon.Invoke;
            CozyWeather.Events.onEvening += onEvening.Invoke;
            CozyWeather.Events.onTwilight += onTwilight.Invoke;
            CozyWeather.Events.onNight += onNight.Invoke;
            CozyWeather.Events.onNewMinute += onNewMinute.Invoke;
            CozyWeather.Events.onNewHour += onNewHour.Invoke;
            CozyWeather.Events.onNewDay += onNewDay.Invoke;
            CozyWeather.Events.onNewYear += onNewYear.Invoke;
            CozyWeather.Events.onWeatherChange += onWeatherProfileChange.Invoke;

        }

        public void LogConsoleEvent()
        {

            Debug.Log("Test Event Passed.");

        }

        public void LogConsoleEvent(string log)
        {

            Debug.Log($"Test Event Passed. Log: {log}");

        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(CozyEventModule))]
    [CanEditMultipleObjects]
    public class E_EventManager : E_CozyModule
    {

        protected static bool todEvents;
        protected static bool teEvents;
        protected static bool weatherEvents;
        protected static bool eventSettings;
        SerializedProperty cozyEvents;

        public override GUIContent GetGUIContent()
        {

            return new GUIContent("    Events", (Texture)Resources.Load("Events"), "Setup Unity events that directly integrate into the COZY system.");

        }

        void OnEnable()
        {
            cozyEvents = serializedObject.FindProperty("cozyEvents");
        }

        public override void OpenDocumentationURL()
        {
            Application.OpenURL("https://distant-lands.gitbook.io/cozy-stylized-weather-documentation/how-it-works/modules/events-module");
        }

        public override void DisplayInCozyWindow()
        {
            EditorGUI.indentLevel = 0;
            serializedObject.Update();

            todEvents = EditorGUILayout.BeginFoldoutHeaderGroup(todEvents,
                    new GUIContent("    Time of Day Events"), EditorUtilities.FoldoutStyle);

            EditorGUILayout.EndFoldoutHeaderGroup();
            if (todEvents)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onDawn"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onMorning"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onDay"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onAfternoon"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onEvening"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onTwilight"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onNight"));
                EditorGUI.indentLevel--;
            }

            teEvents = EditorGUILayout.BeginFoldoutHeaderGroup(teEvents,
                new GUIContent("    Time Elapsed Events"), EditorUtilities.FoldoutStyle);

            EditorGUILayout.EndFoldoutHeaderGroup();
            if (teEvents)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onNewMinute"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onNewHour"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onNewDay"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onNewYear"));
                EditorGUI.indentLevel--;
            }

            weatherEvents = EditorGUILayout.BeginFoldoutHeaderGroup(weatherEvents,
                new GUIContent("    Weather Events"), EditorUtilities.FoldoutStyle);

            EditorGUILayout.EndFoldoutHeaderGroup();
            if (weatherEvents)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onWeatherProfileChange"));

                for (int i = 0; i < cozyEvents.arraySize; i++)
                {
                    string name = "New Event FX";
                    if (cozyEvents.GetArrayElementAtIndex(i).FindPropertyRelative("fxReference").objectReferenceValue)
                        name = cozyEvents.GetArrayElementAtIndex(i).FindPropertyRelative("fxReference").objectReferenceValue.name;
                    EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(cozyEvents.GetArrayElementAtIndex(i).FindPropertyRelative("fxReference"));
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(cozyEvents.GetArrayElementAtIndex(i).FindPropertyRelative("onPlay"));
                    EditorGUILayout.PropertyField(cozyEvents.GetArrayElementAtIndex(i).FindPropertyRelative("onStop"));
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Add New"))
                    {
                        cozyEvents.InsertArrayElementAtIndex(i + 1);
                    }
                    if (GUILayout.Button("Remove"))
                        cozyEvents.DeleteArrayElementAtIndex(i);

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();

                }

                if (GUILayout.Button("Add New Event FX Reference"))
                {
                    cozyEvents.InsertArrayElementAtIndex(cozyEvents.arraySize);
                }

            }

            serializedObject.ApplyModifiedProperties();

        }

    }
#endif
}