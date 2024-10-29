//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DistantLands.Cozy
{
    public class ExampleModule : CozyModule
    {



    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ExampleModule))]
    [CanEditMultipleObjects]
    public class E_ExampleModule : E_CozyModule
    {


        public override GUIContent GetGUIContent()
        {

            //Place your module's GUI content here.
            return new GUIContent("    Example Module", (Texture)Resources.Load("MoreOptions"), "Empty module to be used as a base for custom modules.");

        }


        public override void GetDebugInformation()
        {



        }

        public override void GetReportsInformation()
        {



        }
        
        public override void OpenContextMenu(Vector2 pos)
        {

            //Use this to add new actions to the editor's context menu if needed for your module

            GenericMenu menu = new GenericMenu();
            // menu.AddSeparator("");
            menu.AddItem(new GUIContent("Remove Module"), false, RemoveModule);
            menu.AddItem(new GUIContent("Reset"), false, ResetModule);
            menu.AddItem(new GUIContent("Edit Script"), false, EditScript);
            
            menu.ShowAsContext();

        }

        public override void DisplayInCozyWindow()
        {
            serializedObject.Update();

            //Place custom inspector code here.

            serializedObject.ApplyModifiedProperties();

        }

    }
#endif
}