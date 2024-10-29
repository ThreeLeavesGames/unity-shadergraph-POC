//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using UnityEngine.Rendering;
using System.IO;
using System;
using System.Linq;
using System.Reflection;


#if COZY_URP
using UnityEngine.Rendering.Universal;
#endif

namespace DistantLands.Cozy.EditorScripts
{
    public class E_CozyEditor : EditorWindow
    {
        public Texture titleWindow;
        public Vector2 scrollPos;
        public Vector2 menuSectionsPosition;
        public CozyWeather headUnit;
        public Editor editor;
        public delegate void OnGUIDelegate();
        public class MenuSection
        {
            public GUIContent content;
            public OnGUIDelegate drawFunction;

            public MenuSection(GUIContent windowTitle, OnGUIDelegate drawFunction)
            {
                content = windowTitle;
                this.drawFunction = drawFunction;
            }
        }
        static int currentMenuSection;
        List<MenuSection> menuSections;
        public static GUIStyle ButtonStyle => new GUIStyle(new GUIStyle("LargeButton"))
        {
            fontStyle = FontStyle.Bold,
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter,
            fixedHeight = 40,
            stretchWidth = true,

        };
        public static GUIStyle TitleStyle => new GUIStyle(new GUIStyle("AM MixerHeader"))
        {
            fixedHeight = 40,
            wordWrap = true
        };
        public static GUIStyle CheckStyle => new GUIStyle(new GUIStyle("label"))
        {
            fixedHeight = 25
        };
        public static GUIStyle CorrectStyle => new GUIStyle(new GUIStyle("sv_label_3"))
        {
            fixedHeight = 25,
            alignment = TextAnchor.MiddleCenter
        };
        public static GUIStyle WarningStyle => new GUIStyle(new GUIStyle("sv_label_5"))
        {
            fixedHeight = 25,
            alignment = TextAnchor.MiddleCenter
        };
        public static GUIStyle ExtensionTitleStyle => new GUIStyle(new GUIStyle("label"))
        {
            fontStyle = FontStyle.Bold,
            fontSize = 14
        };
        public static GUIStyle ExtensionDescStyle => new GUIStyle(new GUIStyle("label"))
        {
            alignment = TextAnchor.UpperLeft,
            wordWrap = true
        };
        public static GUIStyle DescriptionStyle => new GUIStyle(new GUIStyle("label"))
        {
            alignment = TextAnchor.UpperLeft,
            wordWrap = true,
            stretchHeight = true,
            fontSize = 12,
            margin = new RectOffset(2, 2, 5, 5)
        };


        [MenuItem("Tools/Cozy: Stylized Weather 3/Open Cozy Editor", false, 1500)]
        [MenuItem("Window/Cozy: Stylized Weather 3/Open Cozy Editor", false, 1005)]
        public static void Init()
        {

            E_CozyEditor window = GetWindow<E_CozyEditor>(false, "COZY: Weather", true);
            window.minSize = new Vector2(600, 500);
            window.Show();

        }

        private void OnEnable()
        {
            UpdateMenu();
        }

        public void UpdateMenu()
        {
            menuSections = new List<MenuSection>
            {
                new MenuSection(new GUIContent("Home"), HomeGUI),
                new MenuSection(new GUIContent("Scene Setup"), SceneSetupGUI),
                new MenuSection(new GUIContent("Current Instance"), CurrentInstanceGUI),
                // new MenuSection(new GUIContent("Global Settings"), SettingsGUI),
                // new MenuSection(new GUIContent("Integration"), IntegrationGUI),
                new MenuSection(new GUIContent("Extension Modules"), ExtensionGUI),
            };

            var types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(s => s.GetTypes())
                        .Where(p => typeof(ICozyEditorMenuEntry).IsAssignableFrom(p) && p.IsClass);

            foreach (var type in types)
            {
                var instance = Activator.CreateInstance(type);
                var methodInfo = type.GetMethod("GetMenuSection", BindingFlags.Public | BindingFlags.Instance);

                if (methodInfo != null)
                {
                    menuSections.Add((MenuSection)methodInfo.Invoke(instance, null));
                }
            }
        }

