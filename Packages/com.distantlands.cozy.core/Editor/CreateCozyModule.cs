//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;
using System.Text;

namespace DistantLands.Cozy.EditorScripts
{
    public class CreateCozyModule : EditorWindow//, ICozyEditorMenuEntry
    {
        string moduleName = "NewCozyModule";
        string displayName = "New Cozy Module";
        string tooltip = "(optional)";
        string iconPath = "";
        bool isBiomeModule;
        [Flags]
        public enum UpdateMethods
        {
            OnFrameReset = 1,
            UpdateWeatherWeights = 2,
            UpdateFXWeights = 4,
            PropogateVariables = 8,
            CozyUpdateLoop = 16,

        }
        public UpdateMethods updateMethods;


        static string folderPath;


        [MenuItem("Tools/Cozy: Stylized Weather 3/Create Cozy Module")]
        private static void ShowWindow()
        {
            GetWindow<CreateCozyModule>("Create Cozy Module");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Create New COZY Module", E_CozyEditor.TitleStyle);
            moduleName = EditorGUILayout.TextField(new GUIContent("Script Name"), moduleName);
            if (moduleName.Contains(" ") || moduleName.Contains("/") || moduleName.Contains("."))
                EditorGUILayout.HelpBox("Name can only contain letters and underscores", MessageType.Error);
            EditorGUILayout.Space();
            displayName = EditorGUILayout.TextField(new GUIContent("Display Name"), displayName);
            tooltip = EditorGUILayout.TextField(new GUIContent("Tooltip"), tooltip);
            iconPath = EditorGUILayout.TextField(new GUIContent("Icon Path", "Must be in a Resources folder. See the Unity documentation for more information!"), iconPath);

            EditorGUILayout.Space();
            updateMethods = (UpdateMethods)EditorGUILayout.EnumFlagsField("Update Methods", updateMethods);
            isBiomeModule = EditorGUILayout.Toggle(new GUIContent("Is Biome Module"), isBiomeModule);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Generate Cozy Module"))
            {
                GenerateScript();
            }
        }



        private void GenerateScript()
        {
            string scriptName = moduleName;
            string scriptContent = GenerateScriptContent();

            folderPath = "Assets" + EditorUtility.SaveFolderPanel("Select a Folder", folderPath, "").Substring(Application.dataPath.Length);
            string filePath = Path.Combine(folderPath, scriptName + ".cs");

            // Ensure the folder exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Write the script content to the file
            File.WriteAllText(filePath, scriptContent);

            // Refresh the asset database to recognize the new script
            AssetDatabase.Refresh();
        }

