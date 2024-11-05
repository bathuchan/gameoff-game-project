#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace SmoothShakeFree
{
    public static class EditorUtility
    {
#if UNITY_2020
        public static Color SmoothShakeFreeColor = new Color(0.78f, 0.78f, 0.78f);
#else
        public static Color SmoothShakeFreeColor = new(0.78f, 0.78f, 0.78f);
#endif

        public static void DrawTitle(string title)
        {
            //-------------------------------------------
            // Create a custom GUIStyle
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 18;  // Adjust the font size as needed
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            //-------------------------------------------

            EditorGUILayout.LabelField(title, titleStyle);
            EditorGUILayout.GetControlRect(false, 10);
        }

        public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }

        public static void DisplaySerializedProperties(SerializedObject obj, params string[] properties)
        {
            foreach (string property in properties)
            {
                SerializedProperty myDataProperty = obj.FindProperty(property);
                if (myDataProperty == null)
                {
                    Debug.LogError("Property '" + property + "' not found!");
                    continue;
                }
                EditorGUILayout.PropertyField(myDataProperty, true);
                obj.ApplyModifiedProperties();
            }
        }

        public static void DrawSerializedProperties(SerializedProperty property, ref Rect position, float padding, params string[] properties)
        {
            for (int i = 0; i < properties.Length; i++)
            {
                SerializedProperty newProperty = property.FindPropertyRelative(properties[i]);

                if (newProperty == null)
                    Debug.LogError("Property '" + properties[i] + "' not found!");

                float propertyHeight = EditorGUI.GetPropertyHeight(newProperty);
                Rect fieldRect = new Rect(position.x, position.y, position.width, propertyHeight);
                EditorGUI.PropertyField(fieldRect, newProperty);
                position.y += propertyHeight + padding;
            }
        }

        public static string AddSpacesToSentence(string text, bool preserveAcronyms = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            string newText = Regex.Replace(text, "([a-z])([A-Z])", "$1 $2");
            if (preserveAcronyms)
            {
                newText = Regex.Replace(newText, "([A-Z])([A-Z][a-z])", "$1 $2");
            }
            return newText;
        }

        public static void DrawPropertiesHorizontally(SerializedProperty property, ref Rect position, string label, float padding, float minWidth, float[] weight, params string[] properties)
        {
            if (position.width < 5) return;

            Rect[] rects = new Rect[properties.Length];

            float[] minWidths = new float[properties.Length];
            for (int i = 0; i < minWidths.Length; i++) minWidths[i] = minWidth;

            Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            SplitRectsHorizontally(rect, padding, minWidths, weight, ref rects);

            for (int i = 0; i < properties.Length; i++)
            {
                if (i == 0) EditorGUI.PropertyField(rects[0], property.FindPropertyRelative(properties[0]), new GUIContent(label));
                else EditorGUI.PropertyField(rects[i], property.FindPropertyRelative(properties[i]), GUIContent.none);
            }
            position.y += EditorGUIUtility.singleLineHeight + padding;
        }

        public static void DrawDropdown(SerializedProperty property, ref Rect position, float padding, string label, string propertypath)
        {
            float dropdownHeight = EditorGUI.GetPropertyHeight(property.FindPropertyRelative(propertypath));
            Rect dropdownRect = new Rect(position.x, position.y, position.width, dropdownHeight);
            EditorGUI.PropertyField(dropdownRect, property.FindPropertyRelative(propertypath), new GUIContent(label));
            position.y += dropdownHeight + padding;
        }

        public static bool ThinView(float viewWidth = 330f)
        {
            if (EditorGUIUtility.currentViewWidth > viewWidth)
                return false;
            else
                return true;
        }


        //------------------
        //Credit for this section goes to Wessel van der Es 
        public static void SplitRectsHorizontally(this Rect rect, float space, float[] minWidths, float[] weights, ref Rect[] rects)
        {
            int count = weights.Length;

            float budget = rect.width - (count - 1) * space - minWidths.Sum();
            float totalWeight = weights.Sum();

            float offset = 0.0f;
            float error = 0.0f;
            for (int i = 0; i < count; i++)
            {
                rects[i] = rect;
                rects[i].x += offset;
                rects[i].width = minWidths[i] + weights[i] / totalWeight * budget + error;

                float rounded = Mathf.Round(rects[i].width);
                error = rects[i].width - rounded;
                rects[i].width = rounded;

                offset += rects[i].width + space;
            }
        }

        public static float Sum(this IEnumerable<float> numbers)
        {
            float sum = 0.0f;
            foreach (float number in numbers)
                sum += number;
            return sum;
        }
        //------------------
    }

}
#endif