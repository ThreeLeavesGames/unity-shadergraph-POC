//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if BUTO
using OccaSoftware.Buto.Runtime;
#endif

namespace DistantLands.Cozy
{
    [ExecuteAlways]
    public class CozyButoModule : CozyModule
    {

#if BUTO

        [SerializeField]
        private ButoVolumetricFog fog;
        [SerializeField]
        private VolumeProfile volumeProfile;
        [Range(0, 2)] public float fogBrightnessMultiplier;
        [Range(0, 2)] public float fogDensityMultiplier;

        void Awake()
        {
            TryFindFog();
        }

        void Update()
        {

            if (weatherSphere == null)
                base.InitializeModule();

            if (volumeProfile == null)
                fog = null;

            if (weatherSphere.freezeUpdateInEditMode && !Application.isPlaying)
                return;

            if (fog)
            {

                fog.colorInfluence.Override(1);
                fog.litColor.Override(weatherSphere.fogColor5 * fogBrightnessMultiplier);
                fog.shadowedColor.Override(weatherSphere.fogColor5 * 0.5f * fogBrightnessMultiplier);
                fog.fogDensity.Override(fogDensityMultiplier * 10 * weatherSphere.fogDensity);

            }
            else
            {
                TryFindFog();
            }

        }

        void TryFindFog()
        {

            if (volumeProfile)
            {

                foreach (VolumeComponent component in volumeProfile.components)
                {

                    if (component is ButoVolumetricFog)
                    {
                        fog = (ButoVolumetricFog)component;
                        return;
                    }
                }
            }
            else
            {
                foreach (Volume vol in FindObjectsOfType<Volume>())
                {
                    foreach (VolumeComponent component in vol.profile.components)
                    {

                        if (component is ButoVolumetricFog)
                        {
                            fog = (ButoVolumetricFog)component;
                            volumeProfile = vol.profile;
                            return;
                        }
                    }
                }
            }
        }
#endif

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CozyButoModule))]
    [CanEditMultipleObjects]
    public class E_CozyButoModule : E_CozyModule
    {


        public override GUIContent GetGUIContent()
        {

            //Place your module's GUI content here.
            return new GUIContent("    Buto", (Texture)Resources.Load("Occa"), "Control Buto Volumetric Fog within the COZY system.");

        }

        public override void OpenDocumentationURL()
        {
            Application.OpenURL("https://distant-lands.gitbook.io/cozy-stylized-weather-documentation/how-it-works/modules/buto-module");
        }
        public override void DisplayInCozyWindow()
        {
            EditorGUI.indentLevel = 0;
            serializedObject.Update();

            EditorGUI.indentLevel++;

            EditorGUILayout.HelpBox("Please make sure the \"Import for Buto Integration\" unity package is installed for the shaders to properly recompile.", MessageType.Info);
            
#if BUTO
            if (serializedObject.FindProperty("fog").objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Could not find any instance of Buto in your scene! You will have to set the profile manually in the module settings.", MessageType.Warning);
                
            }
            EditorGUILayout.Space(20);
            if (serializedObject.FindProperty("volumeProfile").objectReferenceValue == null)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("volumeProfile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogBrightnessMultiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogDensityMultiplier"));
            EditorGUILayout.Space(20);
#else
            EditorGUILayout.HelpBox("Buto Volumetric Fog is not currently in this project! Please make sure that it has been properly downloaded before using this module.", MessageType.Warning);

            
            EditorGUI.indentLevel--;
#endif

            serializedObject.ApplyModifiedProperties();

        }

    }
#endif
}