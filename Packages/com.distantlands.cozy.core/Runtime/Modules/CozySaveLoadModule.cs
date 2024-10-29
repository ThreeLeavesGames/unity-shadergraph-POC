//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DistantLands.Cozy
{
    public class CozySaveLoadModule : CozyModule
    {


        // Start is called before the first frame update
        void Awake()
        {

            if (!enabled)
                return;

            InitializeModule();

        }


        public void Save()
        {

            if (weatherSphere == null)
                InitializeModule();


            string weatherJSON = JsonUtility.ToJson(weatherSphere);
            Debug.Log(weatherJSON);
            PlayerPrefs.SetString("CZY_Properties", weatherJSON);

        }

        public void Load()
        {


            if (weatherSphere == null)
                InitializeModule();

            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString("CZY_Properties"), weatherSphere);

        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CozySaveLoadModule))]
    public class E_CozySaveLoad : E_CozyModule, IControlPanel
    {

        CozySaveLoadModule saveLoad;

        void OnEnable()
        {

            saveLoad = (CozySaveLoadModule)target;

        }

        public override GUIContent GetGUIContent()
        {

            return new GUIContent("    Save & Load", (Texture)Resources.Load("Save"), "Manage save and load commands within the COZY system.");

        }
        
        public override void OpenDocumentationURL()
        {
            Application.OpenURL("https://distant-lands.gitbook.io/cozy-stylized-weather-documentation/how-it-works/modules/save-and-load-module");
        }

        public override void OnInspectorGUI()
        {


        }

        public override void DisplayInCozyWindow()
        {
            EditorGUI.indentLevel = 0;
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Save"))
                saveLoad.Save();
            if (GUILayout.Button("Load"))
                saveLoad.Load();

            EditorGUILayout.EndHorizontal();

        }
        public void GetControlPanel()
        {
            if (GUILayout.Button("Save"))
                saveLoad.Save();
            if (GUILayout.Button("Load"))
                saveLoad.Load();
        }

    }
#endif
}