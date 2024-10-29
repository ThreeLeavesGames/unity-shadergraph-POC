//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif



namespace DistantLands.Cozy.Data
{

    [System.Serializable]
    [CreateAssetMenu(menuName = "Distant Lands/Cozy/Atmosphere Profile", order = 361)]
    public class AtmosphereProfile : ScriptableObject
    {

        public bool win1;
        public bool win2;
        public bool win3;
        public bool win4;

        [Tooltip("Sets the color of the zenith (or top) of the skybox at a certain time. Starts and ends at midnight.")]
        [CozyPropertyType(true)]
        public CustomProperty skyZenithColor;
        [Tooltip("Sets the color of the horizon (or middle) of the skybox at a certain time. Starts and ends at midnight.")]
        [CozyPropertyType(true)]
        public CustomProperty skyHorizonColor;

        [Tooltip("Sets the main color of the clouds at a certain time. Starts and ends at midnight.")]
        [CozyPropertyType(true)]
        public CustomProperty cloudColor;
        [Tooltip("Sets the highlight color of the clouds at a certain time. Starts and ends at midnight.")]
        [CozyPropertyType(true)]
        public CustomProperty cloudHighlightColor;
        [Tooltip("Sets the color of the high altitude clouds at a certain time. Starts and ends at midnight.")]
        [CozyPropertyType(true)]
        public CustomProperty highAltitudeCloudColor;
        [Tooltip("Sets the color of the sun light source at a certain time. Starts and ends at midnight.")]
        [CozyPropertyType(true)]
        public CustomProperty sunlightColor;
        [Tooltip("Sets the color of the moon light source at a certain time. Starts and ends at midnight.")]
        [CozyPropertyType(true)]
        public CustomProperty moonlightColor;
        [Tooltip("Sets the color of the star particle FX and textures at a certain time. Starts and ends at midnight.")]
        [CozyPropertyType(true)]
        public CustomProperty starColor;
        [Tooltip("Sets the color of the zenith (or top) of the ambient scene lighting at a certain time. Starts and ends at midnight.")]
        [CozyPropertyType(true)]
        public CustomProperty ambientLightHorizonColor;
        [Tooltip("Sets the color of the horizon (or middle) of the ambient scene lighting at a certain time. Starts and ends at midnight.")]
        [CozyPropertyType(true)]
        public CustomProperty ambientLightZenithColor;
        [Tooltip("Multiplies the ambient light intensity.")]
        [CozyPropertyType(false, 0, 4)]
        public CustomProperty ambientLightMultiplier;
        [Tooltip("Sets the intensity of the galaxy effects at a certain time. Starts and ends at midnight.")]
        [CozyPropertyType(false, 0, 1)]
        public CustomProperty galaxyIntensity;


        [CozyPropertyType(true)]
        [Tooltip("Sets the fog color from 0m away from the camera to fog start 1.")]
        public CustomProperty fogColor1;
        [CozyPropertyType(true)]
        [Tooltip("Sets the fog color from fog start 1 to fog start 2.")]
        public CustomProperty fogColor2;
        [Tooltip("Sets the fog color from fog start 2 to fog start 3.")]
        [CozyPropertyType(true)]
        public CustomProperty fogColor3;
        [Tooltip("Sets the fog color from fog start 3 to fog start 4.")]
        [CozyPropertyType(true)]
        public CustomProperty fogColor4;
        [Tooltip("Sets the fog color from fog start 4 to fog start 5.")]
        [CozyPropertyType(true)]
        public CustomProperty fogColor5;
        [CozyPropertyType(true)]
        [Tooltip("Sets the color of the fog flare.")]
        public CustomProperty fogFlareColor;
        [CozyPropertyType(true)]
        [Tooltip("Sets the color of the moon flare for the fog.")]
        public CustomProperty fogMoonFlareColor;
        [CozyPropertyType(false, 0, 1)]
        [Tooltip("Sets the smoothness of the fog.")]
        public CustomProperty fogSmoothness;

        public Vector3 fogVariationDirection;
        [CozyPropertyType(false, 0, 30)]
        [Tooltip("Sets the variation scale of the fog.")]
        public CustomProperty fogVariationScale;
        [CozyPropertyType(false, 0, 1)]
        [Tooltip("Sets the variation amount.")]
        public CustomProperty fogVariationAmount;
        [Tooltip("Sets the variation distance of the fog.")]
        [CozyPropertyType(false, 0, 200)]
        public CustomProperty fogVariationDistance;


