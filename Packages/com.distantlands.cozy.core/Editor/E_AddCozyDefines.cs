//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DistantLands.Cozy.EditorScripts
{

    [InitializeOnLoad]
    public class E_AddCozyDefines : Editor
    {

        /// <summary>
        /// Symbols that will be added to the editor
        /// </summary>
        public static readonly string[] Symbols = new string[] {
        "COZY_WEATHER",
        "COZY_3+"
    };

        /// <summary>
        /// Add define symbols as soon as Unity gets done compiling.
        /// </summary>
        static E_AddCozyDefines()
        {

            if (PlayerPrefs.GetInt("CZY_AddDefines", 1) == 1)
            {

                string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                List<string> allDefines = definesString.Split(';').ToList();
                allDefines.AddRange(Symbols.Except(allDefines));


                if (E_CozyEditor.IsPackageInstalled("com.unity.render-pipelines.universal"))
                {
                    if (!allDefines.Contains("COZY_URP"))
                        allDefines.Add("COZY_URP");
                }
                else if (E_CozyEditor.IsPackageInstalled("com.unity.render-pipelines.high-definition"))
                {
                    if (!allDefines.Contains("COZY_HDRP"))
                        allDefines.Add("COZY_HDRP");
                }

                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    EditorUserBuildSettings.selectedBuildTargetGroup,
                    string.Join(";", allDefines.ToArray()));
            }

            string curVersion = AssetInformation.SEM_VERSION;

            if (PlayerPrefs.GetString("CZY_SemVersion", "") != curVersion)
            {
                Debug.Log("NEW COZY VERSION DETECTED. Please import the appropriate render pipeline setup via the COZY Hub window.");
            }

            
            if (!(PlayerPrefs.GetInt("CZY_Started", 0) == 1) || PlayerPrefs.GetInt("CZY_AlwaysShow", 0) == 1)
            {
                E_CozyEditor.Init();
                PlayerPrefs.SetInt("CZY_Started", 1);
            }

        }
    }
}
