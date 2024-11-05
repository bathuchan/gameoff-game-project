using UnityEngine;

namespace SmoothShakeFree
{
    [CreateAssetMenu(fileName = "SmoothShakePreset", menuName = "Smooth Shake Free/Smooth Shake Preset", order = 1)]
    public class SmoothShakeFreePreset : ScriptableObject
    {
        [Header("Time Settings")]
        [Tooltip("Settings for the shake timing")]
        public TimeSettings timeSettings;

#if UNITY_2020
        [Header("Position Shake Settings")]
        [Tooltip("Settings for Position Shake")]
        public Shaker positionShake = new Shaker();

        [Header("Rotation Shake Settings")]
        [Tooltip("Settings for Rotation Shake")]
        public Shaker rotationShake = new Shaker();
#else
        [Header("Position Shake Settings")]
        [Tooltip("Settings for Position Shake")]
        public Shaker positionShake = new();

        [Header("Rotation Shake Settings")]
        [Tooltip("Settings for Rotation Shake")]
        public Shaker rotationShake = new();
#endif
    }
}
