#if UNITY_EDITOR
using UnityEditor;

namespace SmoothShakeFree
{
    [CustomEditor(typeof(SmoothShakeFreePreset), true)]
    internal class ShakeBasePresetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.DrawTitle("Smooth Shake Free");
            EditorUtility.DrawUILine(EditorUtility.SmoothShakeFreeColor, 1, 10);

            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }

}
#endif