using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmoothShakeFree
{
    public abstract class ShakeBase : MonoBehaviour
    {
        [Header("Time Settings")]
        [Tooltip("Settings for the shake timing")]
        public TimeSettings timeSettings;

        private bool willStop = false;

        [HideInInspector] internal Shaker[] shakers;
#if UNITY_2020
        [HideInInspector] internal readonly List<Coroutine> activeShakeRoutines = new List<Coroutine>();
#else
        [HideInInspector] internal readonly List<Coroutine> activeShakeRoutines = new();
#endif
        [HideInInspector] internal Coroutine clearAfterFinished;

        [HideInInspector] internal Vector3[] sum;

        internal void Awake()
        {
            shakers = GetShakers();
            sum = new Vector3[shakers.Length];
            if (timeSettings.enableOnStart) StartShake();
        }

        internal void Start()
        {
            if (timeSettings.enableOnStart) StartShake();
        }

        public virtual void StartShake()
        {
            willStop = false;
            if (activeShakeRoutines.Count == 0)
            {
                clearAfterFinished = StartCoroutine(ClearAfterFinished());
                SaveDefaultValues();
                for (int i = 0; i < shakers.Length; i++)
                {
                    activeShakeRoutines.Add(StartCoroutine(ShakeRoutine(shakers[i], timeSettings, i)));
                }
            }
            else
            {
                ForceStop();
                StartShake();
            }
        }

        public void StartShake(SmoothShakeFreePreset preset)
        {
            ApplyPresetSettings(preset);
            StartShake();
        }

        public void StopShake() => willStop = true;

        public void ForceStop()
        {
            for (int i = 0; i < activeShakeRoutines.Count; i++)
            {
                if (activeShakeRoutines[i] != null)
                {
                    StopCoroutine(activeShakeRoutines[i]);
                    activeShakeRoutines[i] = null;
                }
            }
            if (clearAfterFinished != null)
            {
                StopCoroutine(clearAfterFinished);
                clearAfterFinished = null;
            }

            for (int i = 0; i < sum.Length; i++)
            {
                sum[i] = Vector3.zero;
            }

            activeShakeRoutines.Clear();
            ResetDefaultValues();
            willStop = false;
        }

        protected IEnumerator ClearAfterFinished()
        {
            if (timeSettings.constantShake)
            {
                while (true)
                {
                    if (willStop) break;
                    yield return null;
                }
                yield return new WaitForSeconds(timeSettings.fadeOutDuration);
                ForceStop();
            }
            else
            {
                yield return new WaitForSeconds(timeSettings.GetShakeDuration());
                ForceStop();
            }
        }

        protected IEnumerator ShakeRoutine(Shaker shaker, TimeSettings timeSettings, int i)
        {
            bool isFadingOut = false;
            if (timeSettings.fadeInDuration > 0) { yield return FadeRoutine(this.timeSettings.fadeInCurve, shaker, timeSettings, isFadingOut, i); }

            if (timeSettings.holdDuration > 0 && !this.timeSettings.constantShake) { yield return HoldRoutine(timeSettings.holdDuration, shaker, timeSettings, i); }
            if (this.timeSettings.constantShake) { yield return HoldRoutine(Mathf.Infinity, shaker, timeSettings, i); }

            isFadingOut = true;
            if (timeSettings.fadeOutDuration > 0) { yield return FadeRoutine(this.timeSettings.fadeOutCurve, shaker, timeSettings, isFadingOut, i); }
        }

        private IEnumerator FadeRoutine(AnimationCurve curve, Shaker shaker, TimeSettings timeSettings, bool isFadingOut, int i)
        {
            //Don't play the fade routine if the curve has no keys
            if (curve.length <= 1) yield break;

            if (isFadingOut && timeSettings.holdDuration == 0 && timeSettings.fadeInDuration == 0) timeSettings.fadeValue = 1;

            Keyframe[] keys = curve.keys;
            float tEnd = isFadingOut ? timeSettings.fadeOutDuration : timeSettings.fadeInDuration;
            float t = 0;

            while (t < tEnd)
            {
                if (!isFadingOut && willStop) yield break;
#if UNITY_2020
                float remappedTime = Utility.Remap(t, 0, tEnd, keys[0].time, keys[keys.Length - 1].time);
#else
                float remappedTime = Utility.Remap(t, 0, tEnd, keys[0].time, keys[^1].time);
#endif
                //timeSettings.fadeValue = Utility.Remap(curve.Evaluate(remappedTime), keys[0].value, keys[^1].value, isFadingOut ? 1 : 0, isFadingOut ? 0 : 1);
                timeSettings.fadeValue = curve.Evaluate(remappedTime);
                Execute(shaker, timeSettings, i);
                yield return null;
                t += Time.deltaTime;
            }

#if UNITY_2020
            timeSettings.fadeValue = Utility.Remap(curve.Evaluate(keys[keys.Length - 1].time), keys[0].value, keys[keys.Length - 1].value, isFadingOut ? 1 : 0, isFadingOut ? 0 : 1);
#else
            timeSettings.fadeValue = Utility.Remap(curve.Evaluate(keys[^1].time), keys[0].value, keys[^1].value, isFadingOut ? 1 : 0, isFadingOut ? 0 : 1);
#endif
            Execute(shaker, timeSettings, i);
        }

        private IEnumerator HoldRoutine(float duration, Shaker shaker, TimeSettings timeSettings, int i)
        {
            if (timeSettings.fadeValue == 0) timeSettings.fadeValue = 1;

            float t = 0;

            // Check if the duration is infinite
            if (float.IsInfinity(duration))
            {
                while (true) // Infinite loop
                {
                    if (willStop) yield break;

                    Execute(shaker, timeSettings, i);
                    yield return null;
                }
            }
            else
            {
                while (t < duration)
                {
                    if (willStop) yield break;

                    Execute(shaker, timeSettings, i);
                    yield return null;
                    t += Time.deltaTime;
                }
            }

            if (timeSettings.fadeOutDuration == 0)
            {
                timeSettings.fadeValue = 0;
                Execute(shaker, timeSettings, i);
            }
        }

        protected virtual void Execute(Shaker shaker, TimeSettings timeSettings, int i)
        {
            sum[i] = shaker.Evaluate(Time.time) * timeSettings.fadeValue;
        }

        protected virtual void ApplySum() => Apply(sum);

        private void Update() { if (activeShakeRoutines.Count > 0) ApplySum(); }

        internal abstract void Apply(Vector3[] value);
        protected abstract Shaker[] GetShakers();
        internal abstract void SaveDefaultValues();
        internal abstract void ResetDefaultValues();
        internal abstract void ApplyPresetSettings(SmoothShakeFreePreset preset);
    }

}
