using System.IO;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static Codice.Client.Common.Connection.AskCredentialsToUser;


[CustomEditor(typeof(DialogNode))]
public class DialogNodeEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
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
                EditorGUILayout.LabelField((j + 1) + ".Text Segment");
                TextSegment currentNodeSegment = dialogNode.npcTextSegments[j];
                currentNodeSegment.text = EditorGUILayout.TextField("NPC Text", currentNodeSegment.text);

                if (currentNodeSegment.showDetails = EditorGUILayout.Foldout(currentNodeSegment.showDetails, "Customize Text:"))
                {
                    currentNodeSegment.color = EditorGUILayout.ColorField("Text Color: ", currentNodeSegment.color);
                    currentNodeSegment.bold = EditorGUILayout.Toggle("Bold", currentNodeSegment.bold);
                    currentNodeSegment.italic = EditorGUILayout.Toggle("Italic", currentNodeSegment.italic);
                    currentNodeSegment.sizeMultiplier = EditorGUILayout.FloatField("Font Size Multipler", currentNodeSegment.sizeMultiplier);


                }
                if (GUILayout.Button("Delete Text Segment: "+currentNodeSegment.segmentId))
                {
                    dialogNode.RemoveTextSegment(currentNodeSegment);
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(currentNodeSegment)); // Deletion of a Node
                    AssetDatabase.Refresh();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.BeginVertical("box");
        }
        EditorGUILayout.EndVertical();

        dialogNode.isMonologue = EditorGUILayout.Toggle("Is Monologue", dialogNode.isMonologue);

        if (dialogNode.isMonologue)
        {
            dialogNode.nextNodeID = EditorGUILayout.IntField("Next Node ID", dialogNode.nextNodeID);
            // Disable choices
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField("Choices (Disabled for Monologues)");
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            // Allow editing choices
            if (GUILayout.Button("Add Choice"))
            {
                dialogNode.AddChoice(new Choice()); // Example of adding a new choice
            }

            for (int j=0; j< dialogNode.choices.Count;j++)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField((j + 1) + ".Choice");
                dialogNode.choices[j].choiceText = EditorGUILayout.TextField("Choice Text", dialogNode.choices[j].choiceText);
                dialogNode.choices[j].nextNodeID = EditorGUILayout.IntField("Next Node ID", dialogNode.choices[j].nextNodeID);
                if (GUILayout.Button("Delete Choice"))
                {
                    dialogNode.RemoveChoice(dialogNode.choices[j]);
                }
                EditorGUILayout.EndVertical();
            }
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
    public void AddTextSegment(DialogNode dialogNode)
    {
        // Create a new TextSegment instance
        TextSegment newSegment = CreateInstance<TextSegment>();
        newSegment.segmentId = dialogNode.npcTextSegments.Count;

        dialogNode.AddTextSegment(newSegment);

        // Get the path to the current DialogData asset to determine the folder for TextSegments
        string dialogDataPath = AssetDatabase.GetAssetPath(dialogNode); // Assuming dialogData is the main asset
        string folderPath = Path.GetDirectoryName(dialogDataPath); // This is now a valid path to the asset's directory

        // Define the TextSegments subfolder path
        string segmentsFolderPath = Path.Combine(folderPath, "TextSegments");

        // Check if TextSegments folder exists, create if it doesn't
        if (!AssetDatabase.IsValidFolder(segmentsFolderPath))
        {
            AssetDatabase.CreateFolder(folderPath, "TextSegments");
        }

        // Define the asset path for the new segment
        string segmentAssetPath = Path.Combine(segmentsFolderPath, $"{dialogNode.name}_TextSegment_{newSegment.segmentId}.asset");

        // Create the asset for the new segment in the TextSegments folder
        AssetDatabase.CreateAsset(newSegment, segmentAssetPath.Replace("\\", "/")); // Replace backslashes for cross-platform compatibility
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
