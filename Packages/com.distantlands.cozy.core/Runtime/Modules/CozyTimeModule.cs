//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using System.Collections;
using UnityEngine;
using DistantLands.Cozy.Data;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DistantLands.Cozy
{
    [ExecuteAlways]
    public class CozyTimeModule : CozyModule
    {

        public CozyTransitModule transit;
        public PerennialProfile perennialProfile;
        public CozyDateOverride overrideDate;
        [MeridiemTimeAttriute]
        [SerializeField]
        private float m_DayPercentage = 0.5f;
        [Range(0, 1)]
        public float yearPercentage = 0;
        public float modifiedDayPercentage;
        public bool transitioningTime;

        public MeridiemTime currentTime
        {
            get { return m_DayPercentage; }
            set { m_DayPercentage = value; }

        }

        public int currentDay;
        public int currentYear;
        public CozyTimeModule parentModule;


        public override void InitializeModule()
        {
            base.InitializeModule();
            weatherSphere.timeModule = this;
        }

        internal override bool CheckIfModuleCanBeRemoved(out string warning)
        {
            if (weatherSphere.GetModule<CozyTransitModule>() != null)
            {
                warning = "Transit Module";
                return false;
            }
            warning = "";
            return true;
        }

        internal override bool CheckIfModuleCanBeAdded(out string warning)
        {
            if (weatherSphere.GetModule<SystemTimeModule>() != null)
            {
                warning = "System Time Module";
                return false;
            }
            warning = "";
            return true;
        }

        void Start()
        {
            SetupTime();
        }

        void Update()
        {

            if (weatherSphere.timeModule == null)
                weatherSphere.timeModule = this;

            ManageTime();

            yearPercentage = GetCurrentYearPercentage();
            modifiedDayPercentage = transit ? transit.ModifyDayPercentage(m_DayPercentage) / 360 : m_DayPercentage;

        }

        void SetupTime()
        {
            if (perennialProfile.resetTimeOnStart)
                currentTime = perennialProfile.startTime;


            if (perennialProfile.realisticYear)
                perennialProfile.daysPerYear = perennialProfile.GetRealisticDaysPerYear(currentYear);

        }

        /// <summary>
        /// Constrains the time to fit within the length parameters set on the perennial profile.
        /// </summary> 
        private void ConstrainTime()
        {
            if (m_DayPercentage >= 1)
            {
                m_DayPercentage -= 1;
                ChangeDay(1);
                weatherSphere.events.timeToCheckFor = 0.25f;
                weatherSphere.events.RaiseOnDayChange();
            }

            if (m_DayPercentage < 0)
            {
                m_DayPercentage += 1;
                ChangeDay(-1);
                weatherSphere.events.RaiseOnDayChange();
            }
        }

        private void ChangeDay(int change)
        {

            if (overrideDate)
            {
                overrideDate.ChangeDay(change);
                return;
            }

            currentDay += change;

            if (currentDay >= perennialProfile.daysPerYear)
            {
                currentDay -= perennialProfile.daysPerYear;
                currentYear++;
                weatherSphere.events.RaiseOnYearChange();
            }

            if (currentDay < 0)
            {
                currentDay += perennialProfile.daysPerYear;
                currentYear--;
                weatherSphere.events.RaiseOnYearChange();
            }
        }

        public int GetDaysPerYear()
        {
            if (overrideDate)
                return overrideDate.DaysPerYear();

            if (perennialProfile.realisticYear)
                return perennialProfile.GetRealisticDaysPerYear(currentYear);
            else
                return perennialProfile.daysPerYear;
        }

        /// <summary>
        /// Returns the current year percentage (0 - 1).
        /// </summary> 
        public float GetCurrentYearPercentage()
        {

            if (overrideDate)
                return overrideDate.GetCurrentYearPercentage();

            float dat = DayAndTime();
            return dat / (float)GetDaysPerYear();
        }

        /// <summary>
        /// Returns the current year percentage (0 - 1) after a number of ticks has passed.
        /// </summary> 
        public float GetCurrentYearPercentage(float inTIme)
        {
            if (overrideDate)
                return overrideDate.GetCurrentYearPercentage(inTIme);

            float dat = DayAndTime() + inTIme;
            return dat / perennialProfile.daysPerYear;
        }

        /// <summary>
        /// Gets the current day plus the current day percentage (0-1). 
        /// </summary> 
        public float DayAndTime()
        {
            if (overrideDate)
                return overrideDate.DayAndTime();

            return currentDay + m_DayPercentage;

        }

        /// <summary>
        /// Manages the movement of time in the scene.
        /// </summary> 
        public void ManageTime()
        {

            if (Application.isPlaying && !perennialProfile.pauseTime)
                m_DayPercentage += modifiedTimeSpeed * Time.deltaTime;


            ConstrainTime();

        }

        public float modifiedTimeSpeed
        {
            get
            {
                return perennialProfile.timeMovementSpeed * perennialProfile.timeSpeedMultiplier.Evaluate(m_DayPercentage) / 1440;
            }
        }

        /// <summary>
        /// Skips the weather system forward by the ticksToSkip value.
        /// </summary> 
        public void SkipTime(MeridiemTime timeToSkip)
        {


            currentTime += (float)timeToSkip;

            if (weatherSphere.GetModule<CozyAmbienceModule>())
                weatherSphere.GetModule<CozyAmbienceModule>().SkipTime(timeToSkip);

            foreach (CozySystem i in weatherSphere.systems)
            {
                i.SkipTime(timeToSkip);
            }

        }

        public void SkipTime(MeridiemTime timeToSkip, int daysToSkip)
        {

            currentTime += (float)timeToSkip;
            currentDay += daysToSkip;

            if (weatherSphere.GetModule<CozyAmbienceModule>())
                weatherSphere.GetModule<CozyAmbienceModule>().SkipTime(timeToSkip + daysToSkip);

            foreach (CozySystem i in weatherSphere.systems)
            {
                i.SkipTime(timeToSkip + daysToSkip);
            }
        }

        public void SetHour(int hour)
        {
            currentTime = new MeridiemTime(hour, currentTime.minutes, currentTime.seconds, currentTime.milliseconds);
        }
        public void SetMinute(int minute)
        {
            currentTime = new MeridiemTime(currentTime.hours, minute, currentTime.seconds, currentTime.milliseconds);
        }

        /// <summary>
        /// Returns the title for the current month.
        /// </summary> 
        public string MonthTitle(float month)
        {


            if (perennialProfile.realisticYear)
            {

                GetCurrentMonth(out string monthName, out int monthDay, out float monthPercentage);
                return monthName + " " + monthDay;

            }
            else
            {

                float j = Mathf.Floor(month * 12);
                float monthLength = perennialProfile.daysPerYear / 12;
                float monthTime = DayAndTime() - (j * monthLength);

                PerennialProfile.DefaultYear monthName = (PerennialProfile.DefaultYear)j;
                PerennialProfile.TimeDivisors monthTimeName = PerennialProfile.TimeDivisors.Mid;

                if ((monthTime / monthLength) < 0.33f)
                    monthTimeName = PerennialProfile.TimeDivisors.Early;
                else if ((monthTime / monthLength) > 0.66f)
                    monthTimeName = PerennialProfile.TimeDivisors.Late;
                else
                    monthTimeName = PerennialProfile.TimeDivisors.Mid;


                return $"{monthTimeName} {monthName}";
            }
        }

        public void GetCurrentMonth(out string monthName, out int monthDay, out float monthPercentage)
        {

            int i = currentDay;
            int j = 0;

            while (i > ((perennialProfile.useLeapYear && currentYear % 4 == 0) ? perennialProfile.leapYear[j].days : perennialProfile.standardYear[j].days))
            {

                i -= (perennialProfile.useLeapYear && currentYear % 4 == 0) ? perennialProfile.leapYear[j].days : perennialProfile.standardYear[j].days;

                j++;

                if (j >= ((perennialProfile.useLeapYear && currentYear % 4 == 0) ? perennialProfile.leapYear.Length : perennialProfile.standardYear.Length))
                    break;

            }

            PerennialProfile.Month k = (perennialProfile.useLeapYear && currentYear % 4 == 0) ? perennialProfile.leapYear[j] : perennialProfile.standardYear[j];

            monthName = k.name;
            monthDay = i;
            monthPercentage = k.days;


        }


        /// <summary>
        /// Smoothly skips time.
        /// </summary> 
        public void TransitionTime(float timeToSkip, float time)
        {

            StartCoroutine(TransitionTime(currentTime, timeToSkip, time));

        }

        IEnumerator TransitionTime(float startDayPercentage, float timeToSkip, float time)
        {

            transitioningTime = true;
            float t = time;
            float targetTime = timeToSkip % 1;
            float targetDay = Mathf.Floor(timeToSkip);
            float transitionSpeed = timeToSkip / time;

            while (t > 0)
            {

                float div = 1 - (t / time);
                yield return new WaitForEndOfFrame();

                currentTime += Time.deltaTime * transitionSpeed;

                t -= Time.deltaTime;

            }

            transitioningTime = false;

        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CozyTimeModule))]
    [CanEditMultipleObjects]
    public class E_CozyTimeModule : E_CozyModule, IControlPanel
    {

        SerializedProperty currentTimePercent;
        SerializedProperty currentDay;
        SerializedProperty currentYear;
        SerializedProperty perennialProfile;
        public static bool isSelectionWindowOpen;
        public static bool isCurrentSettingsWindowOpen;
        public static bool isLengthWindowOpen;
        public static bool isMovementWindowOpen;
        CozyTimeModule timeModule;
        E_PerennialProfile timeEditor;
        PerennialProfile perennial;

        public override GUIContent GetGUIContent()
        {

            //Place your module's GUI content here.
            return new GUIContent("    Time", (Texture)Resources.Load("CozyCalendar"), "Setup time settings, simple calendars, and manage current settings.");

        }

        void OnEnable()
        {
            timeModule = (CozyTimeModule)target;
            currentTimePercent = serializedObject.FindProperty("m_DayPercentage");
            currentDay = serializedObject.FindProperty("currentDay");
            currentYear = serializedObject.FindProperty("currentYear");
            perennial = timeModule.perennialProfile;
            timeEditor = CreateEditor(perennial) as E_PerennialProfile;
            perennialProfile = serializedObject.FindProperty("perennialProfile");
        }

        public void SetTime(object time)
        {
            timeModule.currentTime = (float)time;
        }

        public override void OpenContextMenu(Vector2 pos)
        {

            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Set Time to Morning"), false, SetTime, 0.25f);
            menu.AddItem(new GUIContent("Set Time to Day"), false, SetTime, 0.5f);
            menu.AddItem(new GUIContent("Set Time to Evening"), false, SetTime, 0.75f);
            menu.AddItem(new GUIContent("Set Time to Night"), false, SetTime, 0f);
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Remove Module"), false, RemoveModule);
            menu.AddItem(new GUIContent("Reset"), false, ResetModule);
            menu.AddItem(new GUIContent("Edit Script"), false, EditScript);

            menu.ShowAsContext();

        }

        public override void OpenDocumentationURL()
        {
            Application.OpenURL("https://distant-lands.gitbook.io/cozy-stylized-weather-documentation/how-it-works/modules/time-module");
        }

        public void GetControlPanel()
        {
            EditorGUILayout.PropertyField(currentTimePercent, new GUIContent("Current Time"));
            EditorGUI.BeginChangeCheck();
            currentDay.intValue = EditorGUILayout.IntSlider(new GUIContent("Current Day"), currentDay.intValue, 0, timeModule.GetDaysPerYear());
            if (EditorGUI.EndChangeCheck() && timeModule.transit)
                timeModule.transit.GetModifiedDayPercent();
            EditorGUILayout.PropertyField(currentYear);
        }
        public override void GetReportsInformation()
        {

            EditorGUILayout.LabelField(GetGUIContent(), EditorStyles.toolbar);
            EditorGUILayout.HelpBox("Currently it is " + timeModule.currentTime.ToString() + " on " + timeModule.MonthTitle(timeModule.GetCurrentYearPercentage()) + ".", MessageType.None, true);

        }


        public override void DisplayInCozyWindow()
        {
            EditorGUI.indentLevel = 0;
            serializedObject.Update();
            bool tooltips = EditorPrefs.GetBool("CZY_Tooltips", true);

            isSelectionWindowOpen = EditorGUILayout.BeginFoldoutHeaderGroup(isSelectionWindowOpen, new GUIContent("    Selection Settings"), EditorUtilities.FoldoutStyle);

            if (isSelectionWindowOpen)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(perennialProfile);
                if (serializedObject.hasModifiedProperties)
                {
                    serializedObject.ApplyModifiedProperties();
                    perennial = timeModule.perennialProfile;
                    timeEditor = CreateEditor(perennial) as E_PerennialProfile;
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();


            isCurrentSettingsWindowOpen = EditorGUILayout.BeginFoldoutHeaderGroup(isCurrentSettingsWindowOpen, new GUIContent("    Current Settings"), EditorUtilities.FoldoutStyle);

            if (isCurrentSettingsWindowOpen)
            {
                EditorGUI.indentLevel++;
                if (tooltips)
                {
                    EditorGUILayout.HelpBox("You can also change the length of the year! The default profile uses 48 days in a year to create a shorter year to improve contrast.", MessageType.Info);
                    EditorGUILayout.HelpBox("Don't like the proportions of the current time system? Not to worry! Check out the 2400 tick perennial profile for a more realistic year!", MessageType.Info);
                }

                EditorGUILayout.PropertyField(currentTimePercent, new GUIContent("Current Time"));
                EditorGUI.BeginChangeCheck();
                currentDay.intValue = EditorGUILayout.IntSlider(new GUIContent("Current Day"), currentDay.intValue, 0, timeModule.GetDaysPerYear());
                if (EditorGUI.EndChangeCheck() && timeModule.transit)
                    timeModule.transit.GetModifiedDayPercent();
                EditorGUILayout.PropertyField(currentYear);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            timeEditor.OnStaticMeasureGUI(EditorUtilities.FoldoutStyle, ref isLengthWindowOpen, ref isMovementWindowOpen);

            serializedObject.ApplyModifiedProperties();

        }

    }
#endif
}