        [CozyPropertyType(false, 0, 1)]
        public CustomProperty heightFogIntensity;

        [CozyPropertyType(false, 100, 1000)]
        public CustomProperty heightFogVariationScale;

        [CozyPropertyType(false, 0, 50)]
        public CustomProperty heightFogVariationAmount;

        [CozyPropertyType(false)]
        public CustomProperty fogBase;

        [CozyPropertyType(false, 0, 500)]
        public CustomProperty heightFogTransition;

        [CozyPropertyType(false, 0, 5000)]
        public CustomProperty heightFogDistance;

        [CozyPropertyType(true)]
        public CustomProperty heightFogColor;




        [CozyPropertyType(false, 0, 1)]
        [Tooltip("Controls the exponent used to modulate from the horizon color to the zenith color of the sky.")]
        public CustomProperty gradientExponent;
        [CozyPropertyType(false, 0, 5)]
        [Tooltip("Sets the size of the visual sun in the sky.")]
        public CustomProperty sunSize;
        [Tooltip("Sets the world space direction of the sun in degrees.")]
        [CozyPropertyType(false, 0, 360)]
        public CustomProperty sunDirection;
        [Tooltip("Sets the roll value of the sun's rotation. Allows the sun to be slightly off from directly overhead at noon.")]
        [CozyPropertyType(false, 0, 90)]
        public CustomProperty sunPitch;
        [Tooltip("Sets the color of the visual sun in the sky.")]
        [CozyPropertyType(true)]
        public CustomProperty sunColor;
        [Tooltip("Sets the color of the visual moon in the sky (only impacts the global shader variable for the stylized moon material).")]
        [CozyPropertyType(true)]
        public CustomProperty moonColor;


        [CozyPropertyType(false, 0, 1)]
        [Tooltip("Sets the falloff of the halo around the visual sun.")]
        public CustomProperty sunFalloff;
        [CozyPropertyType(true)]
        [Tooltip("Sets the color of the halo around the visual sun.")]
        public CustomProperty sunFlareColor;
        [CozyPropertyType(false, 0, 1)]
        [Tooltip("Sets the falloff of the halo around the main moon.")]
        public CustomProperty moonFalloff;
        [CozyPropertyType(true)]
        [Tooltip("Sets the color of the halo around the main moon.")]
        public CustomProperty moonFlareColor;
        [CozyPropertyType(true)]
        [Tooltip("Sets the color of the first galaxy algorithm.")]
        public CustomProperty galaxy1Color;
        [CozyPropertyType(true)]
        [Tooltip("Sets the color of the second galaxy algorithm.")]
        public CustomProperty galaxy2Color;
        [CozyPropertyType(true)]
        [Tooltip("Sets the color of the third galaxy algorithm.")]
        public CustomProperty galaxy3Color;
        [CozyPropertyType(true)]
        [Tooltip("Sets the color of the light columns around the horizon.")]
        public CustomProperty lightScatteringColor;
        [Tooltip("Should COZY use a rainbow?")]
        public bool useRainbow = true;
        [Tooltip("Sets the position of the rainbow in the sky.")]
        [CozyPropertyType(false, 0, 100)]
        public CustomProperty rainbowPosition;
        [Tooltip("Sets the width of the rainbow in the sky.")]
        [CozyPropertyType(false, 0, 50)]
        public CustomProperty rainbowWidth;


        [CozyPropertyType(false, 0, 5)]
        [Tooltip("Multiplies the world space distance before entering the fog algorithm. Use this for simple density changes.")]
        public CustomProperty fogDensityMultiplier;

        [Tooltip("Sets the distance at which the first fog color fades into the second fog color.")]
        public float fogStart1;
        public float fogStart2;
        public float fogStart3;
        public float fogStart4;
        [CozyPropertyType(false, 0, 2)]
        public CustomProperty fogHeight;
        [CozyPropertyType(false, 0, 2)]
        public CustomProperty fogLightFlareIntensity;
        [CozyPropertyType(false, 0, 40)]
        public CustomProperty fogLightFlareFalloff;
        [CozyPropertyType(false, 0, 10)]
        [Tooltip("Sets the height divisor for the fog flare. High values sit the flare closer to the horizon, small values extend the flare into the sky.")]
        public CustomProperty fogLightFlareSquish;

