#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SmoothShakeFree
{
    [CustomEditor(typeof(ShakeBase), true)]
    internal class ShakeBaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ShakeBase shakeBase = (ShakeBase)target;

            EditorUtility.DrawTitle("Smooth Shake Free");
            EditorUtility.DrawUILine(EditorUtility.SmoothShakeFreeColor, 1, 10);

            if(shakeBase is SmoothShake ssf)
            {
                DrawPresetInspector(ssf.preset, ssf);
            }

            EditorUtility.DrawUILine(Color.gray, 1, 10);

            //Draw test buttons
            DrawTestButtons(shakeBase);


            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPresetInspector(SmoothShakeFreePreset preset, ShakeBase shakeBase)
        {
            //Draw preset property
            EditorGUILayout.PropertyField(serializedObject.FindProperty("preset"), true);

            if (preset)
            {
                if (!Application.isPlaying) shakeBase.ApplyPresetSettings(preset);

                //Helpbox
                EditorGUILayout.HelpBox("This shake being overriden by " + preset.name, MessageType.Info);

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Go To Preset"))
                    {
                        Selection.activeObject = preset;
                    }

                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                DrawPropertiesExcluding(serializedObject, "m_Script", "preset");
            }
        }

        void DrawTestButtons(ShakeBase shakeBase)
        {
            //Test start and stop buttons in horizontal
            EditorGUILayout.BeginHorizontal();
            {
                //Disable buttons in edit mode
                GUI.enabled = Application.isPlaying;
                if (GUILayout.Button("Test Shake"))
                    shakeBase.StartShake();
                if (GUILayout.Button("Stop Test Shake"))
                    shakeBase.StopShake();
                if (GUILayout.Button("Force Stop Test Shake"))
                    shakeBase.ForceStop();
            }
            EditorGUILayout.EndHorizontal();

            if (!GUI.enabled)
                EditorGUILayout.HelpBox("You can only test shakes play mode", MessageType.Info);
        }
    }
}
#endif