        private void OnGUI()
        {
            if (titleWindow == null)
                titleWindow = (Texture2D)Resources.Load("Promo/Clouds Title_Moment");
                
            EditorGUI.indentLevel = 0;
            GUI.DrawTexture(new Rect(0, 0, position.width, position.width * 1 / 6), titleWindow);
            EditorGUILayout.Space(position.width * 1 / 6);
            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            {
                menuSectionsPosition = EditorGUILayout.BeginScrollView(menuSectionsPosition, new GUIStyle("PreferencesSectionBox"), GUILayout.Width(150));
                {
                    for (int i = 0; i < menuSections.Count; i++)
                    {
                        var iteration = menuSections[i];

                        Rect elementRect = GUILayoutUtility.GetRect(iteration.content, new GUIStyle("PreferencesSection"), GUILayout.ExpandWidth(true));

                        if (i == currentMenuSection && Event.current.type == EventType.Repaint)
                        {
                            new GUIStyle("OL SelectedRow").Draw(elementRect, false, false, i == currentMenuSection, false);
                        }

                        if (GUI.Toggle(elementRect, currentMenuSection == i, iteration.content, new GUIStyle("PreferencesSection")))
                        {
                            currentMenuSection = i;
                        }
                    }
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space(5f);

                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.Space(5f);
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                    menuSections[currentMenuSection].drawFunction();
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

        }
        
        private void CurrentInstanceGUI()
        {
            if (headUnit == null)
            {
                if (CozyWeather.instance)
                {
                    headUnit = CozyWeather.instance;
                    editor = Editor.CreateEditor(headUnit);
                }
                else
                {
                    EditorGUILayout.HelpBox("There is no instance of COZY in your scene! You will need to setup COZY using the wizard before editing", MessageType.Warning);
                    if (GUILayout.Button("Setup COZY"))
                        E_CozyMenuItems.CozySetupScene();
                    return;
                }
            }

            if (editor)
                editor.OnInspectorGUI();
            else if (headUnit)
                editor = Editor.CreateEditor(headUnit);

        }
        
        private void HomeGUI()
        {

            EditorGUILayout.LabelField("Thank You for the Purchase!", TitleStyle);
            EditorGUILayout.LabelField("I hope that this asset serves you well and you find it useful for your projects.", DescriptionStyle);
            EditorGUILayout.LabelField("If you have any issues or requests please feel free to reach out to me. Best of luck with your projects and enjoy the skies!", DescriptionStyle);
            EditorGUILayout.LabelField("Cheers!", DescriptionStyle);
            EditorGUILayout.LabelField("Keller Bowman | Distant Lands!", DescriptionStyle);

            EditorGUILayout.Space(30);

            EditorGUILayout.LabelField("Project Setup", TitleStyle);
            RunProjectChecks();
            EditorGUILayout.Space(30);

            EditorGUILayout.LabelField("Manage Render Pipelines", TitleStyle);
            DrawIntegrations(new GUIContent("Built-in Render Pipeline"),
            "Import this package for BiRP support",
            "Import for BiRP.unitypackage");
            DrawIntegrations(new GUIContent("Universal Render Pipeline - Unity 2021+ / Unity 2023+"),
            "Import this package for URP support",
            "Import for URP.unitypackage");
            DrawIntegrations(new GUIContent("Universal Render Pipeline - Unity 2023+ / Unity 6"),
            "Import this package for URP support",
            "Import for URP Unity 6+.unitypackage");
            DrawIntegrations(new GUIContent("High-Definition Render Pipeline"),
            "Import this package for HDRP support",
            "Import for HDRP.unitypackage");
            EditorGUILayout.Space(30);

            EditorGUILayout.LabelField("Support Links", TitleStyle);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Documentation", ButtonStyle))
                {
                    E_CozyMenuItems.Docs();
                }
                if (GUILayout.Button("Discord", ButtonStyle))
                {
                    E_CozyMenuItems.Discord();
                }
                if (GUILayout.Button("FAQs", ButtonStyle))
                {
                    Application.OpenURL("https://distant-lands.gitbook.io/cozy-stylized-weather-documentation/appendix/frequently-asked-questions-faqs");
                }
            }
            EditorGUILayout.EndHorizontal();


        }
        
        private void SceneSetupGUI()
        {

            EditorGUILayout.LabelField(" Add COZY to Your Scene", TitleStyle);
            if (!CozyWeather.instance)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Setup Scene", ButtonStyle))
                    {
                        E_CozyMenuItems.CozySetupScene();
                    }
                    if (GUILayout.Button("Setup Scene (No Modules)", ButtonStyle))
                    {
                        E_CozyMenuItems.CozySetupSceneSimple();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("COZY is successfully setup in this scene!", MessageType.Info);
            }
            EditorGUILayout.Space(40);

            EditorGUILayout.LabelField(" Helper Objects", TitleStyle);
            if (GUILayout.Button("Create Global Biome", ButtonStyle))
            {
                Camera view = SceneView.lastActiveSceneView.camera;

                GameObject i = new GameObject();
                i.name = "Cozy Biome";
                i.AddComponent<CozyBiome>();
                i.transform.position = (view.transform.forward * 5) + view.transform.position;

                Undo.RegisterCreatedObjectUndo(i, "Create Cozy Biome");
                Selection.activeGameObject = i;
            }
            if (GUILayout.Button("Create Local Biome", ButtonStyle))
            {
                Camera view = SceneView.lastActiveSceneView.camera;

                GameObject i = new GameObject();
                i.name = "Cozy Biome";
                CozyBiome biome = i.AddComponent<CozyBiome>();
                BoxCollider collider = i.AddComponent<BoxCollider>();
                collider.size = Vector3.one * 15;
                biome.trigger = collider;
                biome.trigger.isTrigger = true;
                biome.mode = CozyBiome.BiomeMode.local;
                i.transform.position = (view.transform.forward * 5) + view.transform.position;

                Undo.RegisterCreatedObjectUndo(i, "Create Cozy Biome");
                Selection.activeGameObject = i;
            }
            if (GUILayout.Button("Add FX Block Zone", ButtonStyle))
            {
                Camera view = SceneView.lastActiveSceneView.camera;

                GameObject i = new GameObject();
                i.name = "Cozy FX Block Zone";
                i.AddComponent<BoxCollider>().isTrigger = true;
                i.tag = "FX Block Zone";
                i.transform.position = (view.transform.forward * 5) + view.transform.position;

                Undo.RegisterCreatedObjectUndo(i, "Create Cozy FX Block Zone");
                Selection.activeGameObject = i;
            }
            if (GUILayout.Button("Add Fog Culling Zone", ButtonStyle))
            {
                Camera view = SceneView.lastActiveSceneView.camera;

                GameObject i = GameObject.CreatePrimitive(PrimitiveType.Cube);
                i.name = "Cozy Fog Cull Zone";
                i.GetComponent<MeshRenderer>().material = (Material)Resources.Load("Materials/Fog Culling Zone");
                i.transform.position = (view.transform.forward * 5) + view.transform.position;

                Undo.RegisterCreatedObjectUndo(i, "Create Cozy Fog Culling Zone");
                Selection.activeGameObject = i;
            }
        }

        private void IntegrationGUI()
        {



        }
        private void SettingsGUI()
        {



        }
        private void ExtensionGUI()
        {

            EditorGUILayout.LabelField(" Extension Modules", TitleStyle);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("COZY's modular architecture allows you to customize your project to your teams needs. One way that we support indie developers is through keeping our prices low by outsourcing additional modules. You only need to pay for the modules that you will put to use in your project!", DescriptionStyle);
            EditorGUILayout.LabelField("Extend your project with our extension modules:", DescriptionStyle);
            EditorGUILayout.Space();
            EditorGUI.indentLevel--;

            DrawExtension(new GUIContent("BLOCKS: Extended Atmosphere Control Module", (Texture)Resources.Load("Blocks")),
            "Bring new control and ease of use to your COZY: Weather system with BLOCKS! Easily evaluate colors, set up day gradients, and manage time-of-day colors with the new custom module.",
            "https://assetstore.unity.com/packages/tools/utilities/blocks-atmosphere-control-module-for-cozy-3-238051");
            DrawExtension(new GUIContent("PLUME: Volumetric Clouds Module", (Texture)Resources.Load("Cloud")),
            "Fill your skies with particle-based, fully volumetric clouds with PLUME: Volumetric Clouds for COZY 3!",
            "https://assetstore.unity.com/packages/tools/particles-effects/plume-volumetric-clouds-for-cozy-3-243905");
            DrawExtension(new GUIContent("HABITS: Extended Calendar Module", (Texture)Resources.Load("Habits")),
            "Process dates, events, and planning within the COZY ecosystem more easily with HABITS. Gain complete control over the calendar with an improved events system, graphical scheduling, and much more!",
            "https://assetstore.unity.com/packages/tools/utilities/habits-extended-calendar-module-for-cozy-3-259983");
            DrawExtension(new GUIContent("LINK: Multiplayer Module", (Texture)Resources.Load("Link")),
            "Connect your players' worlds with LINK: Multiplayer Module for COZY! Quickly integrate your project with popular multiplayer solutions and dispense your beautiful weather!",
            "https://assetstore.unity.com/packages/tools/network/link-multiplayer-module-for-cozy-3-238669");
            EditorGUI.BeginDisabledGroup(true);
            DrawExtension(new GUIContent("CULTIVATE: Farming Module", (Texture)Resources.Load("Ecosystem")),
            "Coming Soon!",
            "");
            DrawExtension(new GUIContent("ECLIPSE: Sun Occlusion Module", (Texture)Resources.Load("CozyEclipse")),
            "Coming Soon!",
            "");
            DrawExtension(new GUIContent("RADAR: Worldspace Forecast Module", (Texture)Resources.Load("CozyRadar")),
            "Coming Soon!",
            "");
            DrawExtension(new GUIContent("ReSOUND: Adaptive Soundtrack Module", (Texture)Resources.Load("ReSound Icon")),
            "Coming Soon!",
            "");
            DrawExtension(new GUIContent("CATACLYSM: Natural Disaster Module", (Texture)Resources.Load("Tornado")),
            "Coming Soon!",
            "");
            EditorGUI.EndDisabledGroup();

        }

        private void RunProjectChecks()
        {

            Rect currentRect;
            Rect labelRect;
            Rect fixRect;

            string currentRP = "Built-in Render Pipeline";
#if COZY_URP
            currentRP = "Universal Render Pipeline";
#elif COZY_HDRP
            currentRP = "High Definition Render Pipeline";
#endif

            EditorGUILayout.LabelField("COZY is currently setup for the " + currentRP);

            currentRect = EditorGUILayout.GetControlRect(false, 25);
            labelRect = new Rect(currentRect.x, currentRect.y, currentRect.width * 2 / 3, currentRect.height);
            fixRect = new Rect(currentRect.x + currentRect.width * 2 / 3, currentRect.y, currentRect.width / 3, currentRect.height);
            EditorGUI.LabelField(labelRect, "Colorspace is " + PlayerSettings.colorSpace + ".", CheckStyle);
            if (PlayerSettings.colorSpace != ColorSpace.Linear)
            {
                if (GUI.Button(fixRect, "Fix Now", WarningStyle))
                {
                    PlayerSettings.colorSpace = ColorSpace.Linear;
                }
            }
            else
            {
                EditorGUI.LabelField(fixRect, "Good", CorrectStyle);
            }
#if COZY_URP
            currentRect = EditorGUILayout.GetControlRect(false, 25);
            labelRect = new Rect(currentRect.x, currentRect.y, currentRect.width * 2 / 3, currentRect.height);
            fixRect = new Rect(currentRect.x + currentRect.width * 2 / 3, currentRect.y, currentRect.width / 3, currentRect.height);
            bool checkDepthTexture = IsDepthTextureOptionDisabledAnywhere(out string pipelineName);
            EditorGUI.LabelField(labelRect, "Depth texture is " + (checkDepthTexture ? $"disabled in the {pipelineName} renderer." : "enabled."), CheckStyle);
            if (checkDepthTexture)
            {
                if (GUI.Button(fixRect, "Fix Now", WarningStyle))
                {
                    for (int i = 0; i < GraphicsSettings.allConfiguredRenderPipelines.Length; i++)
                    {
                        if (GraphicsSettings.allConfiguredRenderPipelines[i].GetType() != typeof(UniversalRenderPipelineAsset)) continue;
                        UniversalRenderPipelineAsset pipeline = (UniversalRenderPipelineAsset)GraphicsSettings.allConfiguredRenderPipelines[i];
                        pipeline.supportsCameraDepthTexture = true;
                    }
                }
            }
            else
            {
                EditorGUI.LabelField(fixRect, "Good", CorrectStyle);
            }

            currentRect = EditorGUILayout.GetControlRect(false, 25);
            labelRect = new Rect(currentRect.x, currentRect.y, currentRect.width * 2 / 3, currentRect.height);
            fixRect = new Rect(currentRect.x + currentRect.width * 2 / 3, currentRect.y, currentRect.width / 3, currentRect.height);
            bool checkOpaqueTexture = IsOpaqueTextureOptionDisabledAnywhere(out pipelineName);
            EditorGUI.LabelField(labelRect, "Opaque texture is " + (checkOpaqueTexture ? $"disabled in the {pipelineName} renderer." : "enabled."), CheckStyle);
            if (checkOpaqueTexture)
            {
                if (GUI.Button(fixRect, "Fix Now", WarningStyle))
                {
                    for (int i = 0; i < GraphicsSettings.allConfiguredRenderPipelines.Length; i++)
                    {
                        if (GraphicsSettings.allConfiguredRenderPipelines[i].GetType() != typeof(UniversalRenderPipelineAsset)) continue;
                        UniversalRenderPipelineAsset pipeline = (UniversalRenderPipelineAsset)GraphicsSettings.allConfiguredRenderPipelines[i];
                        pipeline.supportsCameraOpaqueTexture = true;
                    }
                }
            }
            else
            {
                EditorGUI.LabelField(fixRect, "Good", CorrectStyle);
            }

            currentRect = EditorGUILayout.GetControlRect(false, 25);
            labelRect = new Rect(currentRect.x, currentRect.y, currentRect.width * 2 / 3, currentRect.height);
            fixRect = new Rect(currentRect.x + currentRect.width * 2 / 3, currentRect.y, currentRect.width / 3, currentRect.height);
            bool checkOpaqueDownsampling = IsOpaqueDownsamplingNotSetToNoneAnywhere(out pipelineName);
            EditorGUI.LabelField(labelRect, "Opaque downsampling is " + (checkOpaqueDownsampling ? $"not set to none in the {pipelineName} renderer." : "set to the right value."), CheckStyle);
            if (checkOpaqueDownsampling)
            {
                EditorGUI.LabelField(fixRect, "Set to none in your URP settings", WarningStyle);
            }
            else
            {
                EditorGUI.LabelField(fixRect, "Good", CorrectStyle);
            }

            currentRect = EditorGUILayout.GetControlRect(false, 25);
            labelRect = new Rect(currentRect.x, currentRect.y, currentRect.width * 2 / 3, currentRect.height);
            fixRect = new Rect(currentRect.x + currentRect.width * 2 / 3, currentRect.y, currentRect.width / 3, currentRect.height);
            bool checkHDR = IsHDRDisabled(out pipelineName);
            EditorGUI.LabelField(labelRect, "HDR is " + (checkHDR ? $"disabled in the {pipelineName} renderer." : "enabled."), CheckStyle);
            if (checkHDR)
            {
                if (GUI.Button(fixRect, "Fix Now", WarningStyle))
                {
                    for (int i = 0; i < GraphicsSettings.allConfiguredRenderPipelines.Length; i++)
                    {
                        if (GraphicsSettings.allConfiguredRenderPipelines[i].GetType() != typeof(UniversalRenderPipelineAsset)) continue;
                        UniversalRenderPipelineAsset pipeline = (UniversalRenderPipelineAsset)GraphicsSettings.allConfiguredRenderPipelines[i];
                        pipeline.supportsHDR = true;
                    }
                }
            }
            else
            {
                EditorGUI.LabelField(fixRect, "Good", CorrectStyle);
            }

#endif
        }

        public static List<T> GetAssets<T>(string[] _foldersToSearch, string _filter) where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets(_filter, _foldersToSearch);
            List<T> a = new List<T>();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a.Add(AssetDatabase.LoadAssetAtPath<T>(path));
            }
            return a;
        }