        [CozyPropertyType(true)]
        public CustomProperty cloudMoonColor;
        [CozyPropertyType(false, 0, 50)]
        public CustomProperty cloudSunHighlightFalloff;
        [CozyPropertyType(false, 0, 50)]
        public CustomProperty cloudMoonHighlightFalloff;
        [CozyPropertyType(false, 0, 10)]
        public CustomProperty cloudWindSpeed;
        [CozyPropertyType(false, 0, 1)]
        public CustomProperty clippingThreshold;
        [CozyPropertyType(false, 2, 60)]
        public CustomProperty cloudMainScale;
        [CozyPropertyType(false, 0.2f, 10)]
        public CustomProperty cloudDetailScale;
        [CozyPropertyType(false, 0, 30)]
        public CustomProperty cloudDetailAmount;
        [CozyPropertyType(false, 0.1f, 3)]
        public CustomProperty acScale;
        [CozyPropertyType(false, 0, 3)]
        public CustomProperty cirroMoveSpeed;
        [CozyPropertyType(false, 0, 3)]
        public CustomProperty cirrusMoveSpeed;
        [CozyPropertyType(false, 0, 3)]
        public CustomProperty chemtrailsMoveSpeed;




        public Texture cloudTexture;
        public Texture chemtrailsTexture;
        public Texture cirrusCloudTexture;
        public Texture cirrostratusCloudTexture;
        public Texture altocumulusCloudTexture;
        public Texture starMap;
        public Texture galaxyMap;
        public Texture galaxyStarMap;
        public Texture galaxyVariationMap;
        public Texture lightScatteringMap;

        public Texture partlyCloudyLuxuryClouds;
        public Texture mostlyCloudyLuxuryClouds;
        public Texture overcastLuxuryClouds;
        public Texture lowBorderLuxuryClouds;
        public Texture highBorderLuxuryClouds;
        public Texture lowNimbusLuxuryClouds;
        public Texture midNimbusLuxuryClouds;
        public Texture highNimbusLuxuryClouds;
        public Texture luxuryVariation;

