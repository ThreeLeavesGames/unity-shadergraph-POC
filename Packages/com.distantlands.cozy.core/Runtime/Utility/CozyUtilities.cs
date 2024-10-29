using System.Collections;
using DistantLands.Cozy.Data;
using UnityEngine;

namespace DistantLands.Cozy
{
    public class CozyUtilities
    {

        public Color DoubleGradient(Gradient start, Gradient target, float depth, float time)
        {
            return Color.Lerp(start.Evaluate(time), target.Evaluate(time), depth);
        }



    }

    [System.Serializable]
    public class WeatherRelation
    {
        [Range(0, 1)] public float weight; public WeatherProfile profile; public bool transitioning = true;

        public IEnumerator Transition(float value, float time)
        {

            transitioning = true;
            float t = 0;
            float start = weight;

            while (t < time)
            {

                float div = (t / time);
                yield return new WaitForEndOfFrame();

                weight = Mathf.Lerp(start, value, div);
                t += Time.deltaTime;

            }

            weight = value;
            transitioning = false;

        }

    }


}