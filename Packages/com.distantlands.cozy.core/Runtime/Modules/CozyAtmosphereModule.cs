//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
using DistantLands.Cozy.Data;

namespace DistantLands.Cozy
{
    [ExecuteAlways]
    public class CozyAtmosphereModule : CozyModule, ICozyBiomeModule
    {
        public AtmosphereProfile atmosphereProfile;
        public bool transitioningAtmosphere = false;
        public List<CozyAtmosphereModule> biomes = new List<CozyAtmosphereModule>();
        public bool isBiomeModule { get; set; }
        float weight;
        public override void InitializeModule()
        {
            isBiomeModule = GetComponent<CozyBiome>();

            if (isBiomeModule)
            {
                AddBiome();
                return;
            }
            base.InitializeModule();
            weatherSphere.atmosphereModule = this;
            biomes = FindObjectsOfType<CozyAtmosphereModule>().Where(x => x != this).ToList();

        }

        public override void PropogateVariables()
        {

            if (atmosphereProfile == null)
            {
                Debug.LogWarning("Cozy Weather requires an active weather profile, an active perennial profile and an active atmosphere profile to function properly.\nPlease ensure that the active CozyWeather script contains all necessary profile references.");
                return;
            }

            SetAtmosphereVariables();
        }


        void LateUpdate()
        {
            if (!isBiomeModule)
            {
                ComputeBiomeWeights();
                weatherSphere.UpdateShaderVariables();
            }
        }

