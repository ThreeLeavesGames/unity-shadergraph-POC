//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DistantLands.Cozy
{
    public class CozyControlPanelModule : CozyModule
    {



    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CozyControlPanelModule))]
    [CanEditMultipleObjects]
    public class E_CozyControlPanelModule : E_CozyModule
    {


        public override GUIContent GetGUIContent()
        {

            //Place your module's GUI content here.
            return new GUIContent("    Control Panel", (Texture)Resources.Load("Control Panel"), "Manage the most important functions of all modules in a unified editor for rapid prototyping.");

        }
        public override void OpenDocumentationURL()
        {
            Application.OpenURL("https://distant-lands.gitbook.io/cozy-stylized-weather-documentation/how-it-works/modules/control-panel-module");
        }
        public override void DisplayInCozyWindow()
        {
            EditorGUI.indentLevel = 0;
            foreach (E_CozyModule module in E_CozyWeather.editors)
            {
                module.serializedObject.Update();
                if (module is not IControlPanel)
                    continue;

                module.DisplayToolar(false);
                ((IControlPanel)module).GetControlPanel();
                module.serializedObject.ApplyModifiedProperties();
                EditorGUILayout.Space();
            }
        }

    }

    public interface IControlPanel
    {
        public abstract void GetControlPanel();

    }
#endif
}