        private string GenerateScriptContent()
        {
            StringBuilder scriptBuilder = new StringBuilder();

            scriptBuilder.AppendLine("using UnityEngine;");
            scriptBuilder.AppendLine("#if UNITY_EDITOR");
            scriptBuilder.AppendLine("using UnityEditor;");
            scriptBuilder.AppendLine("#endif");
            scriptBuilder.AppendLine("using System.Collections.Generic;");
            scriptBuilder.AppendLine("using System.Linq;");
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("namespace DistantLands.Cozy");
            scriptBuilder.AppendLine("{");
            scriptBuilder.AppendLine($"    public class {moduleName} : CozyModule{(isBiomeModule ? ", ICozyBiomeModule" : "")}");
            scriptBuilder.AppendLine("    {");
            if (isBiomeModule)
            {
                scriptBuilder.AppendLine($"        public List<{moduleName}> biomes = new List<{moduleName}>();");
                scriptBuilder.AppendLine("        public bool isBiomeModule { get; set; }");
                scriptBuilder.AppendLine("        public float weight = 1;");
            }
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("        //Place your variables here");
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("        //Initialize will be called along with the OnEnable method");
            scriptBuilder.AppendLine("        public override void InitializeModule()");
            scriptBuilder.AppendLine("        {");
            scriptBuilder.AppendLine("            base.InitializeModule();");
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("            //Place your initialization logic here!");
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("        }");
            scriptBuilder.AppendLine();
            if ((updateMethods & UpdateMethods.OnFrameReset) != 0)
            {
                scriptBuilder.AppendLine("        public override void FrameReset()");
                scriptBuilder.AppendLine("        {");
                scriptBuilder.AppendLine();
                scriptBuilder.AppendLine("        }");
                scriptBuilder.AppendLine();
            }
            if ((updateMethods & UpdateMethods.UpdateWeatherWeights) != 0)
            {
                scriptBuilder.AppendLine("        public override void UpdateWeatherWeights()");
                scriptBuilder.AppendLine("        {");
                scriptBuilder.AppendLine();
                scriptBuilder.AppendLine("        }");
                scriptBuilder.AppendLine();
            }
            if ((updateMethods & UpdateMethods.UpdateFXWeights) != 0)
            {
                scriptBuilder.AppendLine("        public override void UpdateFXWeights()");
                scriptBuilder.AppendLine("        {");
                scriptBuilder.AppendLine();
                scriptBuilder.AppendLine("        }");
                scriptBuilder.AppendLine();
            }
            if ((updateMethods & UpdateMethods.PropogateVariables) != 0)
            {
                scriptBuilder.AppendLine("        public override void PropogateVariables()");
                scriptBuilder.AppendLine("        {");
                scriptBuilder.AppendLine();
                scriptBuilder.AppendLine("        }");
                scriptBuilder.AppendLine();
            }
            if ((updateMethods & UpdateMethods.CozyUpdateLoop) != 0)
            {
                scriptBuilder.AppendLine("        public override void CozyUpdateLoop()");
                scriptBuilder.AppendLine("        {");
                scriptBuilder.AppendLine();
                scriptBuilder.AppendLine("        }");
            }
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("        //Deinitialize will be called along with the OnDisable method");
            scriptBuilder.AppendLine("        public override void DeinitializeModule()");
            scriptBuilder.AppendLine("        {");
            scriptBuilder.AppendLine("            base.DeinitializeModule();");
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("            //Place your deinitialization logic here!");
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("        }");
            scriptBuilder.AppendLine();
            if (isBiomeModule)
            {
                scriptBuilder.AppendLine("        public void AddBiome()");
                scriptBuilder.AppendLine("        {");
                scriptBuilder.AppendLine("            weatherSphere = CozyWeather.instance;");
                scriptBuilder.AppendLine($"            weatherSphere.GetModule<{moduleName}>().biomes = FindObjectsOfType<{moduleName}>().Where(x => x != weatherSphere.GetModule<{moduleName}>()).ToList();");
                scriptBuilder.AppendLine("        }");
                scriptBuilder.AppendLine();
                scriptBuilder.AppendLine("        public void RemoveBiome()");
                scriptBuilder.AppendLine("        {");
                scriptBuilder.AppendLine($"            weatherSphere.GetModule<{moduleName}>().biomes.Remove(this);");
                scriptBuilder.AppendLine("        }");
                scriptBuilder.AppendLine();
                scriptBuilder.AppendLine("        public void UpdateBiomeModule()");
                scriptBuilder.AppendLine("        {");
                scriptBuilder.AppendLine("            //Place your update logic for the biome here");
                scriptBuilder.AppendLine("        }");
                scriptBuilder.AppendLine();
                scriptBuilder.AppendLine("        //Checks if this module is added to the weather sphere");
                scriptBuilder.AppendLine("        public bool CheckBiome()");
                scriptBuilder.AppendLine("        {");
                scriptBuilder.AppendLine($"            if (!weatherSphere.GetModule<{moduleName}>())");
                scriptBuilder.AppendLine("            {");
                scriptBuilder.AppendLine("                Debug.LogError(\"Please add this module to the weather sphere before setting up your biome.\");");
                scriptBuilder.AppendLine("                return false;");
                scriptBuilder.AppendLine("            }");
                scriptBuilder.AppendLine("            return true;");
                scriptBuilder.AppendLine("        }");
                scriptBuilder.AppendLine();
                scriptBuilder.AppendLine("        public void ComputeBiomeWeights()");
                scriptBuilder.AppendLine("        {");
                scriptBuilder.AppendLine("            float totalSystemWeight = 0;");
                scriptBuilder.AppendLine("            biomes.RemoveAll(x => x == null);");
                scriptBuilder.AppendLine();
                scriptBuilder.AppendLine($"            foreach ({moduleName} biome in biomes)");
                scriptBuilder.AppendLine("            {");
                scriptBuilder.AppendLine("                if (biome != this)");
                scriptBuilder.AppendLine("                {");
                scriptBuilder.AppendLine("                    totalSystemWeight += biome.system.targetWeight;");
                scriptBuilder.AppendLine("                }");
                scriptBuilder.AppendLine("            }");
                scriptBuilder.AppendLine();
                scriptBuilder.AppendLine("            weight = Mathf.Clamp01(1 - (totalSystemWeight));");
                scriptBuilder.AppendLine("            totalSystemWeight += weight;");
                scriptBuilder.AppendLine();
                scriptBuilder.AppendLine($"            foreach ({moduleName} biome in biomes)");
                scriptBuilder.AppendLine("            {");
                scriptBuilder.AppendLine("                if (biome.system != this)");
                scriptBuilder.AppendLine("                    biome.weight = biome.system.targetWeight / (totalSystemWeight == 0 ? 1 : totalSystemWeight);");
                scriptBuilder.AppendLine("            }");
                scriptBuilder.AppendLine("        }");
            }
            scriptBuilder.AppendLine("    }");
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("#if UNITY_EDITOR");
            scriptBuilder.AppendLine($"[CustomEditor(typeof({moduleName}))]");
            scriptBuilder.AppendLine($"    public class E_{moduleName} : E_CozyModule");
            scriptBuilder.AppendLine("    {");
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("        public override GUIContent GetGUIContent()");
            scriptBuilder.AppendLine("        {");
            scriptBuilder.AppendLine($"            return new GUIContent(\"    {displayName}\", (Texture)Resources.Load(\"{iconPath}\"), \"{tooltip}\");");
            scriptBuilder.AppendLine("        }");
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("        public override void DisplayInCozyWindow()");
            scriptBuilder.AppendLine("        {");
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("            //Serialize your custom fields here!");
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("        }");
            scriptBuilder.AppendLine();
            scriptBuilder.AppendLine("    }");
            scriptBuilder.AppendLine("#endif");
            scriptBuilder.AppendLine("}");

            return scriptBuilder.ToString();

        }

    }
}