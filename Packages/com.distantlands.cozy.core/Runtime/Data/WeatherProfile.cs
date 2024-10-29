//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using System.Collections.Generic;
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif



namespace DistantLands.Cozy.Data
{

    [System.Serializable]
    [CreateAssetMenu(menuName = "Distant Lands/Cozy/Weather Profile", order = 361)]
    public class WeatherProfile : ScriptableObject
    {

        [Tooltip("Specifies the minimum length for this weather profile in in-game hours and minutes.")]
        [MeridiemTimeAttriute]
        public float minWeatherTime = 0.25f;
        [Tooltip("Specifies the maximum length for this weather profile in in-game hours and minutes.")]
        [MeridiemTimeAttriute]
        public float maxWeatherTime = 0.35f;
        [Tooltip("Multiplier for the computational chance that this weather profile will play; 0 being never, and 2 being twice as likely as the average.")]
        [Range(0, 2)]
        public float likelihood = 1;
        [HideTitle]
        [Tooltip("Allow only these weather profiles to immediately follow this weather profile in a forecast.")]
        public WeatherProfile[] forecastNext;
        public enum ForecastModifierMethod { forecastNext, DontForecastNext, forecastAnyProfileNext }
        public ForecastModifierMethod forecastModifierMethod = ForecastModifierMethod.forecastAnyProfileNext;
        [Tooltip("Animation curves that increase or decrease weather chance based on time, temprature, etc.")]
        [ChanceEffector]
        public List<ChanceEffector> chances = new List<ChanceEffector>();

        [FX]
        public FXProfile[] FX;

        public float GetChance(CozyWeather weather, float inTime)
        {
            float i = likelihood;
            foreach (ChanceEffector j in chances)
            {
                i *= j.GetChance(weather, inTime);
            }
            return i > 0 ? i : 0;
        }

        public float GetChance(CozyWeather weather)
        {
            float i = likelihood;

            foreach (ChanceEffector j in chances)
            {
                i *= j.GetChance(weather);
            }

            return i > 0 ? i : 0;
        }

        public void SetWeatherWeight(float weightVal)
        {
            foreach (FXProfile fx in FX)
                fx?.PlayEffect(weightVal);
        }

    }


#if UNITY_EDITOR
    [CustomEditor(typeof(WeatherProfile))]
    [CanEditMultipleObjects]
    public class E_WeatherProfile : Editor
    {

        WeatherProfile prof;



        void OnEnable()
        {

            prof = (WeatherProfile)target;

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Forecasting Behaviours", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minWeatherTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxWeatherTime"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("likelihood"));

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("forecastModifierMethod"), true);
            switch ((WeatherProfile.ForecastModifierMethod)serializedObject.FindProperty("forecastModifierMethod").intValue)
            {

                default:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("forecastNext"), new GUIContent("Forecast Modifiers", "Modifies the weather profiles that follow this in the forecast. Use the dropdown to force the forecast to either choose only one of the included profiles to forecast next, or to avoid the selected profiles entirely."), true);
                    break;
                case (WeatherProfile.ForecastModifierMethod.DontForecastNext):
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("forecastNext"), new GUIContent("Don't Forecast Next", "The forecast module will not select any of these weather profiles to immediately follow this profile in the forecast."), true);
                    break;
                case (WeatherProfile.ForecastModifierMethod.forecastNext):
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("forecastNext"), new GUIContent("Forecast Next", "The forecast module will only select one of these weather profiles to immediately follow this profile in the forecast."), true);
                    break;
                case (WeatherProfile.ForecastModifierMethod.forecastAnyProfileNext):
                    break;

            }
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("chances"), new GUIContent("Chance Effectors"), true);
            EditorGUI.indentLevel--;


            EditorGUILayout.Space(20);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("FX"), new GUIContent("Weather Effects"));




            serializedObject.ApplyModifiedProperties();


        }


        public void DisplayInCozyWindow()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Forecasting Behaviours", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minWeatherTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxWeatherTime"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("likelihood"));

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("forecastModifierMethod"), true);
            switch ((WeatherProfile.ForecastModifierMethod)serializedObject.FindProperty("forecastModifierMethod").intValue)
            {

                default:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("forecastNext"), new GUIContent("Forecast Modifiers", "Modifies the weather profiles that follow this in the forecast. Use the dropdown to force the forecast to either choose only one of the included profiles to forecast next, or to avoid the selected profiles entirely."), true);
                    break;
                case (WeatherProfile.ForecastModifierMethod.DontForecastNext):
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("forecastNext"), new GUIContent("Don't Forecast Next", "The forecast module will not select any of these weather profiles to immediately follow this profile in the forecast."), true);
                    break;
                case (WeatherProfile.ForecastModifierMethod.forecastNext):
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("forecastNext"), new GUIContent("Forecast Next", "The forecast module will only select one of these weather profiles to immediately follow this profile in the forecast."), true);
                    break;
                case (WeatherProfile.ForecastModifierMethod.forecastAnyProfileNext):
                    break;

            }
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("chances"), new GUIContent("Chance Effectors"), true);
            EditorGUI.indentLevel--;


            EditorGUILayout.Space(20);


            EditorGUILayout.PropertyField(serializedObject.FindProperty("FX"), new GUIContent("Weather Effects"));




            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

}