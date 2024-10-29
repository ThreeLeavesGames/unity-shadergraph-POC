//  Distant Lands 2024
//  COZY: Stylized Weather 3
//  All code included in this file is protected under the Unity Asset Store Eula

using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DistantLands.Cozy.Data
{
    [System.Serializable]
    public abstract class FXProfile : ScriptableObject
    {

        [TransitionTime]
        [Tooltip("A curve modifier that is used to impact the speed of the transition for this effect.")]
        public AnimationCurve transitionTimeModifier = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        protected CozyWeather weatherSphere;
        [System.Serializable]
        public struct OverrideData
        {
            public float value;
            public bool overrideValue;
            public static implicit operator bool(OverrideData data)
            {
                return data.overrideValue;
            }
            public OverrideData(float _value, bool _overrideValue)
            {
                overrideValue = _overrideValue;
                value = _value;
            }
            public static implicit operator float(OverrideData data)
            {
                return data.overrideValue ? data.value : 0;
            }
            public static implicit operator OverrideData(float value)
            {
                return new OverrideData(value, true);
            }
        }

        /// <summary>
        /// Plays the Cozy effect at a set intensity.
        /// </summary>      
        /// <param name="weight">The weight (or intensity percentage) that this effect will play at. From 0.0 to 1.0</param>
        public abstract void PlayEffect(float weight);

        /// <summary>
        /// Called to initialize the COZY effect into the system.
        /// </summary>                                                                                                          
        /// <param name="module">Holds a reference to the Cozy Module that this FX profile works with.</param>
        public virtual bool InitializeEffect(CozyWeather weather)
        {

            weatherSphere = weather ? weather : CozyWeather.instance;
            return true;
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(FXProfile))]
    [CanEditMultipleObjects]
    public abstract class E_FXProfile : Editor
    {

        public abstract float GetLineHeight();

        public abstract void RenderInWindow(Rect pos);

    }
#endif
}