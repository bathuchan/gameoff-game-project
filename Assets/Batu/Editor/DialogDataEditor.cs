using System.IO;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

[CustomEditor(typeof(DialogData))]
public class DialogDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AssetDatabase.Refresh();
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
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

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
                    EditorGUILayout.LabelField("Node ID: "+ node.nodeID);
                    node.npcText = EditorGUILayout.TextField("NPC Text", node.npcText);
                    node.isMonologue = EditorGUILayout.Toggle("Is Monologue", node.isMonologue);

                    node.typingSpeed = EditorGUILayout.Slider("Type Effect Speed",node.typingSpeed,0.01f,1f);
                    EditorGUILayout.HelpBox("Type effect works faster if the value is lower",MessageType.Info);
                    EditorGUILayout.BeginVertical("box");
                    if (node.isMonologue == true)
                    {
                        
                        node.nextNodeID = EditorGUILayout.IntField("Next Node ID", node.nextNodeID);
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.LabelField("Choices (Disabled for Monologues)");
                        EditorGUI.EndDisabledGroup();
                        node.willEndConversation=EditorGUILayout.Toggle("This will end the conversation:",node.willEndConversation);
                    }else if (node.showChoices = EditorGUILayout.Foldout(node.showChoices, "Choices:")) 
                    {
                        node.willEndConversation=false;
                        if (GUILayout.Button("Add Choice"))
                        {
                            node.AddChoice(new Choice());

                        }
                        for (int k = 0; k < node.choices.Count; k++)
                        {
                            EditorGUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField((k + 1) + ".Choice");
                            node.choices[k].choiceText = EditorGUILayout.TextField("Choice Text", node.choices[k].choiceText);
                            node.choices[k].typingSpeed = EditorGUILayout.Slider("Type Effect Speed", node.choices[k].typingSpeed, 0.01f, 1f);
                            node.choices[k].nextNodeID = EditorGUILayout.IntField("Next Node ID", node.choices[k].nextNodeID);
                            if (GUILayout.Button("Delete Choice"))
                            {
                                node.RemoveChoice(node.choices[k]);
                            }
                            EditorGUILayout.EndVertical();
                        }

                    }

                    if (node.showChoices)
                    {
                    }
                    EditorGUILayout.EndVertical();
                    if (GUILayout.Button("Delete Node Asset"))
                    {
                        dialogData.RemoveDialogNode(node);
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(node)); // Deletion of a Node
                        AssetDatabase.Refresh();
                    }
                }

                
                EditorGUILayout.EndVertical();

            }
            EditorGUILayout.EndVertical();

            // Mark the target as dirty if any changes are made
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}