        [CozyPropertyType(true)]
        public CustomProperty cloudTextureColor;
        [CozyPropertyType(false, 0, 10)]
        public CustomProperty cloudCohesion;
        [CozyPropertyType(false, 0, 1)]
        public CustomProperty spherize;
        [CozyPropertyType(false, 0, 10)]
        public CustomProperty shadowDistance;
        [CozyPropertyType(false, 0, 4)]
        public CustomProperty cloudThickness;
        [CozyPropertyType(false, 0, 3)]
        public CustomProperty textureAmount;
        public Vector3 texturePanDirection;
#if COZY_URP || COZY_HDRP
        [System.Serializable]
        public class SRPFlare
        {
            public LensFlareDataSRP flare;
            public float intensity = 1;
            public float scale = 1;
            public AnimationCurve screenAttenuation;
            public bool useOcclusion = true;
            public float occlusionRadius = 0.5f;
            public bool allowOffscreen = true;
        }
        public SRPFlare sunFlare;
        public SRPFlare moonFlare;
#endif

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AtmosphereProfile))]
    [CanEditMultipleObjects]
    public class E_AtmosphereProfile : Editor
    {


        Vector2 scrollPos;

        public int windowNum;
        public bool tooltips;

        public CozyWeather defaultWeather;


        Color proCol = (Color)new Color32(50, 50, 50, 255);
        Color unityCol = (Color)new Color32(194, 194, 194, 255);

        void OnEnable()
        {
            if (CozyWeather.instance)
                defaultWeather = CozyWeather.instance;

        }

        public override void OnInspectorGUI()
        {
            tooltips = EditorPrefs.GetBool("CZY_Tooltips", true);

            if (defaultWeather)
                OnInspectorGUIInline(defaultWeather);
            else
                EditorGUILayout.HelpBox("To edit the atmosphere profile make sure that your scene is properly setup with a COZY system!", MessageType.Warning);

        }

        public void OnInspectorGUIInline(CozyWeather cozyWeather)
        {
            serializedObject.Update();
            tooltips = EditorPrefs.GetBool("CZY_Tooltips", true);

            E_CozyAtmosphereModule.atmosphereOptionsWindow = EditorGUILayout.BeginFoldoutHeaderGroup(E_CozyAtmosphereModule.atmosphereOptionsWindow,
                new GUIContent("    Atmosphere & Lighting", "Skydome, fog, and lighting settings."), EditorUtilities.FoldoutStyle);

            if (E_CozyAtmosphereModule.atmosphereOptionsWindow)
            {

                DrawAtmosphereTab(cozyWeather);

            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            E_CozyAtmosphereModule.fogOptionsWindow = EditorGUILayout.BeginFoldoutHeaderGroup(E_CozyAtmosphereModule.fogOptionsWindow,
                            new GUIContent("    Fog", "Manage fog settings."), EditorUtilities.FoldoutStyle);

            if (E_CozyAtmosphereModule.fogOptionsWindow)
            {

                DrawFogTab(cozyWeather);

            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            E_CozyAtmosphereModule.cloudsOptionsWindow = EditorGUILayout.BeginFoldoutHeaderGroup(E_CozyAtmosphereModule.cloudsOptionsWindow,
                            new GUIContent("    Clouds", "Cloud color, generation, and variation settings."), EditorUtilities.FoldoutStyle);

            if (E_CozyAtmosphereModule.cloudsOptionsWindow)
            {

                DrawCloudsTab(cozyWeather);

            }

            EditorGUILayout.EndFoldoutHeaderGroup();


            E_CozyAtmosphereModule.celestialsOptionsWindow = EditorGUILayout.BeginFoldoutHeaderGroup(E_CozyAtmosphereModule.celestialsOptionsWindow,
                            new GUIContent("    Celestials & VFX", "Sun, moon, and light FX settings."), EditorUtilities.FoldoutStyle);

            if (E_CozyAtmosphereModule.celestialsOptionsWindow)
            {

                DrawCelestialsTab(cozyWeather);

            }

            EditorGUILayout.EndFoldoutHeaderGroup();


            serializedObject.ApplyModifiedProperties();


        }



        void DrawAtmosphereTab(CozyWeather cozyWeather)
        {

            GUIStyle labelStyle = new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                fontStyle = FontStyle.Bold
            };

            if (tooltips)
                EditorGUILayout.HelpBox("Interpolate controls change the value depending on the time of day. These range from 00:00 to 23:59, which means that morning is about 25% through the curve, midday 50%, evening 75%, etc. \n \n Constant controls set the value to a single value that remains constant regardless of the time of day.", MessageType.Info);

            EditorGUILayout.LabelField(" Skydome Settings", labelStyle);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("skyZenithColor"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skyHorizonColor"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gradientExponent"), false);

            EditorGUILayout.Space(5);
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField(" Lighting Settings", labelStyle);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("sunlightColor"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("moonlightColor"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ambientLightHorizonColor"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ambientLightZenithColor"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ambientLightMultiplier"), false);
            EditorGUI.indentLevel--;


        }


        void DrawFogTab(CozyWeather cozyWeather)
        {

            GUIStyle labelStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            labelStyle.fontStyle = FontStyle.Bold;

            if (tooltips)
                EditorGUILayout.HelpBox("Interpolate controls change the value depending on the time of day. These range from 00:00 to 23:59, which means that morning is about 25% through the curve, midday 50%, evening 75%, etc. \n \n Constant controls set the value to a single value that remains constant regardless of the time of day.", MessageType.Info);


            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(" Colors", labelStyle);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogColor1"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogColor2"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogColor3"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogColor4"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogColor5"), false);
            EditorGUILayout.Space(5);

            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField(" Placement", labelStyle);
            EditorGUI.indentLevel++;
            float fogStart1 = serializedObject.FindProperty("fogStart1").floatValue;
            float fogStart2 = serializedObject.FindProperty("fogStart2").floatValue;
            float fogStart3 = serializedObject.FindProperty("fogStart3").floatValue;
            float fogStart4 = serializedObject.FindProperty("fogStart4").floatValue;

            fogStart1 = Mathf.Clamp(EditorGUILayout.Slider("Fog Start 2", fogStart1, 0, 50), 0, fogStart2 - 0.1f);
            fogStart2 = Mathf.Clamp(EditorGUILayout.Slider("Fog Start 3", fogStart2, 0, 50), fogStart1 + 0.1f, fogStart3 - 0.1f);
            fogStart3 = Mathf.Clamp(EditorGUILayout.Slider("Fog Start 4", fogStart3, 0, 50), fogStart2 + 0.1f, fogStart4 - 0.1f);
            fogStart4 = Mathf.Clamp(EditorGUILayout.Slider("Fog Start 5", fogStart4, 0, 50), fogStart3 + 0.1f, 50);

            serializedObject.FindProperty("fogStart1").floatValue = fogStart1;
            serializedObject.FindProperty("fogStart2").floatValue = fogStart2;
            serializedObject.FindProperty("fogStart3").floatValue = fogStart3;
            serializedObject.FindProperty("fogStart4").floatValue = fogStart4;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogHeight"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogSmoothness"), false);

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogDensityMultiplier"), false);


            EditorGUILayout.Space(5);
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField(" Flares", labelStyle);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogFlareColor"), new GUIContent("Light Flare Color",
                "Sets the color of the fog for a false \"light flare\" around the main sun directional light."), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogMoonFlareColor"), new GUIContent("Moon Flare Color",
                "Sets the color of the fog for a false \"light flare\" around the main moon directional light."), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogLightFlareIntensity"), new GUIContent("Light Flare Intensity",
                "Modulates the brightness of the light flare."), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogLightFlareFalloff"), new GUIContent("Light Flare Falloff",
                "Sets the falloff speed for the light flare."), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogLightFlareSquish"), new GUIContent("Light Flare Squish",
                "Sets the height divisor for the fog flare. High values sit the flare closer to the horizon, small values extend the flare into the sky."), false);

            EditorGUILayout.Space(5);
            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField(" Variation", labelStyle);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogVariationDirection"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogVariationScale"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogVariationAmount"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fogVariationDistance"), false);
            EditorGUILayout.Space();
            EditorGUI.indentLevel--;
            if (cozyWeather.fogStyle == CozyWeather.FogStyle.heightFog)
            {
                EditorGUILayout.LabelField(" Height Fog", labelStyle);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("heightFogColor"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("heightFogIntensity"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fogBase"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("heightFogTransition"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("heightFogDistance"), false);
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("heightFogVariationScale"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("heightFogVariationAmount"), false);
                EditorGUILayout.Space(5);
                EditorGUI.indentLevel--;
            }

        }

        void DrawCloudsTab(CozyWeather cozyWeather)
        {

            if (tooltips)
                EditorGUILayout.HelpBox("Interpolate controls change the value depending on the time of day. These range from 00:00 to 23:59, which means that morning is about 25% through the curve, midday 50%, evening 75%, etc. \n \n Constant controls set the value to a single value that remains constant regardless of the time of day.", MessageType.Info);


            GUIStyle labelStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            labelStyle.fontStyle = FontStyle.Bold;

            EditorGUILayout.LabelField(" Color Settings", labelStyle);
            EditorGUI.indentLevel++;


            EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudColor"), new GUIContent("Cloud Color", "The main color of the unlit clouds."), false);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("highAltitudeCloudColor"), new GUIContent("High Altitude Color", "The main color multiplier of the high altitude clouds. The cloud types affected are the cirrostratus and the altocumulus types."), false);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudHighlightColor"), new GUIContent("Sun Highlight Color", "The color multiplier for the clouds in a \"dot\" around the sun."), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudSunHighlightFalloff"), new GUIContent("Sun Highlight Falloff", "The falloff for the \"dot\" around the sun."), false);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudMoonColor"), new GUIContent("Moon Highlight Color", "The color multiplier for the clouds in a \"dot\" around the moon."), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudMoonHighlightFalloff"), new GUIContent("Moon Highlight Falloff", "The falloff for the \"dot\" around the moon."), false);


            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);
            if (cozyWeather.cloudStyle != CozyWeather.CloudStyle.singleTexture)
                EditorGUILayout.LabelField(" Generation Settings", labelStyle);

            EditorGUI.indentLevel++;
            if (!(cozyWeather.cloudStyle == CozyWeather.CloudStyle.singleTexture || cozyWeather.cloudStyle == CozyWeather.CloudStyle.luxury))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudWindSpeed"), new GUIContent("Wind Speed", "The speed at which the cloud generation will progress."), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("clippingThreshold"), new GUIContent("Clipping Threshold", "The alpha that the clouds will clip to full alpha at. Default is 0.5"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudMainScale"), new GUIContent("Main Scale", "The scale of the main perlin noise for the cumulus cloud type."), false);
                if (cozyWeather.cloudStyle != CozyWeather.CloudStyle.ghibliDesktop && cozyWeather.cloudStyle != CozyWeather.CloudStyle.ghibliMobile)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudDetailScale"), new GUIContent("Detail Scale", "The scale of the secondary voronoi noise functions for the cumulus cloud type."), false);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudDetailAmount"), new GUIContent("Detail Amount", "The multiplier for the secondary voronoi noise functions for the cumulus cloud type. Lower values give more cohesive cloud types."), false);
                }
                EditorGUILayout.Space(10);
                if (cozyWeather.cloudStyle == CozyWeather.CloudStyle.paintedSkies || cozyWeather.cloudStyle == CozyWeather.CloudStyle.soft)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudThickness"), new GUIContent("Cloud Thickness"), false);
                }
                if (cozyWeather.cloudStyle == CozyWeather.CloudStyle.cozyDesktop || cozyWeather.cloudStyle == CozyWeather.CloudStyle.paintedSkies || cozyWeather.cloudStyle == CozyWeather.CloudStyle.soft)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("acScale"), new GUIContent("Altocumulus Scale"), false);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("cirroMoveSpeed"), new GUIContent("Cirrostratus Movement Speed"), false);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("cirrusMoveSpeed"), new GUIContent("Cirrus Movement Speed"), false);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("chemtrailsMoveSpeed"), new GUIContent("Chemtrails Movement Speed"), false);
                }
            }

            if (cozyWeather.cloudStyle == CozyWeather.CloudStyle.ghibliDesktop || cozyWeather.cloudStyle == CozyWeather.CloudStyle.ghibliMobile)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudCohesion"), new GUIContent("Cloud Cohesion"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("spherize"), new GUIContent("Sphere Distortion"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("shadowDistance"), new GUIContent("Shadow Distance"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudThickness"), new GUIContent("Cloud Thickness"), false);
            }
            if (cozyWeather.cloudStyle == CozyWeather.CloudStyle.cozyDesktop || cozyWeather.cloudStyle == CozyWeather.CloudStyle.paintedSkies || cozyWeather.cloudStyle == CozyWeather.CloudStyle.soft || cozyWeather.cloudStyle == CozyWeather.CloudStyle.singleTexture)
            {
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField(" Texture Settings", labelStyle);
                EditorGUI.indentLevel++;
            }
            if (cozyWeather.cloudStyle == CozyWeather.CloudStyle.luxury)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudWindSpeed"), new GUIContent("Wind Speed", "The speed at which the cloud generation will progress."), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudMainScale"), new GUIContent("Main Scale", "The scale of the main perlin noise for the cumulus cloud type."), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("acScale"), new GUIContent("Altocumulus Scale"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cirroMoveSpeed"), new GUIContent("Cirrostratus Movement Speed"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cirrusMoveSpeed"), new GUIContent("Cirrus Movement Speed"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("chemtrailsMoveSpeed"), new GUIContent("Chemtrails Movement Speed"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("luxuryVariation"), false);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("partlyCloudyLuxuryClouds"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("mostlyCloudyLuxuryClouds"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("overcastLuxuryClouds"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lowBorderLuxuryClouds"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("highBorderLuxuryClouds"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lowNimbusLuxuryClouds"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("midNimbusLuxuryClouds"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("highNimbusLuxuryClouds"), false);
            }
            if (cozyWeather.cloudStyle == CozyWeather.CloudStyle.cozyDesktop || cozyWeather.cloudStyle == CozyWeather.CloudStyle.luxury || cozyWeather.cloudStyle == CozyWeather.CloudStyle.paintedSkies || cozyWeather.cloudStyle == CozyWeather.CloudStyle.soft)

            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("chemtrailsTexture"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cirrusCloudTexture"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cirrostratusCloudTexture"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("altocumulusCloudTexture"), false);
            }

            if (cozyWeather.cloudStyle == CozyWeather.CloudStyle.singleTexture)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("clippingThreshold"), new GUIContent("Clipping Threshold", "The alpha that the clouds will clip to full alpha at. Default is 0.5"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudTexture"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("texturePanDirection"), new GUIContent("Cloud Texture Pan Direction"), false);
            }
            if (cozyWeather.cloudStyle == CozyWeather.CloudStyle.paintedSkies)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudTexture"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cloudTextureColor"), new GUIContent("Texture Color Multiplier"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("textureAmount"), new GUIContent("Texture Amount"), false);
            }

            if (!cozyWeather.weatherModule)
            {

                EditorGUI.indentLevel--;
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField(" Current Settings", labelStyle);
                EditorGUI.indentLevel++;
                cozyWeather.cumulus = EditorGUILayout.Slider("Cumulus", cozyWeather.cumulus, 0, 2);
                cozyWeather.altocumulus = EditorGUILayout.Slider("Altocumulus", cozyWeather.altocumulus, 0, 2);
                cozyWeather.chemtrails = EditorGUILayout.Slider("Chemtrails", cozyWeather.chemtrails, 0, 2);
                cozyWeather.cirrostratus = EditorGUILayout.Slider("Cirrostratus", cozyWeather.cirrostratus, 0, 2);
                cozyWeather.cirrus = EditorGUILayout.Slider("Cirrus", cozyWeather.cirrus, 0, 2);
                cozyWeather.nimbus = EditorGUILayout.Slider("Nimbus", cozyWeather.nimbus, 0, 2);
                EditorGUI.indentLevel++;
                cozyWeather.nimbusVariation = EditorGUILayout.Slider("Variation", cozyWeather.nimbusVariation, 0, 1);
                cozyWeather.nimbusHeightEffect = EditorGUILayout.Slider("Height Effect", cozyWeather.nimbusHeightEffect, 0, 1);
                EditorGUI.indentLevel--;
                cozyWeather.borderHeight = EditorGUILayout.Slider("Border", cozyWeather.borderHeight, 0, 1);
                EditorGUI.indentLevel++;
                cozyWeather.borderVariation = EditorGUILayout.Slider("Variation", cozyWeather.borderVariation, 0, 1);
                cozyWeather.borderEffect = EditorGUILayout.Slider("Effect", cozyWeather.borderEffect, -1, 1);
                EditorGUI.indentLevel--;

            }


            EditorGUI.indentLevel--;


        }

        void DrawCelestialsTab(CozyWeather cozyWeather)
        {

            if (tooltips)
                EditorGUILayout.HelpBox("Interpolate controls change the value depending on the time of day. These range from 00:00 to 23:59, which means that morning is about 25% through the curve, midday 50%, evening 75%, etc. \n \n Constant controls set the value to a single value that remains constant regardless of the time of day.", MessageType.Info);

            GUIStyle labelStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            labelStyle.fontStyle = FontStyle.Bold;

            EditorGUILayout.LabelField(" Sun Settings", labelStyle);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sunColor"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sunSize"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sunDirection"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sunPitch"), false);
            if (!cozyWeather.timeModule)
                cozyWeather.sunAngle = EditorGUILayout.Slider("Sun Angle", cozyWeather.sunAngle, 0, 1);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sunFalloff"), new GUIContent("Sun Halo Falloff"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sunFlareColor"), new GUIContent("Sun Halo Color"), false);