        public static PackageInfo GetPackage(string packageID, bool throwError)
        {
            SearchRequest request = Client.Search(packageID);
            while (request.Status == StatusCode.InProgress) { }

            if (request.Status == StatusCode.Failure && throwError)
            {
                Debug.LogError("Failed to retrieve package from Package Manager...");
                return null;
            }

            return request.Result[0];
        }

        public static bool IsPackageInstalled(string packageID)
        {
            string manifestPath = Application.dataPath + "/../Packages/manifest.json";

            if (File.Exists(manifestPath))
            {
                string manifestContents = File.ReadAllText(manifestPath);

                return manifestContents.Contains(packageID);
            }
            else
            {
                Debug.LogError("Unable to find the manifest file.");
                return false;
            }
        }

        void DrawExtension(GUIContent assetName, string assetDescription, string url)
        {
            Rect currentRect = EditorGUILayout.GetControlRect(false, 90);
            Rect labelRect = new Rect(currentRect.x + 75, currentRect.y, (currentRect.width * 2 / 3), 25);
            Rect descriptionRect = new Rect(currentRect.x + 75, currentRect.y + 30, currentRect.width - 80, 65);
            Rect iconRect = new Rect(currentRect.x + 10, currentRect.y + 10, 50, 50);
            Rect viewRect = new Rect(currentRect.x + 75 + currentRect.width * 2 / 3, currentRect.y, (currentRect.width / 3) - 75, 20);

            EditorGUI.LabelField(labelRect, assetName.text, ExtensionTitleStyle);
            GUI.DrawTexture(iconRect, assetName.image);
            EditorGUI.LabelField(descriptionRect, assetDescription, ExtensionDescStyle);
            if (GUI.Button(viewRect, "Asset Store"))
            {
                Application.OpenURL(url);
            }
            EditorGUILayout.Space();

        }
        void DrawIntegrations(GUIContent assetName, string assetDescription, string url)
        {
            Rect currentRect = EditorGUILayout.GetControlRect(false, 60);
            Rect labelRect = new Rect(currentRect.x, currentRect.y, currentRect.width * 2 / 3, 25);
            Rect descriptionRect = new Rect(currentRect.x, currentRect.y + 30, currentRect.width * 2 / 3, 35);
            Rect importButtonRect = new Rect(currentRect.x + currentRect.width * 2 / 3, currentRect.y + 15, (currentRect.width / 3), 20);

            EditorGUI.LabelField(labelRect, assetName.text, ExtensionTitleStyle);
            EditorGUI.LabelField(descriptionRect, assetDescription, ExtensionDescStyle);
            if (GUI.Button(importButtonRect, "Import"))
            {
                string relativePath = AssetInformation.INTEGRATION_PATH + url;
                string absolutePath = Path.GetFullPath(relativePath);
                Application.OpenURL(absolutePath);
                Debug.Log($"Finished set up COZY v{AssetInformation.SEM_VERSION} for the {assetName.text}.");
                PlayerPrefs.SetString("CZY_SemVersion", AssetInformation.SEM_VERSION);

            }
            EditorGUILayout.Space();

        }
#if COZY_URP
        public static bool IsDepthTextureOptionDisabledAnywhere(out string problemPipelineAsset)
        {
            problemPipelineAsset = "";

            for (int i = 0; i < GraphicsSettings.allConfiguredRenderPipelines.Length; i++)
            {
                if (GraphicsSettings.allConfiguredRenderPipelines[i].GetType() != typeof(UniversalRenderPipelineAsset)) continue;

                UniversalRenderPipelineAsset pipeline = (UniversalRenderPipelineAsset)GraphicsSettings.allConfiguredRenderPipelines[i];

                if (pipeline.supportsCameraDepthTexture == false)
                {
                    problemPipelineAsset = pipeline.name;
                    return true;
                }
            }

            return false;
        }
        public static bool IsOpaqueTextureOptionDisabledAnywhere(out string problemPipelineAsset)
        {
            problemPipelineAsset = null;

            for (int i = 0; i < GraphicsSettings.allConfiguredRenderPipelines.Length; i++)
            {
                if (GraphicsSettings.allConfiguredRenderPipelines[i].GetType() != typeof(UniversalRenderPipelineAsset)) continue;

                UniversalRenderPipelineAsset pipeline = (UniversalRenderPipelineAsset)GraphicsSettings.allConfiguredRenderPipelines[i];

                if (pipeline.supportsCameraOpaqueTexture == false)
                {
                    problemPipelineAsset = pipeline.name;
                    return true;
                }
            }

            return false;
        }
        public static bool IsOpaqueDownsamplingNotSetToNoneAnywhere(out string problemPipelineAsset)
        {
            problemPipelineAsset = null;

            for (int i = 0; i < GraphicsSettings.allConfiguredRenderPipelines.Length; i++)
            {
                if (GraphicsSettings.allConfiguredRenderPipelines[i].GetType() != typeof(UniversalRenderPipelineAsset)) continue;

                UniversalRenderPipelineAsset pipeline = (UniversalRenderPipelineAsset)GraphicsSettings.allConfiguredRenderPipelines[i];

                if (!(pipeline.opaqueDownsampling == Downsampling.None))
                {
                    problemPipelineAsset = pipeline.name;
                    return true;
                }
            }

            return false;
        }
        public static bool IsHDRDisabled(out string problemPipelineAsset)
        {
            problemPipelineAsset = "";

            for (int i = 0; i < GraphicsSettings.allConfiguredRenderPipelines.Length; i++)
            {
                if (GraphicsSettings.allConfiguredRenderPipelines[i].GetType() != typeof(UniversalRenderPipelineAsset)) continue;

                UniversalRenderPipelineAsset pipeline = (UniversalRenderPipelineAsset)GraphicsSettings.allConfiguredRenderPipelines[i];
                
                if (!(pipeline.supportsHDR == true))
                {
                    problemPipelineAsset = pipeline.name;
                    return true;
                }
            }

            return false;
        }
#endif
    }

    public interface ICozyEditorMenuEntry
    {
        public E_CozyEditor.MenuSection GetMenuSection();
    }

}