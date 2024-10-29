//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif



namespace DistantLands.Cozy.Data
{

    [System.Serializable]
    [CreateAssetMenu(menuName = "Distant Lands/Cozy/Climate Profile", order = 361)]
    public class ClimateProfile : ScriptableObject
    {


        [Tooltip("The global temprature during the year. the x-axis is the current day over the days in the year and the y axis is the temprature in farenheit.")]
        [FormerlySerializedAs("tempratureOverYear")]
        public AnimationCurve temperatureOverYear;
        [Tooltip("The global precipitation during the year. the x-axis is the current day over the days in the year and the y axis is the precipitation.")]
        public AnimationCurve humidityOverYear;
        [Tooltip("The local temprature during the day. the x-axis is the current ticks over 360 and the y axis is the temprature change in farenheit from the global temprature.")]
        [FormerlySerializedAs("tempratureOverDay")]
        public AnimationCurve temperatureOverDay;
        [Tooltip("The local precipitation during the day. the x-axis is the current ticks over 360 and the y axis is the precipitation change from the global precipitation.")]
        public AnimationCurve humidityOverDay;

        [Tooltip("Adds an offset to the global temprature. Useful for adding biomes or climate change by location or elevation")]
        public float temperatureFilter;
        [Tooltip("Adds an offset to the global precipitation. Useful for adding biomes or climate change by location or elevation")]
        public float humidityFilter;

        public float GetTemperature()
        {

            CozyWeather weather = CozyWeather.instance;

            float i = (temperatureOverYear.Evaluate(weather.yearPercentage) * temperatureOverDay.Evaluate(weather.modifiedDayPercentage)) + temperatureFilter;

            return i;
        }
        public float GetTemperature(CozyWeather weather)
        {
            if (weather == null)
                return GetTemperature();

            float i = (temperatureOverYear.Evaluate(weather.yearPercentage) * temperatureOverDay.Evaluate(weather.modifiedDayPercentage)) + temperatureFilter;


            return i;
        }
        public float GetTemperature(CozyWeather weather, float inTime)
        {

            if (!weather.timeModule)
                return GetTemperature(weather);

            float nextDays = inTime;

            float i = (temperatureOverYear.Evaluate((weather.timeModule.DayAndTime() + nextDays) / weather.timeModule.GetDaysPerYear()) * temperatureOverDay.Evaluate(weather.timeModule.modifiedDayPercentage)) + temperatureFilter;

            return i;
        }
        public float GetHumidity()
        {
            CozyWeather weather = CozyWeather.instance;
            float i = (humidityOverYear.Evaluate(weather.yearPercentage) * humidityOverDay.Evaluate(weather.modifiedDayPercentage)) + humidityFilter;

            return i;
        }
        public float GetHumidity(CozyWeather weather)
        {
            if (weather == null)
                weather = CozyWeather.instance;
                
            float i = (humidityOverYear.Evaluate(weather.yearPercentage) * humidityOverDay.Evaluate(weather.modifiedDayPercentage)) + humidityFilter;

            return i;
        }
        public float GetHumidity(CozyWeather weather, float inTime)
        {
            if (!weather.timeModule)
                return GetHumidity(weather);

            float nextDays = inTime;
            float i = (humidityOverYear.Evaluate((weather.timeModule.DayAndTime() + nextDays) / weather.perennialProfile.daysPerYear) * humidityOverDay.Evaluate(weather.timeModule.modifiedDayPercentage)) + humidityFilter;

            return i;
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ClimateProfile))]
    [CanEditMultipleObjects]
    public class E_ClimateProfile : Editor
    {

        SerializedProperty tempratureOverYear;
        SerializedProperty precipitationOverYear;
        SerializedProperty tempratureOverDay;
        SerializedProperty precipitationOverDay;
        SerializedProperty tempratureFilter;
        SerializedProperty precipitationFilter;
        ClimateProfile prof;

        void OnEnable()
        {

            tempratureOverYear = serializedObject.FindProperty("temperatureOverYear");
            precipitationOverYear = serializedObject.FindProperty("humidityOverYear");
            tempratureOverDay = serializedObject.FindProperty("temperatureOverDay");
            precipitationOverDay = serializedObject.FindProperty("humidityOverDay");
            tempratureFilter = serializedObject.FindProperty("temperatureFilter");
            precipitationFilter = serializedObject.FindProperty("humidityFilter");
            prof = (ClimateProfile)target;

        }


        public override void OnInspectorGUI()
        {

            serializedObject.Update();
            Undo.RecordObject(prof, prof.name + " Profile Changes");

            EditorGUILayout.LabelField("Global Curves", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(tempratureOverYear);
            EditorGUILayout.PropertyField(precipitationOverYear);
            EditorGUILayout.PropertyField(tempratureOverDay);
            EditorGUILayout.PropertyField(precipitationOverDay);
            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("Global Filters", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(tempratureFilter);
            EditorGUILayout.PropertyField(precipitationFilter);

            EditorGUILayout.Space();
            EditorUtility.SetDirty(prof);

            serializedObject.ApplyModifiedProperties();

        }
    }
#endif
}