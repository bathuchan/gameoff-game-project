#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SmoothShakeFree
{
    [CustomPropertyDrawer(typeof(TimeSettings))]
    internal class TimeSettingsDrawer : PropertyDrawer
    {
        public float padding = 2f;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //Draw enableOnStart and constantShake
            EditorUtility.DrawSerializedProperties(property, ref position, padding, "enableOnStart", "constantShake");

            // Draw fadeInDuration and fadeInCurve 
            EditorUtility.DrawPropertiesHorizontally(property, ref position, "Fade In", padding, 25f, new float[] { 1.5f, 1 }, "fadeInDuration", "fadeInCurve");

            // Draw holdDuration
            EditorUtility.DrawSerializedProperties(property, ref position, padding, "holdDuration");

            // Draw fadeOutDuration and fadeOutCurve
            EditorUtility.DrawPropertiesHorizontally(property, ref position, "Fade Out", padding, 25f, new float[] { 1.5f, 1 }, "fadeOutDuration", "fadeOutCurve");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (EditorGUIUtility.singleLineHeight + padding) * 5;
        }
    }
}
#endif