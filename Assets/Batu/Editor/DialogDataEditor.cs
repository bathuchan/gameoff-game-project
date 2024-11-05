using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogData))]
public class DialogDataEditor : Editor
{
    private SerializedObject dialogDataSerialized;

    private void OnEnable()
    {
        dialogDataSerialized = new SerializedObject(target);
    }

    public override void OnInspectorGUI()
    {
        
        dialogDataSerialized.Update();
        DialogData dialogData = (DialogData)target;

        // Add New Dialog Node Button
        if (GUILayout.Button("Add New Dialog Node"))
        {
            CreateAndAddDialogNode(dialogData);
        }

        EditorGUI.BeginDisabledGroup(dialogData.dialogNodes.Count == 0);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("DIALOG NODES");

        for (int i = 0; i < dialogData.dialogNodes.Count; i++)
        {
            DialogNode node = dialogData.dialogNodes[i];
            DrawNode(node, dialogData);
        }

        EditorGUILayout.EndVertical();
        EditorGUI.EndDisabledGroup();

        // Apply all changes at once
        if (dialogDataSerialized.ApplyModifiedProperties())
        {
            MarkDirty(dialogData);
        }
    }

    private void CreateAndAddDialogNode(DialogData dialogData)
    {
        DialogNode newNode = CreateInstance<DialogNode>();
        newNode.nodeID = dialogData.dialogNodes.Count;
        dialogData.AddDialogNode(newNode);

        string dialogDataPath = AssetDatabase.GetAssetPath(dialogData);
        string folderPath = Path.GetDirectoryName(dialogDataPath);
        string nodesFolderPath = Path.Combine(folderPath, "Nodes");

        if (!AssetDatabase.IsValidFolder(nodesFolderPath))
        {
            AssetDatabase.CreateFolder(folderPath, "Nodes");
        }

        string nodeAssetPath = Path.Combine(nodesFolderPath, $"{dialogData.name}_DialogNode_{newNode.nodeID}.asset");
        AssetDatabase.CreateAsset(newNode, nodeAssetPath.Replace("\\", "/"));
        MarkDirty(newNode, dialogData);
    }

