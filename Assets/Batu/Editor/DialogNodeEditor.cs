using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogNode))]
public class DialogNodeEditor : Editor
{
    private SerializedObject dialogNodeSerialized;

    private void OnEnable()
    {
        dialogNodeSerialized = new SerializedObject(target);
    }

    public override void OnInspectorGUI()
    {
        dialogNodeSerialized.Update();

        DialogNode dialogNode = (DialogNode)target;

        EditorGUILayout.LabelField("Node ID: " + dialogNode.nodeID);
        EditorGUILayout.BeginVertical("box");

        if (GUILayout.Button("Add New Text Segment"))
        {
            AddTextSegment(dialogNode);
        }

        if (dialogNode.npcTextSegments.Count != 0)
        {
            EditorGUILayout.BeginVertical("box");

            for (int j = 0; j < dialogNode.npcTextSegments.Count; j++)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField((j + 1) + ". Text Segment");
                TextSegment currentNodeSegment = dialogNode.npcTextSegments[j];

                currentNodeSegment.text = EditorGUILayout.TextField("NPC Text", currentNodeSegment.text);

                if (currentNodeSegment.showDetails = EditorGUILayout.Foldout(currentNodeSegment.showDetails, "Customize Text:"))
                {
                    currentNodeSegment.color = EditorGUILayout.ColorField("Text Color: ", currentNodeSegment.color);
                    currentNodeSegment.bold = EditorGUILayout.Toggle("Bold", currentNodeSegment.bold);
                    currentNodeSegment.italic = EditorGUILayout.Toggle("Italic", currentNodeSegment.italic);
                    currentNodeSegment.sizeMultiplier = EditorGUILayout.FloatField("Font Size Multiplier", currentNodeSegment.sizeMultiplier);
                }

                if (GUILayout.Button("Delete Text Segment: " + currentNodeSegment.segmentId))
                {
                    dialogNode.RemoveTextSegment(currentNodeSegment);
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(currentNodeSegment)); // Delete the segment asset
                    AssetDatabase.Refresh();
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();

        dialogNode.isMonologue = EditorGUILayout.Toggle("Is Monologue", dialogNode.isMonologue);

        if (dialogNode.isMonologue)
        {
            dialogNode.nextNodeID = EditorGUILayout.IntField("Next Node ID", dialogNode.nextNodeID);

            // Disable choices if this is a monologue
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField("Choices (Disabled for Monologues)");
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            if (GUILayout.Button("Add Choice"))
            {
                dialogNode.AddChoice(new Choice());
                EditorUtility.SetDirty(dialogNode);
            }

            for (int j = 0; j < dialogNode.choices.Count; j++)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField((j + 1) + ". Choice");
                dialogNode.choices[j].choiceText = EditorGUILayout.TextField("Choice Text", dialogNode.choices[j].choiceText);
                dialogNode.choices[j].nextNodeID = EditorGUILayout.IntField("Next Node ID", dialogNode.choices[j].nextNodeID);

                if (GUILayout.Button("Delete Choice"))
                {
                    dialogNode.RemoveChoice(dialogNode.choices[j]);
                }
                EditorGUILayout.EndVertical();
            }
        }

        // Mark the target as dirty if any changes are made
        if (dialogNodeSerialized.ApplyModifiedProperties() || GUI.changed)
        {
            EditorUtility.SetDirty(dialogNode);
            AssetDatabase.SaveAssets();
        }
    }

    private void AddTextSegment(DialogNode dialogNode)
    {
        TextSegment newSegment = CreateInstance<TextSegment>();
        newSegment.segmentId = dialogNode.npcTextSegments.Count;

        dialogNode.AddTextSegment(newSegment);

        string dialogDataPath = AssetDatabase.GetAssetPath(dialogNode);
        string folderPath = Path.GetDirectoryName(dialogDataPath);
        string segmentsFolderPath = Path.Combine(folderPath, "TextSegments");

        if (!AssetDatabase.IsValidFolder(segmentsFolderPath))
        {
            AssetDatabase.CreateFolder(folderPath, "TextSegments");
        }

        string segmentAssetPath = Path.Combine(segmentsFolderPath, $"{dialogNode.name}_TextSegment_{newSegment.segmentId}.asset");
        AssetDatabase.CreateAsset(newSegment, segmentAssetPath.Replace("\\", "/"));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
