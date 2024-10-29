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
    [ExecuteAlways]
    public class CozyTransitModule : CozyModule
    {

        [System.Serializable]
        public struct TimeWeightRelation
        {
            [MeridiemTimeAttriute] public float time; [Range(0, 360)] public float sunHeight; [Range(0, 1)] public float weight;

            public TimeWeightRelation(float time, float sunHeight, float weight)
            {
                this.time = time;
                this.sunHeight = sunHeight;
                this.weight = weight;
            }
        }
        [HideInInspector]
        public AnimationCurve sunMovementCurve;

        [Tooltip("Specifies the default weight of the sunrise.")]
        public TimeWeightRelation sunriseWeight = new TimeWeightRelation(0.25f, 90, 0.2f);
        [Tooltip("Specifies the default weight of the day.")]
        public TimeWeightRelation dayWeight = new TimeWeightRelation(0.5f, 180, 0.2f);
        [Tooltip("Specifies the default weight of the sunset.")]
        public TimeWeightRelation sunsetWeight = new TimeWeightRelation(0.75f, 270, 0.2f);
        [Tooltip("Specifies the default weight of the night.")]
        public TimeWeightRelation nightWeight = new TimeWeightRelation(1, 360, 0.2f);

        [Tooltip("Specifies the day length multiplier in the spring.")]
        [Range(-1, 1)]
        public float springDayLengthOffset = 0;
        [Tooltip("Specifies the day length multiplier in the summer.")]
        [Range(-1, 1)]
        public float summerDayLengthOffset = 0.4f;
        [Tooltip("Specifies the day length multiplier in the fall.")]
        [Range(-1, 1)]
        public float fallDayLengthOffset = 0;
        [Tooltip("Specifies the day length multiplier in the winter.")]
        [Range(-1, 1)]
        public float winterDayLengthOffset = -0.3f;


        [HideTitle(4)]
        public AnimationCurve dayWeightsDisplayCurve;
        [HideTitle(4)]
        public AnimationCurve yearWeightsCurve;
        public enum TimeCurveSettings { linearDay, simpleCurve, advancedCurve }
        public TimeCurveSettings timeCurveSettings;

        [System.Serializable]
        public class TimeBlock
        {
            [MeridiemTimeAttriute]
            public float start;
            [MeridiemTimeAttriute]
            public float end;
            public TimeBlock(float startDayPercentage, float endDayPercentage)
            {
                start = startDayPercentage;
                end = endDayPercentage;
            }

        }

        public TimeBlock dawnBlock = new TimeBlock(4f / 24f, 5.5f / 24f);
        public TimeBlock morningBlock = new TimeBlock(6f / 24f, 7f / 24f);
        public TimeBlock dayBlock = new TimeBlock(7.5f / 24f, 9f / 24f);
        public TimeBlock afternoonBlock = new TimeBlock(13f / 24f, 14f / 24f);
        public TimeBlock eveningBlock = new TimeBlock(16f / 24f, 18f / 24f);
        public TimeBlock twilightBlock = new TimeBlock(20f / 24f, 21f / 24f);
        public TimeBlock nightBlock = new TimeBlock(21f / 24f, 22f / 24f);

        public void GetModifiedDayPercent()
        {

            yearWeightsCurve = new AnimationCurve(new Keyframe[5]
            {
                new Keyframe(0, winterDayLengthOffset, 0, 0),
                new Keyframe(0.25f, springDayLengthOffset, 0, 0),
                new Keyframe(0.5f, summerDayLengthOffset, 0, 0),
                new Keyframe(0.75f, fallDayLengthOffset, 0, 0),
                new Keyframe(1, winterDayLengthOffset, 0, 0)
            });

            float offset = yearWeightsCurve.Evaluate(weatherSphere.timeModule.yearPercentage) / 5;

            switch (timeCurveSettings)
            {

                case (TimeCurveSettings.advancedCurve):
                    sunMovementCurve = new AnimationCurve(new Keyframe[5]
                    {
                new Keyframe(0, 0, 0, 0, nightWeight.weight, nightWeight.weight),
                new Keyframe(sunriseWeight.time - offset, sunriseWeight.sunHeight, 0, 0, sunriseWeight.weight, sunriseWeight.weight),
                new Keyframe(dayWeight.time, dayWeight.sunHeight, 0, 0, dayWeight.weight, dayWeight.weight),
                new Keyframe(sunsetWeight.time + offset, sunsetWeight.sunHeight, 0, 0, sunsetWeight.weight, sunsetWeight.weight),
                new Keyframe(1, sunsetWeight.sunHeight > dayWeight.sunHeight ? 360 : 0, 0, 0, nightWeight.weight, nightWeight.weight)
                    });

                    dayWeightsDisplayCurve = new AnimationCurve(new Keyframe[5]
                    {
                new Keyframe(0, 0, 0, 0, nightWeight.weight, nightWeight.weight),
                new Keyframe(sunriseWeight.time - offset, sunriseWeight.sunHeight, 0, 0, sunriseWeight.weight, sunriseWeight.weight),
                new Keyframe(dayWeight.time, dayWeight.sunHeight, 0, 0, dayWeight.weight, dayWeight.weight),
                new Keyframe(sunsetWeight.time + offset, sunsetWeight.sunHeight > 180 ? 360 - sunsetWeight.sunHeight : sunsetWeight.sunHeight, 0, 0, sunsetWeight.weight, sunsetWeight.weight),
                new Keyframe(1, 0, 0, 0, nightWeight.weight, nightWeight.weight)
                    });
                    break;

                case (TimeCurveSettings.simpleCurve):
                    sunMovementCurve = new AnimationCurve(new Keyframe[5]
                    {
                new Keyframe(0, 0, 0, 0, nightWeight.weight, nightWeight.weight),
                new Keyframe(0.25f - offset, 90f, 0, 0, sunriseWeight.weight, sunriseWeight.weight),
                new Keyframe(0.5f, 180f, 0, 0, dayWeight.weight, dayWeight.weight),
                new Keyframe(0.75f + offset, 270f, 0, 0, sunsetWeight.weight, sunsetWeight.weight),
                new Keyframe(1, 360, 0, 0, nightWeight.weight, nightWeight.weight)
                    });

                    dayWeightsDisplayCurve = new AnimationCurve(new Keyframe[5]
                    {
                new Keyframe(0, 0, 0, 0, nightWeight.weight, nightWeight.weight),
                new Keyframe(0.25f - offset, 90f, 0, 0, sunriseWeight.weight, sunriseWeight.weight),
                new Keyframe(0.5f, 180f, 0, 0, dayWeight.weight, dayWeight.weight),
                new Keyframe(0.75f + offset, 90, 0, 0, sunsetWeight.weight, sunsetWeight.weight),
                new Keyframe(1, 0, 0, 0, nightWeight.weight, nightWeight.weight)
                    });
                    break;

                case (TimeCurveSettings.linearDay):
                    sunMovementCurve = new AnimationCurve(new Keyframe[5]
                    {
                new Keyframe(0, 0, 0, 0, 0, 0),
                new Keyframe(0.25f - offset, 90, 0, 0, 0, 0),
                new Keyframe(0.5f, 180, 0, 0, 0, 0),
                new Keyframe(0.75f + offset, 270, 0, 0, 0, 0),
                new Keyframe(1, 360, 0, 0, 0, 0)
                    });

                    dayWeightsDisplayCurve = new AnimationCurve(new Keyframe[5]
                    {
                new Keyframe(0, 0, 0, 0, 0, 0),
                new Keyframe(0.25f - offset, 90, 0, 0, 0, 0),
                new Keyframe(0.5f, 180, 0, 0, 0, 0),
                new Keyframe(0.75f + offset, 90, 0, 0, 0, 0),
                new Keyframe(1, 0, 0, 0, 0, 0)
                    });
                    break;
            }
        }

        public override void InitializeModule()
        {
            base.SetupModule(new Type[1] { typeof(CozyTimeModule) });

            CozyWeather.Events.onNewDay += GetModifiedDayPercent;
            if (weatherSphere.timeModule)
            {
                weatherSphere.timeModule.transit = this;

            }

        }


        void Start()
        {
            SetupTimeEvents();
            GetModifiedDayPercent();
        }

        void Update()
        {
            ManageTimeEvents();
        }

        private void ManageTimeEvents()
        {

            if (weatherSphere.timeModule.currentTime > weatherSphere.events.timeToCheckFor && !(weatherSphere.timeModule.currentTime > nightBlock.start && weatherSphere.events.timeToCheckFor == dawnBlock.start))
            {
                if (weatherSphere.timeModule.currentTime > nightBlock.start && weatherSphere.events.timeToCheckFor == nightBlock.start)
                {
                    weatherSphere.events.RaiseOnNight();
                    weatherSphere.events.timeToCheckFor = dawnBlock.start;
                }
                else if (weatherSphere.timeModule.currentTime > twilightBlock.start && weatherSphere.events.timeToCheckFor == twilightBlock.start)
                {
                    weatherSphere.events.RaiseOnTwilight();
                    weatherSphere.events.timeToCheckFor = nightBlock.start;
                }
                else if (weatherSphere.timeModule.currentTime > eveningBlock.start && weatherSphere.events.timeToCheckFor == eveningBlock.start)
                {
                    weatherSphere.events.RaiseOnEvening();
                    weatherSphere.events.timeToCheckFor = twilightBlock.start;
                }
                else if (weatherSphere.timeModule.currentTime > afternoonBlock.start && weatherSphere.events.timeToCheckFor == afternoonBlock.start)
                {
                    weatherSphere.events.RaiseOnAfternoon();
                    weatherSphere.events.timeToCheckFor = eveningBlock.start;
                }
                else if (weatherSphere.timeModule.currentTime > dayBlock.start && weatherSphere.events.timeToCheckFor == dayBlock.start)
                {
                    weatherSphere.events.RaiseOnDay();
                    weatherSphere.events.timeToCheckFor = afternoonBlock.start;
                }
                else if (weatherSphere.timeModule.currentTime > morningBlock.start && weatherSphere.events.timeToCheckFor == morningBlock.start)
                {
                    weatherSphere.events.RaiseOnMorning();
                    weatherSphere.events.timeToCheckFor = dayBlock.start;
                }
                else
                {
                    weatherSphere.events.RaiseOnDawn();
                    weatherSphere.events.timeToCheckFor = morningBlock.start;
                }
            }

            // if (weatherSphere.timeModule.currentTime < weatherSphere.events.timeToCheckFor - 0.25f) { SetupTimeEvents(); }
            if (Mathf.FloorToInt(weatherSphere.timeModule.currentTime * 24) != weatherSphere.events.currentHour)
            {
                weatherSphere.events.currentHour = Mathf.FloorToInt(weatherSphere.timeModule.currentTime * 24);
                weatherSphere.events.RaiseOnNewHour();
            }
            if (Mathf.FloorToInt(weatherSphere.timeModule.currentTime * 1440) != weatherSphere.events.currentMinute)
            {
                weatherSphere.events.currentMinute = Mathf.FloorToInt(weatherSphere.timeModule.currentTime * 1440);
                weatherSphere.events.RaiseOnMinutePass();
            }

        }

        private void SetupTimeEvents()
        {
            weatherSphere.events.timeToCheckFor = dawnBlock.start;
            if (weatherSphere.timeModule.currentTime > dawnBlock.start)
                weatherSphere.events.timeToCheckFor = morningBlock.start;
            if (weatherSphere.timeModule.currentTime > morningBlock.start)
                weatherSphere.events.timeToCheckFor = dayBlock.start;
            if (weatherSphere.timeModule.currentTime > dayBlock.start)
                weatherSphere.events.timeToCheckFor = afternoonBlock.start;
            if (weatherSphere.timeModule.currentTime > afternoonBlock.start)
                weatherSphere.events.timeToCheckFor = eveningBlock.start;
            if (weatherSphere.timeModule.currentTime > eveningBlock.start)
                weatherSphere.events.timeToCheckFor = twilightBlock.start;
            if (weatherSphere.timeModule.currentTime > twilightBlock.start)
                weatherSphere.events.timeToCheckFor = nightBlock.start;
            if (weatherSphere.timeModule.currentTime > nightBlock.start)
                weatherSphere.events.timeToCheckFor = dawnBlock.start;

            weatherSphere.events.currentHour = Mathf.FloorToInt(weatherSphere.timeModule.currentTime * 24);
            weatherSphere.events.currentMinute = Mathf.FloorToInt(weatherSphere.timeModule.currentTime * 1440);


        }

        public float ModifyDayPercentage(float input)
        {
            return sunMovementCurve.Evaluate(input);
        }



    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CozyTransitModule))]
    [CanEditMultipleObjects]
    public class E_TransitModule : E_CozyModule
    {

        CozyTransitModule transit;
        Data.PerennialProfile prof;
        public static bool timeBlocksWindow;
        public static bool curveWindow;
        public static bool yearCurveWindow;

        public override GUIContent GetGUIContent()
        {

            //Place your module's GUI content here.
            return new GUIContent("    Transit", (Texture)Resources.Load("Transit"), "Manage the sun moving through the sky.");

        }

        void OnEnable()
        {
            transit = (CozyTransitModule)target;
            if (transit.weatherSphere)
            prof = transit.weatherSphere.timeModule.perennialProfile;
        }
        
        public override void OpenDocumentationURL()
        {
            Application.OpenURL("https://distant-lands.gitbook.io/cozy-stylized-weather-documentation/how-it-works/modules/transit-module");
        }
        public override void GetReportsInformation()
        {

            EditorGUILayout.LabelField(GetGUIContent(), EditorStyles.toolbar);
            EditorGUILayout.HelpBox("Currently the Transit module is modifying the current time of " + transit.weatherSphere.timeModule.currentTime.ToString() + " to appear with the same sun position as it would at " + (MeridiemTime)(transit.ModifyDayPercentage(transit.weatherSphere.timeModule.currentTime) / 360) + " without the Transit module.", MessageType.None, true);

        }

        public override void DisplayInCozyWindow()
        {
            EditorGUI.indentLevel = 0;
            serializedObject.Update();
            timeBlocksWindow = EditorGUILayout.BeginFoldoutHeaderGroup(timeBlocksWindow,
                           new GUIContent("    Time Blocks Window"), EditorUtilities.FoldoutStyle);
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (timeBlocksWindow)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField("Dawn");
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("dawnBlock").FindPropertyRelative("start"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("dawnBlock").FindPropertyRelative("end"));
                EditorGUI.indentLevel--;
                EditorGUILayout.LabelField("Morning");
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("morningBlock").FindPropertyRelative("start"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("morningBlock").FindPropertyRelative("end"));
                EditorGUI.indentLevel--;
                EditorGUILayout.LabelField("Day");
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("dayBlock").FindPropertyRelative("start"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("dayBlock").FindPropertyRelative("end"));
                EditorGUI.indentLevel--;
                EditorGUILayout.LabelField("Afternoon");
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("afternoonBlock").FindPropertyRelative("start"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("afternoonBlock").FindPropertyRelative("end"));
                EditorGUI.indentLevel--;
                EditorGUILayout.LabelField("Evening");
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("eveningBlock").FindPropertyRelative("start"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("eveningBlock").FindPropertyRelative("end"));
                EditorGUI.indentLevel--;
                EditorGUILayout.LabelField("Twilight");
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("twilightBlock").FindPropertyRelative("start"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("twilightBlock").FindPropertyRelative("end"));
                EditorGUI.indentLevel--;
                EditorGUILayout.LabelField("Night");
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("nightBlock").FindPropertyRelative("start"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("nightBlock").FindPropertyRelative("end"));
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            curveWindow = EditorGUILayout.BeginFoldoutHeaderGroup(curveWindow,
                new GUIContent("    Day Curve Settings"), EditorUtilities.FoldoutStyle);
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (curveWindow)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(serializedObject.FindProperty("timeCurveSettings"));
                EditorGUILayout.Space();
                transit.GetModifiedDayPercent();

                EditorGUI.indentLevel++;

                switch ((CozyTransitModule.TimeCurveSettings)serializedObject.FindProperty("timeCurveSettings").enumValueIndex)
                {

                    case (CozyTransitModule.TimeCurveSettings.linearDay):
                        break;
                    case (CozyTransitModule.TimeCurveSettings.simpleCurve):
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("sunriseWeight").FindPropertyRelative("weight"), new GUIContent("Sunrise Weight"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dayWeight").FindPropertyRelative("weight"), new GUIContent("Day Weight"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("sunsetWeight").FindPropertyRelative("weight"), new GUIContent("Sunset Weight"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("nightWeight").FindPropertyRelative("weight"), new GUIContent("Night Weight"));
                        break;
                    case (CozyTransitModule.TimeCurveSettings.advancedCurve):
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("sunriseWeight"), new GUIContent("Sunrise Settings"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dayWeight"), new GUIContent("Day Settings"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("sunsetWeight"), new GUIContent("Sunset Settings"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("nightWeight"), new GUIContent("Night Settings"));
                        break;
                }

                EditorGUI.indentLevel--;

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("dayWeightsDisplayCurve"));

                EditorGUI.indentLevel--;
            }

            yearCurveWindow = EditorGUILayout.BeginFoldoutHeaderGroup(yearCurveWindow,
                new GUIContent("    Seasonal Variation"), EditorUtilities.FoldoutStyle);
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (yearCurveWindow)
            {
                EditorGUI.indentLevel++;

                transit.GetModifiedDayPercent();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("springDayLengthOffset"), new GUIContent("Spring Offset"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("summerDayLengthOffset"), new GUIContent("Summer Offset"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fallDayLengthOffset"), new GUIContent("Fall Offset"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("winterDayLengthOffset"), new GUIContent("Winter Offset"));

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("yearWeightsCurve"));

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();

        }

    }
#endif
}