#if COZY_URP || COZY_HDRP
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sunFlare"), true);
#endif
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(16);

            EditorGUILayout.LabelField(" Moon Settings", labelStyle);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("moonColor"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("moonFalloff"), new GUIContent("Moon Halo Falloff"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("moonFlareColor"), new GUIContent("Moon Halo Color"), false);
#if COZY_URP || COZY_HDRP
            EditorGUILayout.PropertyField(serializedObject.FindProperty("moonFlare"), true);
#endif
            EditorGUI.indentLevel--;



            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField(" VFX", labelStyle);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("starColor"), false);
            if (cozyWeather.skyStyle == CozyWeather.SkyStyle.desktop)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("starMap"), false);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("galaxyIntensity"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("galaxy1Color"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("galaxy2Color"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("galaxy3Color"), false);
            if (cozyWeather.skyStyle == CozyWeather.SkyStyle.desktop)
            {

                EditorGUILayout.PropertyField(serializedObject.FindProperty("galaxyMap"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("galaxyStarMap"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("galaxyVariationMap"), false);
            }
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lightScatteringColor"), false);
            if (cozyWeather.skyStyle == CozyWeather.SkyStyle.desktop)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lightScatteringMap"), false);
            }
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useRainbow"), false);
            EditorGUI.BeginDisabledGroup(!serializedObject.FindProperty("useRainbow").boolValue);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rainbowPosition"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rainbowWidth"), false);
            EditorGUI.EndDisabledGroup();

            EditorGUI.indentLevel--;


        }
    }
#endif
}