        /// <summary>
        /// Immediately sets all of the atmosphere variables.
        /// </summary> 
        void SetAtmosphereVariables()
        {

            float i = weatherSphere.usePhysicalSunHeight ? weatherSphere.modifiedDayPercentage : weatherSphere.dayPercentage;

            weatherSphere.gradientExponent = atmosphereProfile.gradientExponent.GetFloatValue(i);
            weatherSphere.acScale = atmosphereProfile.acScale.GetFloatValue(i);
            weatherSphere.ambientLightHorizonColor = atmosphereProfile.ambientLightHorizonColor.GetColorValue(i);
            weatherSphere.ambientLightZenithColor = atmosphereProfile.ambientLightZenithColor.GetColorValue(i);
            weatherSphere.ambientLightMultiplier = atmosphereProfile.ambientLightMultiplier.GetFloatValue(i);
            weatherSphere.chemtrailsMoveSpeed = atmosphereProfile.chemtrailsMoveSpeed.GetFloatValue(i);
            weatherSphere.cirroMoveSpeed = atmosphereProfile.cirroMoveSpeed.GetFloatValue(i);
            weatherSphere.cirrusMoveSpeed = atmosphereProfile.cirrusMoveSpeed.GetFloatValue(i);
            weatherSphere.clippingThreshold = atmosphereProfile.clippingThreshold.GetFloatValue(i);
            weatherSphere.cloudCohesion = atmosphereProfile.cloudCohesion.GetFloatValue(i);
            weatherSphere.cloudColor = atmosphereProfile.cloudColor.GetColorValue(i);
            weatherSphere.cloudDetailAmount = atmosphereProfile.cloudDetailAmount.GetFloatValue(i);
            weatherSphere.cloudDetailScale = atmosphereProfile.cloudDetailScale.GetFloatValue(i);
            weatherSphere.cloudHighlightColor = atmosphereProfile.cloudHighlightColor.GetColorValue(i);
            weatherSphere.cloudMainScale = atmosphereProfile.cloudMainScale.GetFloatValue(i);
            weatherSphere.cloudMoonColor = atmosphereProfile.cloudMoonColor.GetColorValue(i);
            weatherSphere.cloudMoonHighlightFalloff = atmosphereProfile.cloudMoonHighlightFalloff.GetFloatValue(i);
            weatherSphere.cloudSunHighlightFalloff = atmosphereProfile.cloudSunHighlightFalloff.GetFloatValue(i);
            weatherSphere.cloudTextureColor = atmosphereProfile.cloudTextureColor.GetColorValue(i);
            weatherSphere.cloudThickness = atmosphereProfile.cloudThickness.GetFloatValue(i);
            weatherSphere.cloudWindSpeed = atmosphereProfile.cloudWindSpeed.GetFloatValue(i);
            weatherSphere.fogColor1 = atmosphereProfile.fogColor1.GetColorValue(i);
            weatherSphere.fogColor2 = atmosphereProfile.fogColor2.GetColorValue(i);
            weatherSphere.fogColor3 = atmosphereProfile.fogColor3.GetColorValue(i);
            weatherSphere.fogColor4 = atmosphereProfile.fogColor4.GetColorValue(i);
            weatherSphere.fogColor5 = atmosphereProfile.fogColor5.GetColorValue(i);
            weatherSphere.fogStart1 = atmosphereProfile.fogStart1;
            weatherSphere.fogStart2 = atmosphereProfile.fogStart2;
            weatherSphere.fogStart3 = atmosphereProfile.fogStart3;
            weatherSphere.fogStart4 = atmosphereProfile.fogStart4;
            weatherSphere.fogDensityMultiplier = atmosphereProfile.fogDensityMultiplier.GetFloatValue(i);
            weatherSphere.fogFlareColor = atmosphereProfile.fogFlareColor.GetColorValue(i);
            weatherSphere.fogMoonFlareColor = atmosphereProfile.fogMoonFlareColor.GetColorValue(i);
            weatherSphere.fogHeight = atmosphereProfile.fogHeight.GetFloatValue(i);
            weatherSphere.fogVariationAmount = atmosphereProfile.fogVariationAmount.GetFloatValue(i);
            weatherSphere.fogVariationDirection = atmosphereProfile.fogVariationDirection;
            weatherSphere.fogVariationDistance = atmosphereProfile.fogVariationDistance.GetFloatValue(i);
            weatherSphere.fogVariationScale = atmosphereProfile.fogVariationScale.GetFloatValue(i);
            weatherSphere.fogLightFlareFalloff = atmosphereProfile.fogLightFlareFalloff.GetFloatValue(i);
            weatherSphere.fogLightFlareIntensity = atmosphereProfile.fogLightFlareIntensity.GetFloatValue(i);
            weatherSphere.fogLightFlareSquish = atmosphereProfile.fogLightFlareSquish.GetFloatValue(i);
            weatherSphere.fogBase = atmosphereProfile.fogBase.GetFloatValue(i);
            weatherSphere.heightFogColor = atmosphereProfile.heightFogColor.GetColorValue(i);
            weatherSphere.heightFogDistance = atmosphereProfile.heightFogDistance.GetFloatValue(i);
            weatherSphere.heightFogIntensity = atmosphereProfile.heightFogIntensity.GetFloatValue(i);
            weatherSphere.heightFogTransition = atmosphereProfile.heightFogTransition.GetFloatValue(i);
            weatherSphere.heightFogVariationAmount = atmosphereProfile.heightFogVariationAmount.GetFloatValue(i);
            weatherSphere.heightFogVariationScale = atmosphereProfile.heightFogVariationScale.GetFloatValue(i);
            weatherSphere.galaxy1Color = atmosphereProfile.galaxy1Color.GetColorValue(i);
            weatherSphere.galaxy2Color = atmosphereProfile.galaxy2Color.GetColorValue(i);
            weatherSphere.galaxy3Color = atmosphereProfile.galaxy3Color.GetColorValue(i);
            weatherSphere.galaxyIntensity = atmosphereProfile.galaxyIntensity.GetFloatValue(i);
            weatherSphere.highAltitudeCloudColor = atmosphereProfile.highAltitudeCloudColor.GetColorValue(i);
            weatherSphere.lightScatteringColor = atmosphereProfile.lightScatteringColor.GetColorValue(i);
            weatherSphere.moonlightColor = atmosphereProfile.moonlightColor.GetColorValue(i);
            weatherSphere.moonColor = atmosphereProfile.moonColor.GetColorValue(i);
            weatherSphere.moonFalloff = atmosphereProfile.moonFalloff.GetFloatValue(i);
            weatherSphere.moonFlareColor = atmosphereProfile.moonFlareColor.GetColorValue(i);
            weatherSphere.useRainbow = atmosphereProfile.useRainbow;
            weatherSphere.rainbowPosition = atmosphereProfile.rainbowPosition.GetFloatValue(i);
            weatherSphere.rainbowWidth = atmosphereProfile.rainbowWidth.GetFloatValue(i);
            weatherSphere.shadowDistance = atmosphereProfile.shadowDistance.GetFloatValue(i);
            weatherSphere.skyHorizonColor = atmosphereProfile.skyHorizonColor.GetColorValue(i);
            weatherSphere.skyZenithColor = atmosphereProfile.skyZenithColor.GetColorValue(i);
            weatherSphere.spherize = atmosphereProfile.spherize.GetFloatValue(i);
            weatherSphere.starColor = atmosphereProfile.starColor.GetColorValue(i);
            weatherSphere.sunColor = atmosphereProfile.sunColor.GetColorValue(i);
            weatherSphere.sunDirection = atmosphereProfile.sunDirection.GetFloatValue(i);
            weatherSphere.sunFalloff = atmosphereProfile.sunFalloff.GetFloatValue(i);
            weatherSphere.sunFlareColor = atmosphereProfile.sunFlareColor.GetColorValue(i);
            weatherSphere.sunlightColor = atmosphereProfile.sunlightColor.GetColorValue(i);
            weatherSphere.sunPitch = atmosphereProfile.sunPitch.GetFloatValue(i);
            weatherSphere.sunSize = atmosphereProfile.sunSize.GetFloatValue(i);
            weatherSphere.textureAmount = atmosphereProfile.textureAmount.GetFloatValue(i);
            weatherSphere.fogSmoothness = atmosphereProfile.fogSmoothness.GetFloatValue(i);
            weatherSphere.texturePanDirection = atmosphereProfile.texturePanDirection;
            weatherSphere.cloudTexture = atmosphereProfile.cloudTexture;
            weatherSphere.chemtrailsTexture = atmosphereProfile.chemtrailsTexture;
            weatherSphere.cirrusCloudTexture = atmosphereProfile.cirrusCloudTexture;
            weatherSphere.altocumulusCloudTexture = atmosphereProfile.altocumulusCloudTexture;
            weatherSphere.cirrostratusCloudTexture = atmosphereProfile.cirrostratusCloudTexture;
            weatherSphere.starMap = atmosphereProfile.starMap;
            weatherSphere.galaxyMap = atmosphereProfile.galaxyMap;
            weatherSphere.galaxyStarMap = atmosphereProfile.galaxyStarMap;
            weatherSphere.galaxyVariationMap = atmosphereProfile.galaxyVariationMap;
            weatherSphere.lightScatteringMap = atmosphereProfile.lightScatteringMap;

            weatherSphere.partlyCloudyLuxuryClouds = atmosphereProfile.partlyCloudyLuxuryClouds;
            weatherSphere.mostlyCloudyLuxuryClouds = atmosphereProfile.mostlyCloudyLuxuryClouds;
            weatherSphere.overcastLuxuryClouds = atmosphereProfile.overcastLuxuryClouds;
            weatherSphere.lowBorderLuxuryClouds = atmosphereProfile.lowBorderLuxuryClouds;
            weatherSphere.highBorderLuxuryClouds = atmosphereProfile.highBorderLuxuryClouds;
            weatherSphere.lowNimbusLuxuryClouds = atmosphereProfile.lowNimbusLuxuryClouds;
            weatherSphere.midNimbusLuxuryClouds = atmosphereProfile.midNimbusLuxuryClouds;
            weatherSphere.highNimbusLuxuryClouds = atmosphereProfile.highNimbusLuxuryClouds;
            weatherSphere.luxuryVariation = atmosphereProfile.luxuryVariation;


#if COZY_URP || COZY_HDRP
            weatherSphere.sunFlare = atmosphereProfile.sunFlare;
            weatherSphere.moonFlare = atmosphereProfile.moonFlare;
#endif

            foreach (CozyAtmosphereModule biome in biomes)
            {
                if (biome == null) continue;
                if (biome.system.weight == 0) continue;

                weatherSphere.gradientExponent = Mathf.Lerp(weatherSphere.gradientExponent, biome.atmosphereProfile.gradientExponent.GetFloatValue(i), biome.weight);
                weatherSphere.ambientLightHorizonColor = Color.Lerp(weatherSphere.ambientLightHorizonColor, biome.atmosphereProfile.ambientLightHorizonColor.GetColorValue(i), biome.weight);
                weatherSphere.ambientLightZenithColor = Color.Lerp(weatherSphere.ambientLightZenithColor, biome.atmosphereProfile.ambientLightZenithColor.GetColorValue(i), biome.weight);
                weatherSphere.ambientLightMultiplier = Mathf.Lerp(weatherSphere.ambientLightMultiplier, biome.atmosphereProfile.ambientLightMultiplier.GetFloatValue(i), biome.weight);
                weatherSphere.clippingThreshold = Mathf.Lerp(weatherSphere.clippingThreshold, biome.atmosphereProfile.clippingThreshold.GetFloatValue(i), biome.weight);
                weatherSphere.cloudCohesion = Mathf.Lerp(weatherSphere.cloudCohesion, biome.atmosphereProfile.cloudCohesion.GetFloatValue(i), biome.weight);
                weatherSphere.cloudColor = Color.Lerp(weatherSphere.cloudColor, biome.atmosphereProfile.cloudColor.GetColorValue(i), biome.weight);
                weatherSphere.cloudHighlightColor = Color.Lerp(weatherSphere.cloudHighlightColor, biome.atmosphereProfile.cloudHighlightColor.GetColorValue(i), biome.weight);
                weatherSphere.cloudMoonColor = Color.Lerp(weatherSphere.cloudMoonColor, biome.atmosphereProfile.cloudMoonColor.GetColorValue(i), biome.weight);
                weatherSphere.cloudMoonHighlightFalloff = Mathf.Lerp(weatherSphere.cloudMoonHighlightFalloff, biome.atmosphereProfile.cloudMoonHighlightFalloff.GetFloatValue(i), biome.weight);
                weatherSphere.cloudSunHighlightFalloff = Mathf.Lerp(weatherSphere.cloudSunHighlightFalloff, biome.atmosphereProfile.cloudSunHighlightFalloff.GetFloatValue(i), biome.weight);
                weatherSphere.cloudTextureColor = Color.Lerp(weatherSphere.cloudTextureColor, biome.atmosphereProfile.cloudTextureColor.GetColorValue(i), biome.weight);
                weatherSphere.cloudThickness = Mathf.Lerp(weatherSphere.cloudThickness, biome.atmosphereProfile.cloudThickness.GetFloatValue(i), biome.weight);
                weatherSphere.fogColor1 = Color.Lerp(weatherSphere.fogColor1, biome.atmosphereProfile.fogColor1.GetColorValue(i), biome.weight);
                weatherSphere.fogColor2 = Color.Lerp(weatherSphere.fogColor2, biome.atmosphereProfile.fogColor2.GetColorValue(i), biome.weight);
                weatherSphere.fogColor3 = Color.Lerp(weatherSphere.fogColor3, biome.atmosphereProfile.fogColor3.GetColorValue(i), biome.weight);
                weatherSphere.fogColor4 = Color.Lerp(weatherSphere.fogColor4, biome.atmosphereProfile.fogColor4.GetColorValue(i), biome.weight);
                weatherSphere.fogColor5 = Color.Lerp(weatherSphere.fogColor5, biome.atmosphereProfile.fogColor5.GetColorValue(i), biome.weight);
                weatherSphere.fogDensityMultiplier = Mathf.Lerp(weatherSphere.fogDensityMultiplier, biome.atmosphereProfile.fogDensityMultiplier.GetFloatValue(i), biome.weight);
                weatherSphere.fogFlareColor = Color.Lerp(weatherSphere.fogFlareColor, biome.atmosphereProfile.fogFlareColor.GetColorValue(i), biome.weight);
                weatherSphere.fogMoonFlareColor = Color.Lerp(weatherSphere.fogMoonFlareColor, biome.atmosphereProfile.fogMoonFlareColor.GetColorValue(i), biome.weight);
                weatherSphere.fogHeight = Mathf.Lerp(weatherSphere.fogHeight, biome.atmosphereProfile.fogHeight.GetFloatValue(i), biome.weight);
                weatherSphere.fogVariationAmount = Mathf.Lerp(weatherSphere.fogVariationAmount, biome.atmosphereProfile.fogVariationAmount.GetFloatValue(i), biome.weight);
                weatherSphere.fogVariationDistance = Mathf.Lerp(weatherSphere.fogVariationDistance, biome.atmosphereProfile.fogVariationDistance.GetFloatValue(i), biome.weight);
                weatherSphere.fogLightFlareFalloff = Mathf.Lerp(weatherSphere.fogLightFlareFalloff, biome.atmosphereProfile.fogLightFlareFalloff.GetFloatValue(i), biome.weight);
                weatherSphere.fogLightFlareIntensity = Mathf.Lerp(weatherSphere.fogLightFlareIntensity, biome.atmosphereProfile.fogLightFlareIntensity.GetFloatValue(i), biome.weight);
                weatherSphere.fogLightFlareSquish = Mathf.Lerp(weatherSphere.fogLightFlareSquish, biome.atmosphereProfile.fogLightFlareSquish.GetFloatValue(i), biome.weight);
                weatherSphere.galaxy1Color = Color.Lerp(weatherSphere.galaxy1Color, biome.atmosphereProfile.galaxy1Color.GetColorValue(i), biome.weight);
                weatherSphere.galaxy2Color = Color.Lerp(weatherSphere.galaxy2Color, biome.atmosphereProfile.galaxy2Color.GetColorValue(i), biome.weight);
                weatherSphere.galaxy3Color = Color.Lerp(weatherSphere.galaxy3Color, biome.atmosphereProfile.galaxy3Color.GetColorValue(i), biome.weight);
                weatherSphere.galaxyIntensity = Mathf.Lerp(weatherSphere.galaxyIntensity, biome.atmosphereProfile.galaxyIntensity.GetFloatValue(i), biome.weight);
                weatherSphere.highAltitudeCloudColor = Color.Lerp(weatherSphere.highAltitudeCloudColor, biome.atmosphereProfile.highAltitudeCloudColor.GetColorValue(i), biome.weight);
                weatherSphere.lightScatteringColor = Color.Lerp(weatherSphere.lightScatteringColor, biome.atmosphereProfile.lightScatteringColor.GetColorValue(i), biome.weight);
                weatherSphere.moonlightColor = Color.Lerp(weatherSphere.moonlightColor, biome.atmosphereProfile.moonlightColor.GetColorValue(i), biome.weight);
                weatherSphere.moonColor = Color.Lerp(weatherSphere.moonColor, biome.atmosphereProfile.moonColor.GetColorValue(i), biome.weight);
                weatherSphere.moonFalloff = Mathf.Lerp(weatherSphere.moonFalloff, biome.atmosphereProfile.moonFalloff.GetFloatValue(i), biome.weight);
                weatherSphere.moonFlareColor = Color.Lerp(weatherSphere.moonFlareColor, biome.atmosphereProfile.moonFlareColor.GetColorValue(i), biome.weight);
                weatherSphere.rainbowPosition = Mathf.Lerp(weatherSphere.rainbowPosition, biome.atmosphereProfile.rainbowPosition.GetFloatValue(i), biome.weight);
                weatherSphere.rainbowWidth = Mathf.Lerp(weatherSphere.rainbowWidth, biome.atmosphereProfile.rainbowWidth.GetFloatValue(i), biome.weight);
                weatherSphere.shadowDistance = Mathf.Lerp(weatherSphere.shadowDistance, biome.atmosphereProfile.shadowDistance.GetFloatValue(i), biome.weight);
                weatherSphere.skyHorizonColor = Color.Lerp(weatherSphere.skyHorizonColor, biome.atmosphereProfile.skyHorizonColor.GetColorValue(i), biome.weight);
                weatherSphere.skyZenithColor = Color.Lerp(weatherSphere.skyZenithColor, biome.atmosphereProfile.skyZenithColor.GetColorValue(i), biome.weight);
                weatherSphere.spherize = Mathf.Lerp(weatherSphere.spherize, biome.atmosphereProfile.spherize.GetFloatValue(i), biome.weight);
                weatherSphere.starColor = Color.Lerp(weatherSphere.starColor, biome.atmosphereProfile.starColor.GetColorValue(i), biome.weight);
                weatherSphere.sunColor = Color.Lerp(weatherSphere.sunColor, biome.atmosphereProfile.sunColor.GetColorValue(i), biome.weight);
                weatherSphere.sunDirection = Mathf.Lerp(weatherSphere.sunDirection, biome.atmosphereProfile.sunDirection.GetFloatValue(i), biome.weight);
                weatherSphere.sunFalloff = Mathf.Lerp(weatherSphere.sunFalloff, biome.atmosphereProfile.sunFalloff.GetFloatValue(i), biome.weight);
                weatherSphere.sunFlareColor = Color.Lerp(weatherSphere.sunFlareColor, biome.atmosphereProfile.sunFlareColor.GetColorValue(i), biome.weight);
                weatherSphere.sunlightColor = Color.Lerp(weatherSphere.sunlightColor, biome.atmosphereProfile.sunlightColor.GetColorValue(i), biome.weight);
                weatherSphere.sunPitch = Mathf.Lerp(weatherSphere.sunPitch, biome.atmosphereProfile.sunPitch.GetFloatValue(i), biome.weight);
                weatherSphere.sunSize = Mathf.Lerp(weatherSphere.sunSize, biome.atmosphereProfile.sunSize.GetFloatValue(i), biome.weight);
                weatherSphere.textureAmount = Mathf.Lerp(weatherSphere.textureAmount, biome.atmosphereProfile.textureAmount.GetFloatValue(i), biome.weight);
                weatherSphere.fogSmoothness = Mathf.Lerp(weatherSphere.fogSmoothness, biome.atmosphereProfile.fogSmoothness.GetFloatValue(i), biome.weight);
                weatherSphere.fogBase = Mathf.Lerp(weatherSphere.fogBase, biome.atmosphereProfile.fogBase.GetFloatValue(i), biome.weight);
                weatherSphere.heightFogColor = Color.Lerp(weatherSphere.heightFogColor, biome.atmosphereProfile.heightFogColor.GetColorValue(i), biome.weight);
                weatherSphere.heightFogDistance = Mathf.Lerp(weatherSphere.heightFogDistance, biome.atmosphereProfile.heightFogDistance.GetFloatValue(i), biome.weight);
                weatherSphere.heightFogIntensity = Mathf.Lerp(weatherSphere.heightFogIntensity, biome.atmosphereProfile.heightFogIntensity.GetFloatValue(i), biome.weight);
                weatherSphere.heightFogTransition = Mathf.Lerp(weatherSphere.heightFogTransition, biome.atmosphereProfile.heightFogTransition.GetFloatValue(i), biome.weight);
                weatherSphere.heightFogVariationAmount = Mathf.Lerp(weatherSphere.heightFogVariationAmount, biome.atmosphereProfile.heightFogVariationAmount.GetFloatValue(i), biome.weight);
                weatherSphere.heightFogVariationScale = Mathf.Lerp(weatherSphere.heightFogVariationScale, biome.atmosphereProfile.heightFogVariationScale.GetFloatValue(i), biome.weight);


            }

        }

