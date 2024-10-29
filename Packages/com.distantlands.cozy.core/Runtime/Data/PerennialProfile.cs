//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using System.Collections.Generic;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UnityEngine;



namespace DistantLands.Cozy.Data
{

    [System.Serializable]
    [CreateAssetMenu(menuName = "Distant Lands/Cozy/Perennial Profile", order = 361)]
    public class PerennialProfile : ClimateProfile
    {
        [HideInInspector]
        public float dayAndTime;
        public bool pauseTime;
        [Tooltip("Should this profile use a series of months for a more realistic year.")]
        public bool realisticYear;
        [Tooltip("Should this profile use a longer year every 4th year.")]
        public bool useLeapYear;

        [Tooltip("Should this system reset the time when it loads?")]
        public bool resetTimeOnStart = false;
        [Tooltip("The time that this system should start at when the scene is loaded.")]
        public MeridiemTime startTime = new MeridiemTime(9, 00);
        [Tooltip("Specifies the amount of in-game minutes that pass in a real-world second.")]
        public float timeMovementSpeed = 1;
        [Tooltip("Changes the time speed based on the day percentage.")]
        public AnimationCurve timeSpeedMultiplier;

        [System.Serializable]
        public class Month
        {

            public string name;
            public int days;

        }

        [MonthList]
        public Month[] standardYear = new Month[12] { new Month() { days = 31, name = "January"}, new Month() { days = 28, name = "Febraury" },
        new Month() { days = 31, name = "March"}, new Month() { days = 30, name = "April"}, new Month() { days = 31, name = "May"},
        new Month() { days = 30, name = "June"}, new Month() { days = 31, name = "July"}, new Month() { days = 31, name = "August"},
        new Month() { days = 30, name = "September"}, new Month() { days = 31, name = "October"}, new Month() { days = 30, name = "Novemeber"},
        new Month() { days = 31, name = "December"}};

        [MonthList]
        public Month[] leapYear = new Month[12] { new Month() { days = 31, name = "January"}, new Month() { days = 29, name = "Febraury" },
        new Month() { days = 31, name = "March"}, new Month() { days = 30, name = "April"}, new Month() { days = 31, name = "May"},
        new Month() { days = 30, name = "June"}, new Month() { days = 31, name = "July"}, new Month() { days = 31, name = "August"},
        new Month() { days = 30, name = "September"}, new Month() { days = 31, name = "October"}, new Month() { days = 30, name = "Novemeber"},
        new Month() { days = 31, name = "December"}};

        public enum DefaultYear { January, February, March, April, May, June, July, August, September, October, November, December }
        public enum TimeDivisors { Early, Mid, Late }

        public enum TimeCurveSettings { linearDay, simpleCurve, advancedCurve }
        public TimeCurveSettings timeCurveSettings;

        public int daysPerYear = 48;

        public int GetRealisticDaysPerYear(int currentYear)
        {

            int i = 0;
            foreach (Month j in (useLeapYear && currentYear % 4 == 0) ? leapYear : standardYear) i += j.days;
            return i;


        }


    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PerennialProfile))]
    [CanEditMultipleObjects]
    public class E_PerennialProfile : Editor
    {

        SerializedProperty timeSpeedMultiplier;
        SerializedProperty standardYear;
        SerializedProperty leapYear;
        PerennialProfile prof;

        void OnEnable()
        {

            timeSpeedMultiplier = serializedObject.FindProperty("timeSpeedMultiplier");
            standardYear = serializedObject.FindProperty("standardYear");
            leapYear = serializedObject.FindProperty("leapYear");
            prof = (PerennialProfile)target;

        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Time Movement", EditorStyles.boldLabel);
            prof.pauseTime = EditorGUILayout.Toggle("Pause Time", prof.pauseTime);
            if (!prof.pauseTime)
            {
                prof.timeMovementSpeed = EditorGUILayout.FloatField("Time Speed", prof.timeMovementSpeed);
                EditorGUILayout.PropertyField(timeSpeedMultiplier);
            }
            EditorGUILayout.Space(20);

            EditorGUILayout.Space(10);
            prof.realisticYear = EditorGUILayout.Toggle("Realistic Year", prof.realisticYear);
            if (prof.realisticYear)
            {
                prof.useLeapYear = EditorGUILayout.Toggle("Use Leap Year", prof.useLeapYear);

                EditorGUILayout.Space(10);
                EditorGUILayout.PropertyField(standardYear);
                if (prof.useLeapYear)
                    EditorGUILayout.PropertyField(leapYear);
            }
            else
            {

                prof.daysPerYear = EditorGUILayout.IntField("Days Per Year", prof.daysPerYear);

            }

            serializedObject.ApplyModifiedProperties();

        }

        public void OnStaticMeasureGUI(GUIStyle style, ref bool lengthWindow, ref bool movementWindow)
        {

            serializedObject.Update();


            movementWindow = EditorGUILayout.BeginFoldoutHeaderGroup(movementWindow,
                new GUIContent("    Movement Settings"), EditorUtilities.FoldoutStyle);

            if (movementWindow)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(serializedObject.FindProperty("pauseTime"));
                if (!serializedObject.FindProperty("pauseTime").boolValue)
                {

                    EditorGUILayout.Space();
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("resetTimeOnStart"));
                    if (serializedObject.FindProperty("resetTimeOnStart").boolValue)
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("startTime"));
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("timeMovementSpeed"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("timeSpeedMultiplier"));
                    EditorGUI.indentLevel--;


                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            lengthWindow = EditorGUILayout.BeginFoldoutHeaderGroup(lengthWindow,
                new GUIContent("    Length Settings"), EditorUtilities.FoldoutStyle);
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (lengthWindow)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(serializedObject.FindProperty("realisticYear"));
                if (serializedObject.FindProperty("realisticYear").boolValue)
                {


                    EditorGUILayout.PropertyField(serializedObject.FindProperty("useLeapYear"));
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("standardYear"));

                    if (serializedObject.FindProperty("useLeapYear").boolValue)
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("leapYear"));

                }
                else
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("daysPerYear"));

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();

        }

        public void OnRuntimeMeasureGUI()
        {

            serializedObject.Update();


            serializedObject.FindProperty("currentTicks").floatValue = EditorGUILayout.Slider("Current Ticks", serializedObject.FindProperty("currentTicks").floatValue, 0, serializedObject.FindProperty("ticksPerDay").floatValue);
            serializedObject.FindProperty("currentDay").intValue = EditorGUILayout.IntSlider(new GUIContent("Current Day"), serializedObject.FindProperty("currentDay").intValue, 0, serializedObject.FindProperty("daysPerYear").intValue);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("currentYear"));

            serializedObject.ApplyModifiedProperties();

        }

    }
#endif
}