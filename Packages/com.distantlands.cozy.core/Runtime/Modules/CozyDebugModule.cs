//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DistantLands.Cozy
{
    [ExecuteAlways]
    public class CozyDebugModule : CozyModule
    {


        // Start is called before the first frame update
        void Awake()
        {

            if (!enabled)
                return;

        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CozyDebugModule))]
    public class E_CozyDebugModule : E_CozyModule
    {

        CozyDebugModule t;

        void OnEnable()
        {

            t = (CozyDebugModule)target;

        }

        public override GUIContent GetGUIContent()
        {

            return new GUIContent("    Debug", (Texture)Resources.Load("Debug"), "Aids in debugging and testing the COZY system.");

        }

        public override void OnInspectorGUI()
        {


        }
        public override void OpenDocumentationURL()
        {
            Application.OpenURL("https://distant-lands.gitbook.io/cozy-stylized-weather-documentation/how-it-works/modules/debug-module");
        }

        public override void DisplayInCozyWindow()
        {
            EditorGUI.indentLevel = 0;

            foreach (E_CozyModule module in E_CozyWeather.editors)
            {
                module.GetDebugInformation();
            }


        }

    }
#endif
}
