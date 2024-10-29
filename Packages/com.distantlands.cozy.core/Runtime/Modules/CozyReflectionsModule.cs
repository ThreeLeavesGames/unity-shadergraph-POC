//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if COZY_URP
using UnityEngine.Rendering.Universal;
#endif

namespace DistantLands.Cozy
{
    [ExecuteAlways]
    public class CozyReflectionsModule : CozyModule
    {

        public enum UpdateFrequency { everyFrame, onAwake, viaScripting }
        public UpdateFrequency updateFrequency;
        public Cubemap reflectionCubemap;
        public Camera reflectionCamera;
        [Tooltip("How many frames should pass before the cubemap renders again? A value of 0 renders every frame and a value of 30 renders once every 30 frames.")]
        [Range(0, 30)]
        public int framesBetweenRenders = 10;
        [Tooltip("What layers should be rendered into the skybox reflections?.")]
        public LayerMask layerMask = 2048;
        private int framesLeft;
#if COZY_URP
        public int rendererOverride;
#endif

        public override void InitializeModule()
        {

            base.InitializeModule();
            reflectionCubemap = Resources.Load("Materials/Reflection Cubemap") as Cubemap;
            RenderSettings.customReflection = reflectionCubemap;
            RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
            weatherSphere.fogMesh.gameObject.layer = ToLayer(layerMask);
            weatherSphere.skyMesh.gameObject.layer = ToLayer(layerMask);
            weatherSphere.cloudMesh.gameObject.layer = ToLayer(layerMask);

            if (updateFrequency == UpdateFrequency.onAwake)
            {
                RenderReflections();
            }
        }

        public override void CozyUpdateLoop()
        {
            if (weatherSphere == null)
            {
                base.InitializeModule();
            }

            if (weatherSphere.freezeUpdateInEditMode && !Application.isPlaying)
            {
                return;
            }

            if (updateFrequency == UpdateFrequency.everyFrame)
            {
                if (framesLeft < 0)
                {

                    RenderReflections();
                    framesLeft = framesBetweenRenders;

                }
                else
                {
                    framesLeft--;
                }
            }
        }

        public int ToLayer(LayerMask mask)
        {
            int value = mask.value;
            if (value == 0)
            {
                return 0;
            }
            for (int l = 1; l < 32; l++)
            {
                if ((value & (1 << l)) != 0)
                {
                    return l;
                }
            }
            return -1;
        }

        public override void DeinitializeModule()
        {
            base.DeinitializeModule();

            if (reflectionCamera)
            {
                DestroyImmediate(reflectionCamera.gameObject);
            }

            RenderSettings.customReflection = null;

        }

        public void RenderReflections()
        {

            if (!weatherSphere.cozyCamera)
            {
                Debug.LogError("COZY Reflections requires the cozy camera to be set in the settings tab!");
                return;
            }

            if (reflectionCamera == null)
            {
                SetupCamera();
            }

            reflectionCamera.enabled = true;
            reflectionCamera.transform.position = transform.position;
            reflectionCamera.nearClipPlane = weatherSphere.cozyCamera.nearClipPlane;
            reflectionCamera.farClipPlane = weatherSphere.cozyCamera.farClipPlane;
            reflectionCamera.cullingMask = layerMask;
#if COZY_URP
            if (reflectionCamera.GetComponent<UniversalAdditionalCameraData>())
                reflectionCamera.GetComponent<UniversalAdditionalCameraData>().SetRenderer(rendererOverride);
#endif
            reflectionCamera.RenderToCubemap(reflectionCubemap);
            reflectionCamera.enabled = false;

        }

        public void SetupCamera()
        {


            GameObject i = new GameObject             
            {
                name = "COZY Reflection Camera",
                hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.HideInHierarchy
            };

            reflectionCamera = i.AddComponent<Camera>();
            reflectionCamera.depth = -50;
            reflectionCamera.enabled = false;

        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CozyReflectionsModule))]
    [CanEditMultipleObjects]
    public class E_CozyReflect : E_CozyModule
    {
        private CozyReflectionsModule reflect;

        public override GUIContent GetGUIContent()
        {

            //Place your module's GUI content here.
            return new GUIContent("    Reflections", (Texture)Resources.Load("Reflections"), "Sets up a cubemap for reflections with COZY.");

        }

        public override void OpenDocumentationURL()
        {
            Application.OpenURL("https://distant-lands.gitbook.io/cozy-stylized-weather-documentation/how-it-works/modules/reflections-module");
        }

        public override void DisplayInCozyWindow()
        {
            EditorGUI.indentLevel = 0;
            if (reflect == null)
            {
                reflect = (CozyReflectionsModule)target;
            }

            serializedObject.Update();


            EditorGUILayout.PropertyField(serializedObject.FindProperty("updateFrequency"));
            if (reflect.updateFrequency == CozyReflectionsModule.UpdateFrequency.everyFrame)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("framesBetweenRenders"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("reflectionCubemap"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("layerMask"));
#if COZY_URP
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rendererOverride"));
#endif



            serializedObject.ApplyModifiedProperties();

        }

    }
#endif
}