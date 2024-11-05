using System;
using UnityEngine;

namespace SmoothShakeFree
{
    [Serializable]
    public class Shaker
    {
        //Serializable Properties
        [Tooltip("The type of shake to use")]
        public NoiseType noiseType;

        //Serializable properties
        [Tooltip("The amplitude (strength) of this shaker")]
        public Vector3 amplitude;
        [Tooltip("The frequency (speed) of this shaker")]
        public Vector3 frequency;

        //Convert to Vector3
        public Vector3 Evaluate(float t)
        {
            Vector3 modified;
            modified.x = EvaluateBase(t, amplitude.x, frequency.x);
            modified.y = EvaluateBase(t, amplitude.y, frequency.y);
            modified.z = EvaluateBase(t, amplitude.z, frequency.z);
            return modified;
        }

        //Evaluate based on noise type
        protected float EvaluateBase(
            float t,
            float amplitude,
            float frequency
            ) => noiseType switch
            {
                NoiseType.SineWave => amplitude * EvaluateSinewave(frequency * t),
                NoiseType.WhiteNoise => amplitude * EvaluateWhiteNoise(),
                _ => throw new Exception("Unknown noise type")
            };

        //Enum to store noise type
        public enum NoiseType
        {
            SineWave,
            WhiteNoise,
        }

        //SineWave
        private float EvaluateSinewave(float t) => Mathf.Sin(2 * Mathf.PI * t);

        //Whitenoise
        private float EvaluateWhiteNoise() => UnityEngine.Random.Range(-1f, 1f);
    }
}