        /// <summary>
        /// Smoothly interpolates the current atmosphere profile and all of the impacted settings by the transition time.
        /// </summary> 
        public void ChangeAtmosphere(AtmosphereProfile end, float transitionTime)
        {

            StartCoroutine(TransitionAtmosphere(end, transitionTime));

        }

        IEnumerator TransitionAtmosphere(AtmosphereProfile end, float transitionTime)
        {


            float gradientExponentStart = weatherSphere.gradientExponent;
            float acScaleStart = weatherSphere.acScale;
            Color ambientLightHorizonColorStart = weatherSphere.ambientLightHorizonColor;
            Color ambientLightZenithColorStart = weatherSphere.ambientLightZenithColor;
            float ambientLightMultiplierStart = weatherSphere.ambientLightMultiplier;
            float chemtrailsMoveSpeedStart = weatherSphere.chemtrailsMoveSpeed;
            float cirroMoveSpeedStart = weatherSphere.cirroMoveSpeed;
            float cirrusMoveSpeedStart = weatherSphere.cirrusMoveSpeed;
            float clippingThresholdStart = weatherSphere.clippingThreshold;
            float cloudCohesionStart = weatherSphere.cloudCohesion;
            Color cloudColorStart = weatherSphere.cloudColor;
            float cloudDetailAmountStart = weatherSphere.cloudDetailAmount;
            float cloudDetailScaleStart = weatherSphere.cloudDetailScale;
            Color cloudHighlightColorStart = weatherSphere.cloudHighlightColor;
            float cloudMainScaleStart = weatherSphere.cloudMainScale;
            Color cloudMoonColorStart = weatherSphere.cloudMoonColor;
            float cloudMoonHighlightFalloffStart = weatherSphere.cloudMoonHighlightFalloff;
            float cloudSunHighlightFalloffStart = weatherSphere.cloudSunHighlightFalloff;
            Color cloudTextureColorStart = weatherSphere.cloudTextureColor;
            float cloudThicknessStart = weatherSphere.cloudThickness;
            float cloudWindSpeedStart = weatherSphere.cloudWindSpeed;
            Color fogColor1Start = weatherSphere.fogColor1;
            Color fogColor2Start = weatherSphere.fogColor2;
            Color fogColor3Start = weatherSphere.fogColor3;
            Color fogColor4Start = weatherSphere.fogColor4;
            Color fogColor5Start = weatherSphere.fogColor5;
            float fogStart1Start = weatherSphere.fogStart1;
            float fogStart2Start = weatherSphere.fogStart2;
            float fogStart3Start = weatherSphere.fogStart3;
            float fogStart4Start = weatherSphere.fogStart4;
            float fogDensityMultiplierStart = weatherSphere.fogDensityMultiplier;
            Color fogFlareColorStart = weatherSphere.fogFlareColor;
            float fogHeightStart = weatherSphere.fogHeight;
            float fogLightFlareFalloffStart = weatherSphere.fogLightFlareFalloff;
            float fogLightFlareIntensityStart = weatherSphere.fogLightFlareIntensity;
            float fogLightFlareSquishStart = weatherSphere.fogLightFlareSquish;
            Color galaxy1ColorStart = weatherSphere.galaxy1Color;
            Color galaxy2ColorStart = weatherSphere.galaxy2Color;
            Color galaxy3ColorStart = weatherSphere.galaxy3Color;
            Color highAltitudeCloudColorStart = weatherSphere.highAltitudeCloudColor;
            Color lightScatteringColorStart = weatherSphere.lightScatteringColor;
            Color moonlightColorStart = weatherSphere.moonlightColor;
            Color moonFlareColorStart = weatherSphere.moonFlareColor;
            Color skyHorizonColorStart = weatherSphere.skyHorizonColor;
            Color skyZenithColorStart = weatherSphere.skyZenithColor;
            Color starColorStart = weatherSphere.starColor;
            Color sunColorStart = weatherSphere.sunColor;
            Color sunFlareColorStart = weatherSphere.sunFlareColor;
            Color sunlightColorStart = weatherSphere.sunlightColor;
            float galaxyIntensityStart = weatherSphere.galaxyIntensity;
            float moonFalloffStart = weatherSphere.moonFalloff;
            float rainbowPositionStart = weatherSphere.rainbowPosition;
            float rainbowWidthStart = weatherSphere.rainbowWidth;
            float shadowDistanceStart = weatherSphere.shadowDistance;
            float spherizeStart = weatherSphere.spherize;
            float sunDirectionStart = weatherSphere.sunDirection;
            float sunFalloffStart = weatherSphere.sunFalloff;
            float sunPitchStart = weatherSphere.sunPitch;
            float sunSizeStart = weatherSphere.sunSize;
            float textureAmountStart = weatherSphere.textureAmount;


            transitioningAtmosphere = true;
            float t = transitionTime;

            while (t > 0)
            {

                float div = 1 - (t / transitionTime);
                yield return new WaitForEndOfFrame();

                weatherSphere.gradientExponent = Mathf.Lerp(gradientExponentStart, end.gradientExponent.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.acScale = Mathf.Lerp(acScaleStart, end.acScale.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.ambientLightHorizonColor = Color.Lerp(ambientLightHorizonColorStart, end.ambientLightHorizonColor.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.ambientLightZenithColor = Color.Lerp(ambientLightZenithColorStart, end.ambientLightZenithColor.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.ambientLightMultiplier = Mathf.Lerp(ambientLightMultiplierStart, end.ambientLightMultiplier.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.chemtrailsMoveSpeed = Mathf.Lerp(chemtrailsMoveSpeedStart, end.chemtrailsMoveSpeed.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.cirroMoveSpeed = Mathf.Lerp(cirroMoveSpeedStart, end.cirroMoveSpeed.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.cirrusMoveSpeed = Mathf.Lerp(cirrusMoveSpeedStart, end.cirrusMoveSpeed.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.clippingThreshold = Mathf.Lerp(clippingThresholdStart, end.clippingThreshold.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.cloudCohesion = Mathf.Lerp(cloudCohesionStart, end.cloudCohesion.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.cloudColor = Color.Lerp(cloudColorStart, end.cloudColor.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.cloudDetailAmount = Mathf.Lerp(cloudDetailAmountStart, end.cloudDetailAmount.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.cloudDetailScale = Mathf.Lerp(cloudDetailScaleStart, end.cloudDetailScale.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.cloudHighlightColor = Color.Lerp(cloudHighlightColorStart, end.cloudHighlightColor.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.cloudMainScale = Mathf.Lerp(cloudMainScaleStart, end.cloudMainScale.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.cloudMoonColor = Color.Lerp(cloudMoonColorStart, end.cloudMoonColor.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.cloudMoonHighlightFalloff = Mathf.Lerp(cloudMoonHighlightFalloffStart, end.cloudMoonHighlightFalloff.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.cloudSunHighlightFalloff = Mathf.Lerp(cloudSunHighlightFalloffStart, end.cloudSunHighlightFalloff.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.cloudTextureColor = Color.Lerp(cloudTextureColorStart, end.cloudTextureColor.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.cloudThickness = Mathf.Lerp(cloudThicknessStart, end.cloudThickness.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.cloudWindSpeed = Mathf.Lerp(cloudWindSpeedStart, end.cloudWindSpeed.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.fogColor1 = Color.Lerp(fogColor1Start, end.fogColor1.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.fogColor2 = Color.Lerp(fogColor2Start, end.fogColor2.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.fogColor3 = Color.Lerp(fogColor3Start, end.fogColor3.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.fogColor4 = Color.Lerp(fogColor4Start, end.fogColor4.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.fogColor5 = Color.Lerp(fogColor5Start, end.fogColor5.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.fogStart1 = Mathf.Lerp(fogStart1Start, end.fogStart1, div);
                weatherSphere.fogStart2 = Mathf.Lerp(fogStart2Start, end.fogStart2, div);
                weatherSphere.fogStart3 = Mathf.Lerp(fogStart3Start, end.fogStart3, div);
                weatherSphere.fogStart4 = Mathf.Lerp(fogStart4Start, end.fogStart4, div);
                weatherSphere.fogDensityMultiplier = Mathf.Lerp(fogDensityMultiplierStart, end.fogDensityMultiplier.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.fogFlareColor = Color.Lerp(fogFlareColorStart, end.fogFlareColor.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.fogHeight = Mathf.Lerp(fogHeightStart, end.fogHeight.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.fogLightFlareFalloff = Mathf.Lerp(fogLightFlareFalloffStart, end.fogLightFlareFalloff.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.fogLightFlareIntensity = Mathf.Lerp(fogLightFlareIntensityStart, end.fogLightFlareIntensity.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.fogLightFlareSquish = Mathf.Lerp(fogLightFlareSquishStart, end.fogLightFlareSquish.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.galaxy1Color = Color.Lerp(galaxy1ColorStart, end.galaxy1Color.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.galaxy2Color = Color.Lerp(galaxy2ColorStart, end.galaxy2Color.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.galaxy3Color = Color.Lerp(galaxy3ColorStart, end.galaxy3Color.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.galaxyIntensity = Mathf.Lerp(galaxyIntensityStart, end.galaxyIntensity.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.highAltitudeCloudColor = Color.Lerp(highAltitudeCloudColorStart, end.highAltitudeCloudColor.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.lightScatteringColor = Color.Lerp(lightScatteringColorStart, end.lightScatteringColor.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.moonlightColor = Color.Lerp(moonlightColorStart, end.moonlightColor.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.moonFalloff = Mathf.Lerp(moonFalloffStart, end.moonFalloff.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.moonFlareColor = Color.Lerp(moonFlareColorStart, end.moonFlareColor.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.rainbowPosition = Mathf.Lerp(rainbowPositionStart, end.rainbowPosition.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.rainbowWidth = Mathf.Lerp(rainbowWidthStart, end.rainbowWidth.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.shadowDistance = Mathf.Lerp(shadowDistanceStart, end.shadowDistance.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.skyHorizonColor = Color.Lerp(skyHorizonColorStart, end.skyHorizonColor.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.skyZenithColor = Color.Lerp(skyZenithColorStart, end.skyZenithColor.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.spherize = Mathf.Lerp(spherizeStart, end.spherize.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.starColor = Color.Lerp(starColorStart, end.starColor.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.sunColor = Color.Lerp(sunColorStart, end.sunColor.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.sunDirection = Mathf.Lerp(sunDirectionStart, end.sunDirection.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.sunFalloff = Mathf.Lerp(sunFalloffStart, end.sunFalloff.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.sunFlareColor = Color.Lerp(sunFlareColorStart, end.sunFlareColor.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.sunlightColor = Color.Lerp(sunlightColorStart, end.sunlightColor.GetColorValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.sunPitch = Mathf.Lerp(sunPitchStart, end.sunPitch.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.sunSize = Mathf.Lerp(sunSizeStart, end.sunSize.GetFloatValue(weatherSphere.modifiedDayPercentage), div);
                weatherSphere.textureAmount = Mathf.Lerp(textureAmountStart, end.textureAmount.GetFloatValue(weatherSphere.modifiedDayPercentage), div);

                t -= Time.deltaTime;

            }

            transitioningAtmosphere = false;
            atmosphereProfile = end;

        }

        public void AddBiome()
        {
            weatherSphere = CozyWeather.instance;
            weatherSphere.atmosphereModule.biomes = FindObjectsOfType<CozyAtmosphereModule>().Where(x => x != weatherSphere.atmosphereModule).ToList();
        }

        public void RemoveBiome()
        {
            weatherSphere.atmosphereModule.biomes.Remove(this);
        }

        public void UpdateBiomeModule()
        {

        }

        public void ComputeBiomeWeights()
        {

            float totalSystemWeight = 0;
            biomes.RemoveAll(x => x == null);

            foreach (CozyAtmosphereModule biome in biomes)
            {
                if (biome != this)
                {
                    totalSystemWeight += biome.system.targetWeight;
                }
            }

            weight = Mathf.Clamp01(1 - (totalSystemWeight));
            totalSystemWeight += weight;

            foreach (CozyAtmosphereModule biome in biomes)
            {
                if (biome.system != this)
                    biome.weight = biome.system.targetWeight / (totalSystemWeight == 0 ? 1 : totalSystemWeight);
            }
        }

        public bool CheckBiome()
        {
            if (!weatherSphere.atmosphereModule)
            {
                Debug.LogError("The atmosphere biome module requires the atmosphere module to be enabled on your weather sphere. Please add the atmosphere module before setting up your biome.");
                return false;
            }
            return true;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CozyAtmosphereModule))]
    [CanEditMultipleObjects]
    public class E_CozyAtmosphereModule : E_CozyModule, E_BiomeModule, IControlPanel
    {
        public static bool fogOptionsWindow;
        public static bool atmosphereOptionsWindow;
        public static bool atmosphereSelectionOptionsWindow;
        public static bool cloudsOptionsWindow;
        public static bool celestialsOptionsWindow;
        public static Editor atmosEditor;
        bool tooltips;
        CozyAtmosphereModule atmosphereModule;


        public override GUIContent GetGUIContent()
        {
            return new GUIContent("    Atmosphere", (Texture)Resources.Load("Atmosphere"), "Manage skydome, fog, and lighting settings.");
        }

        public void GetControlPanel()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("atmosphereProfile"));
        }

        void OnEnable()
        {
            atmosphereModule = (CozyAtmosphereModule)target;
        }

        public override void OpenDocumentationURL()
        {
            Application.OpenURL("https://distant-lands.gitbook.io/cozy-stylized-weather-documentation/modules/atmosphere-module");
        }

        public override void GetDebugInformation()
        {

        }

        public override void DisplayInCozyWindow()
        {

            EditorGUI.indentLevel = 0;
            serializedObject.Update();
            tooltips = EditorPrefs.GetBool("CZY_Tooltips", true);

            GUIStyle labelStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            labelStyle.fontStyle = FontStyle.Bold;

            atmosphereSelectionOptionsWindow = EditorGUILayout.BeginFoldoutHeaderGroup(atmosphereSelectionOptionsWindow,
                new GUIContent("    Selection Settings"), EditorUtilities.FoldoutStyle);


            if (atmosphereSelectionOptionsWindow)
            {
                if (tooltips)
                    EditorGUILayout.HelpBox("How should this weather system manage atmosphere settings? Native sets all settings locally to this system, Profile sets global settings on the atmosphere profile.", MessageType.Info);


                EditorGUILayout.PropertyField(serializedObject.FindProperty("atmosphereProfile"));
                if (serializedObject.hasModifiedProperties)
                    atmosEditor = CreateEditor(serializedObject.FindProperty("atmosphereProfile").objectReferenceValue);

                EditorGUILayout.Space();

                if (tooltips)
                    EditorGUILayout.HelpBox("Set the shader model used for the sky, clouds, and fog here.", MessageType.Info);

            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUI.indentLevel = 0;

            if (serializedObject.FindProperty("atmosphereProfile").objectReferenceValue)
                if (atmosEditor == null)
                {
                    atmosEditor = CreateEditor(serializedObject.FindProperty("atmosphereProfile").objectReferenceValue);
                    (atmosEditor as E_AtmosphereProfile).OnInspectorGUIInline(atmosphereModule.weatherSphere);
                }
                else
                    (atmosEditor as E_AtmosphereProfile).OnInspectorGUIInline(atmosphereModule.weatherSphere);
            else
            {
                EditorGUILayout.HelpBox("Assign an atmosphere profile!", MessageType.Error);

            }

            serializedObject.ApplyModifiedProperties();

        }

        public void DrawBiomeReports()
        {

        }

        public void DrawInlineBiomeUI()
        {
            if (!target) return;
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("atmosphereProfile"));
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}