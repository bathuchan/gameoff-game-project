#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using static SmoothShakeFree.Shaker;

namespace SmoothShakeFree
{
    [CustomPropertyDrawer(typeof(Shaker))]
    internal class ShakerDrawer : PropertyDrawer
    {
        private readonly float padding = 2f;

        //Init foldoutStates
#if UNITY_2020
        private static Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();
#else
        private static Dictionary<string, bool> foldoutStates = new();
#endif

        //-----------------------------------------------------------------------------------------
        //Noise type implementation

        //Noise type variable setup
        private readonly string[] waveVariables = new string[] { "amplitude", "frequency" };
        private readonly string[] baseVariables = new string[] { "amplitude" };

        public void DrawNoiseTypeSettings(Rect position, SerializedProperty property, NoiseType noiseType)
        {
            switch (noiseType)
            {
                case NoiseType.SineWave:
                    EditorUtility.DrawSerializedProperties(property, ref position, padding, waveVariables);
                    break;
                case NoiseType.WhiteNoise:
                    EditorUtility.DrawSerializedProperties(property, ref position, padding, baseVariables);
                    break;
            }
        }

        public float GetPropertyAmount(NoiseType noiseType) => noiseType switch
        {
            NoiseType.SineWave => waveVariables.Length,
            NoiseType.WhiteNoise => baseVariables.Length,
            _ => throw new System.Exception("Unknown noise type")
        };

        //-----------------------------------------------------------------------------------------

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //Get noise type info
            SerializedProperty noiseTypeProperty = property.FindPropertyRelative("noiseType");
            NoiseType noiseType = (NoiseType)noiseTypeProperty.enumValueIndex;

            // Draw foldout
            InitFoldout(property, out string key);
            DrawFoldout(ref position, noiseType, key);
            if (!foldoutStates[key]) return;

            //Draw the noiseType property
            EditorUtility.DrawDropdown(property, ref position, padding, "Noise Type", "noiseType");

            // Draw fields based on the selected noiseType
            DrawNoiseTypeSettings(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property, noiseType);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            InitFoldout(property, out string key);
            if (!foldoutStates[key]) return EditorGUIUtility.singleLineHeight + padding;

            //Add height for foldout
            float foldoutHeight = EditorGUIUtility.singleLineHeight + padding;

            //Add height for noiseType property
            NoiseType noiseType = (NoiseType)property.FindPropertyRelative("noiseType").enumValueIndex;
            float propertiesHeight = GetPropertyAmount(noiseType) * (EditorGUIUtility.singleLineHeight + padding) + padding;
            if (EditorUtility.ThinView()) propertiesHeight += GetPropertyAmount(noiseType) * (EditorGUIUtility.singleLineHeight + padding);

            return foldoutHeight + propertiesHeight + (EditorGUIUtility.singleLineHeight + padding) + padding;
        }

        private void InitFoldout(SerializedProperty property, out string key)
        {
            key = GetFoldoutKey(property);

            // Initialize foldout state if not present
            if (!foldoutStates.ContainsKey(key))
            {
                foldoutStates[key] = false;
            }
        }

        private void DrawFoldout(ref Rect position, NoiseType noiseType, string key)
        {
            foldoutStates[key] = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight + padding), foldoutStates[key], "Shake Settings", true);
            position.y += EditorGUIUtility.singleLineHeight + padding;
        }

        private string GetFoldoutKey(SerializedProperty property)
        {
            return property.serializedObject.targetObject.GetInstanceID() + "-" + property.propertyPath;
        }
    }
}
#endif