    private void DrawNode(DialogNode node, DialogData dialogData)
    {
        EditorGUILayout.BeginVertical("box");

        node.showDetails = EditorGUILayout.Foldout(node.showDetails, "Dialog Node: " + node.nodeID);


        if (node.showDetails)
        {
            DrawNodeDetails(node);

            DrawTextSegments(node);
            
            DrawChoices(node, dialogData);
        }

        if (GUILayout.Button("Delete Node Asset: " + node.nodeID))
        {
            DeleteNodeAsset(node, dialogData);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawNodeDetails(DialogNode node)
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Node ID: " + node.nodeID);
        if(node.showSettings = EditorGUILayout.Foldout(node.showSettings, "Node Settings"))
        {
            DrawNodeSettings(node);
            node.typingSpeed = EditorGUILayout.Slider("Type Effect Duration", node.typingSpeed, 0.01f, 1f);
            EditorGUILayout.HelpBox("Type effect works faster if the value is lower", MessageType.Info);
        }
        EditorGUILayout.EndVertical();

    }

    private void DrawTextSegments(DialogNode node)
    {
        if (GUILayout.Button("Add New Text Segment"))
        {
            TextSegment newSegment = CreateInstance<TextSegment>();
            newSegment.segmentId = node.npcTextSegments.Count;
            node.AddTextSegment(newSegment);

            string nodePath = AssetDatabase.GetAssetPath(node);
            string folderPath = Path.GetDirectoryName(nodePath);
            string segmentsFolderPath = Path.Combine(folderPath, "TextSegments");

            if (!AssetDatabase.IsValidFolder(segmentsFolderPath))
            {
                AssetDatabase.CreateFolder(folderPath, "TextSegments");
            }

            string segmentAssetPath = Path.Combine(segmentsFolderPath, $"{node.name}_TextSegment_{newSegment.segmentId}.asset");
            AssetDatabase.CreateAsset(newSegment, segmentAssetPath.Replace("\\", "/"));
            MarkDirty(newSegment, node);
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
                        if (!node.useFixedColor) 
                        {
                            currentTextSegment.color = EditorGUILayout.ColorField("Text Color: ", currentTextSegment.color);
                        }
                        else 
                        {
                            EditorGUI.BeginDisabledGroup(node.useFixedColor);
                            EditorGUILayout.LabelField("Fixed Color is ON");
                            EditorGUI.EndDisabledGroup();
                        }
                        

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
                        EditorUtility.SetDirty(currentTextSegment);
                        node.RemoveTextSegment(currentTextSegment);
                        
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(currentTextSegment)); // Deletion of a Node
                        AssetDatabase.Refresh();
                    }
                    EditorGUILayout.EndVertical();
                }
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical("box");
        
    }

    private void DrawNodeSettings(DialogNode node)
    {
        EditorGUILayout.BeginVertical("box");
        node.isMonologue = EditorGUILayout.Toggle("Is Monologue", node.isMonologue);

        if (node.isMonologue == true)
        {
            node.nextNodeID = EditorGUILayout.IntField("Next Node ID", node.nextNodeID);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField("Choices (Disabled for Monologues)");
            EditorGUI.EndDisabledGroup();
            node.willEndConversation = EditorGUILayout.Toggle("This will end the conversation:", node.willEndConversation);
        }
        else if (node.isMonologue == false && node.choices.Count == 0)
        {
            EditorGUILayout.HelpBox("You Didn't add any choice this way game will softlock itself", MessageType.Warning);
        }
        EditorGUILayout.EndVertical();
        node.useFixedFont = EditorGUILayout.Toggle("Use Fixed Font", node.useFixedFont);
        node.useFixedColor = EditorGUILayout.Toggle("Use Fixed Font Color", node.useFixedColor);

        if (node.useFixedFont)
        {
            node.fixedFontAsset = (TMP_FontAsset)EditorGUILayout.ObjectField("Fixed Font Asset", node.fixedFontAsset, typeof(TMP_FontAsset), false);
        }

        if (node.useFixedColor)
        {
            node.fixedColor = EditorGUILayout.ColorField("Fixed Text Color: ", node.fixedColor);
        }
    }

    private void DrawChoices(DialogNode node, DialogData dialogData)
    {
        if (!node.isMonologue) 
        {
            if (GUILayout.Button("Add Choice"))
            {
                node.AddChoice(new Choice());
                MarkDirty(dialogData);
            }

            EditorGUILayout.BeginVertical("box");
            if (node.showChoices=EditorGUILayout.Foldout(node.showChoices,"Choices"+"("+node.choices.Count+")")) 
            {
                for (int i = 0; i < node.choices.Count; i++)
                {
                    Choice choice = node.choices[i];
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField((i + 1) + ". Choices");
                    choice.choiceText = EditorGUILayout.TextField("Choice Text", choice.choiceText);
                    choice.typingSpeed = EditorGUILayout.Slider("Type Effect Speed", choice.typingSpeed, 0.01f, 1f);
                    choice.nextNodeID = EditorGUILayout.IntField("Next Node ID", choice.nextNodeID);

                    if (GUILayout.Button("Delete Choice"))
                    {
                        node.RemoveChoice(choice);
                        MarkDirty(dialogData, node);
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndVertical();


        }
        else 
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField("Choices (Disabled for Monologues)");
            EditorGUI.EndDisabledGroup();
        }
       
        //EditorGUI.EndDisabledGroup();
    }

    private void DeleteNodeAsset(DialogNode node, DialogData dialogData)
    {
        node.RemoveAllTextSegments();
        dialogData.RemoveDialogNode(node);
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(node));
        MarkDirty(dialogData);
    }

    private void MarkDirty(params Object[] objs)
    {
        foreach (var obj in objs)
        {
            EditorUtility.SetDirty(obj);
        }
        AssetDatabase.SaveAssets();
    }
}
