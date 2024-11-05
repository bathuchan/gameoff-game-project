using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogData))]
public class DialogDataEditor : Editor
{
    

    public override void OnInspectorGUI()
    {
        DialogData dialogData = (DialogData)target;

        if (GUILayout.Button("Add New Dialog Node"))
        {
            // Create a new DialogNode instance
            DialogNode newNode = CreateInstance<DialogNode>();
            newNode.nodeID = dialogData.dialogNodes.Count; // Assign a unique ID
            dialogData.AddDialogNode(newNode);

            // Get the path to the current DialogData asset
            string dialogDataPath = AssetDatabase.GetAssetPath(dialogData);

            // Get the folder path for the DialogData asset
            string folderPath = Path.GetDirectoryName(dialogDataPath);

            // Define the Nodes subfolder path
            string nodesFolderPath = Path.Combine(folderPath, "Nodes");

            // Check if Nodes folder exists, create if it doesn't
            if (!AssetDatabase.IsValidFolder(nodesFolderPath))
            {
                AssetDatabase.CreateFolder(folderPath, "Nodes");
            }

            // Create the asset for the new node in the Nodes folder
            string nodeAssetPath = Path.Combine(nodesFolderPath, $"{dialogData.name}_DialogNode_{newNode.nodeID}.asset");
            AssetDatabase.CreateAsset(newNode, nodeAssetPath.Replace("\\", "/")); // Replace backslashes for cross-platform compatibility
            EditorUtility.SetDirty(newNode);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // Display global settings for Fixed Font

        EditorGUI.BeginDisabledGroup(dialogData.dialogNodes.Count == 0);
        // Display each existing DialogNode with options to edit or remove
        if (dialogData.dialogNodes != null)
        {
            
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.LabelField("DIALOG NODES");

            for (int i = 0; i < dialogData.dialogNodes.Count; i++)
            {
                DialogNode node = dialogData.dialogNodes[i];

                EditorGUILayout.BeginVertical("box");

                node.showDetails = EditorGUILayout.Foldout(node.showDetails, "Dialog Node:" + node.nodeID);

                if (node.showDetails)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("Node ID: " + node.nodeID);

                    if (GUILayout.Button("Add New Text Segment"))
                    {
                        // Create a new TextSegment instance
                        TextSegment newSegment = CreateInstance<TextSegment>();
                        newSegment.segmentId = node.npcTextSegments.Count;

                        node.AddTextSegment(newSegment);

                        // Get the path to the current DialogData asset to determine the folder for TextSegments
                        string dialogDataPath = AssetDatabase.GetAssetPath(node); // Assuming dialogData is the main asset
                        string folderPath = Path.GetDirectoryName(dialogDataPath); // This is now a valid path to the asset's directory

                        // Define the TextSegments subfolder path
                        string segmentsFolderPath = Path.Combine(folderPath, "TextSegments");

                        // Check if TextSegments folder exists, create if it doesn't
                        if (!AssetDatabase.IsValidFolder(segmentsFolderPath))
                        {
                            AssetDatabase.CreateFolder(folderPath, "TextSegments");
                        }

                        // Define the asset path for the new segment
                        string segmentAssetPath = Path.Combine(segmentsFolderPath, $"{node.name}_TextSegment_{newSegment.segmentId}.asset");

                        // Create the asset for the new segment in the TextSegments folder
                        AssetDatabase.CreateAsset(newSegment, segmentAssetPath.Replace("\\", "/")); // Replace backslashes for cross-platform compatibility
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }

                    if (node.npcTextSegments.Count != 0)
                    {
                        if (node.showTexts = EditorGUILayout.Foldout(node.showTexts, "NPC Text Segments"))
                        {
                            for (int j = 0; j < node.npcTextSegments.Count; j++)
                            {
                                EditorGUILayout.BeginVertical("box");
                                TextSegment currentTextSegment = node.npcTextSegments[j];
                                EditorGUILayout.LabelField("Segment ID: " + currentTextSegment.segmentId);
                                currentTextSegment.text = EditorGUILayout.TextField("NPC Text", currentTextSegment.text);

                                if (currentTextSegment.showDetails = EditorGUILayout.Foldout(currentTextSegment.showDetails, "Customize Text:"))
                                {
                                    EditorGUI.BeginDisabledGroup(node.useFixedColor);
                                    currentTextSegment.color = EditorGUILayout.ColorField("Text Color: ", currentTextSegment.color);
                                    if(node.useFixedColor)
                                        EditorGUILayout.LabelField("Fixed Color is ON");
                                    EditorGUI.EndDisabledGroup();
                                    
                                    currentTextSegment.bold = EditorGUILayout.Toggle("Bold", currentTextSegment.bold);
                                    currentTextSegment.italic = EditorGUILayout.Toggle("Italic", currentTextSegment.italic);
                                    currentTextSegment.sizeMultiplier = EditorGUILayout.FloatField("Font Size Multiplier", currentTextSegment.sizeMultiplier);

                                    // Disable font selection if useFixedFont is true
                                    EditorGUI.BeginDisabledGroup(node.useFixedFont);
                                    currentTextSegment.font = (TMP_FontAsset)EditorGUILayout.ObjectField("Text Segment Font", currentTextSegment.font, typeof(TMP_FontAsset), false);
                                    if (node.useFixedFont)
                                        EditorGUILayout.LabelField("Fixed Font is ON");
                                    EditorGUI.EndDisabledGroup();
                                    
                                }

                                if (GUILayout.Button("Delete Text Segment: " + currentTextSegment.segmentId))
                                {
                                    node.RemoveTextSegment(currentTextSegment);
                                    EditorUtility.SetDirty(currentTextSegment);
                                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(currentTextSegment)); // Deletion of a Node
                                    AssetDatabase.Refresh();
                                }
                                EditorGUILayout.EndVertical();
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("Node Settings", EditorStyles.boldLabel);
                    node.isMonologue = EditorGUILayout.Toggle("Is Monologue", node.isMonologue);
                    if (node.isMonologue == true)
                    {
                        node.nextNodeID = EditorGUILayout.IntField("Next Node ID", node.nextNodeID);
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.LabelField("Choices (Disabled for Monologues)");
                        EditorGUI.EndDisabledGroup();
                        node.willEndConversation = EditorGUILayout.Toggle("This will end the conversation:", node.willEndConversation);
                    }
                    else if(node.isMonologue == false && node.choices.Count==0)
                    {
                        EditorGUILayout.HelpBox("You Didn't add any choice this way game will softlock itself", MessageType.Warning);
                    }
                        node.useFixedFont = EditorGUILayout.Toggle("Use Fixed Font", node.useFixedFont);

                    if (node.useFixedFont)
                    {
                        node.fixedFontAsset= (TMP_FontAsset)EditorGUILayout.ObjectField("Fixed Font Asset", node.fixedFontAsset, typeof(TMP_FontAsset), false);
                        //node.ChangeAllFont();
                    }

                    node.useFixedColor = EditorGUILayout.Toggle("Use Fixed Font Color", node.useFixedColor);
                    if (node.useFixedColor)
                    {
                        node.fixedColor = EditorGUILayout.ColorField("Fixed Text Color: ", node.fixedColor);
                        //node.ChangeAllColor();

                    }

                    node.typingSpeed = EditorGUILayout.Slider("Type Effect Duration", node.typingSpeed, 0.01f, 1f);
                    EditorGUILayout.HelpBox("Type effect works faster if the value is lower", MessageType.Info);
                    EditorGUILayout.EndVertical();

                    EditorGUI.BeginDisabledGroup(node.isMonologue);
                    EditorGUILayout.BeginVertical("box");
                     if (node.showChoices = EditorGUILayout.Foldout(node.showChoices, "Choices:"))
                    {
                        node.willEndConversation = false;
                        if (GUILayout.Button("Add Choice"))
                        {
                            node.AddChoice(new Choice());
                            EditorUtility.SetDirty(dialogData);
                        }

                        for (int k = 0; k < node.choices.Count; k++)
                        {
                            EditorGUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField((k + 1) + ". Choice");
                            node.choices[k].choiceText = EditorGUILayout.TextField("Choice Text", node.choices[k].choiceText);
                            node.choices[k].typingSpeed = EditorGUILayout.Slider("Type Effect Speed", node.choices[k].typingSpeed, 0.01f, 1f);
                            node.choices[k].nextNodeID = EditorGUILayout.IntField("Next Node ID", node.choices[k].nextNodeID);
                            if (GUILayout.Button("Delete Choice"))
                            {
                                node.RemoveChoice(node.choices[k]);
                                EditorUtility.SetDirty(dialogData); // Ensure the main dialogData object is marked dirty
                                EditorUtility.SetDirty(node);
                                EditorUtility.SetDirty(target);

                                AssetDatabase.SaveAssets(); // Force save to disk
                            }
                            EditorGUILayout.EndVertical();
                        }
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUI.EndDisabledGroup();
                }
                if (GUILayout.Button("Delete Node Asset: " + node.nodeID))
                {
                    node.RemoveAllTextSegments();
                    dialogData.RemoveDialogNode(node);
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(node)); // Deletion of a Node
                    EditorUtility.SetDirty(dialogData); // Ensure the main dialogData object is marked dirty
                    EditorUtility.SetDirty(target);

                    AssetDatabase.SaveAssets(); // Force save to disk
                    AssetDatabase.Refresh();
                }

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            // Mark the target as dirty if any changes are made
            if (GUI.changed)
            {
                EditorUtility.SetDirty(dialogData); // Ensure the main dialogData object is marked dirty
                EditorUtility.SetDirty(target);
                
                AssetDatabase.SaveAssets(); // Force save to disk
            }
        }
        EditorGUI.EndDisabledGroup();
    }
}
