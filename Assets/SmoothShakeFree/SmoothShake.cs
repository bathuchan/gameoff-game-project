using UnityEngine;

namespace SmoothShakeFree
{
    [AddComponentMenu("Smooth Shake Free/Smooth Shake Free")]
    public class SmoothShake : ShakeBase
    {
        [Tooltip("Preset to use for this Smooth Shake")]
        public SmoothShakeFreePreset preset;

        [Header("Position Shake Settings")]
        [Tooltip("Settings for Position Shake")]
        public Shaker positionShake;
        [Header("Rotation Shake Settings")]
        [Tooltip("Settings for Rotation Shake")]
        public Shaker rotationShake;

        [HideInInspector] internal Vector3 startPosition;
        [HideInInspector] internal Vector3 startRotation;

        internal sealed override void Apply(Vector3[] value)
        {
            transform.localPosition = startPosition + value[0];
            transform.localEulerAngles = startRotation + value[1];
        }

        protected override Shaker[] GetShakers() { return new Shaker[] { positionShake, rotationShake }; }

        internal override void ResetDefaultValues()
        {
            transform.localPosition = startPosition;
            transform.localEulerAngles = startRotation;
        }

        internal sealed override void SaveDefaultValues()
        {
            startPosition = transform.localPosition;
            startRotation = transform.localEulerAngles;
        }

        internal sealed override void ApplyPresetSettings(SmoothShakeFreePreset preset)
        {
            positionShake.noiseType = preset.positionShake.noiseType;
            positionShake.amplitude = preset.positionShake.amplitude;
            positionShake.frequency = preset.positionShake.frequency;

            rotationShake.noiseType = preset.rotationShake.noiseType;
            rotationShake.amplitude = preset.rotationShake.amplitude;
            rotationShake.frequency = preset.rotationShake.frequency;

            timeSettings.enableOnStart = preset.timeSettings.enableOnStart;
            timeSettings.constantShake = preset.timeSettings.constantShake;
            timeSettings.fadeInDuration = preset.timeSettings.fadeInDuration;
            timeSettings.fadeOutDuration = preset.timeSettings.fadeOutDuration;
            timeSettings.fadeInCurve = preset.timeSettings.fadeInCurve;
            timeSettings.fadeOutCurve = preset.timeSettings.fadeOutCurve;
            timeSettings.holdDuration = preset.timeSettings.holdDuration;
        }
    }
}
