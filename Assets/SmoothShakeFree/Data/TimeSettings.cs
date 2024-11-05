using UnityEngine;

namespace SmoothShakeFree
{
    [System.Serializable]
    public struct TimeSettings
    {
        [HideInInspector] public float fadeValue;

        [Tooltip("Play this shake on start")]
        public bool enableOnStart;
        [Tooltip("Use an infinite holdduration (until stopped)")]
        public bool constantShake;

        [Tooltip("How long the shake fade in should last")]
        public float fadeInDuration;
        [Tooltip("The curve to use for the shake fade in")]
        public AnimationCurve fadeInCurve;

        [Tooltip("How long the shake should hold at full strength")]
        public float holdDuration;

        [Tooltip("How long the shake fade out should last")]
        public float fadeOutDuration;
        [Tooltip("The curve to use for the shake fade out")]
        public AnimationCurve fadeOutCurve;
        public readonly float GetShakeDuration() => fadeInDuration + holdDuration + fadeOutDuration;
    }

}
