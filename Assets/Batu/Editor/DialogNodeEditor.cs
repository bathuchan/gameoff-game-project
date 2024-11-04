using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogNode))]
public class DialogNodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DialogNode dialogNode = (DialogNode)target;

        EditorGUILayout.LabelField("Node ID:"+ dialogNode.nodeID);
        dialogNode.npcText = EditorGUILayout.TextField("NPC Text", dialogNode.npcText);
        dialogNode.isMonologue = EditorGUILayout.Toggle("Is Monologue", dialogNode.isMonologue);
        dialogNode.typingSpeed = EditorGUILayout.Slider("Type Effect Speed", dialogNode.typingSpeed, 0.01f, 1f);
        EditorGUILayout.HelpBox("Type effect works faster if the value is lower", MessageType.Info);

        if (dialogNode.isMonologue)
        {
            dialogNode.nextNodeID = EditorGUILayout.IntField("Next Node ID", dialogNode.nextNodeID);
            dialogNode.willEndConversation = EditorGUILayout.Toggle("This will end the conversation:", dialogNode.willEndConversation);
            // Disable choices
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField("Choices (Disabled for Monologues)");
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            dialogNode.willEndConversation=false;
            // Allow editing choices
            if (GUILayout.Button("Add Choice"))
            {
                dialogNode.AddChoice(new Choice()); // Example of adding a new choice
            }

            if (dialogNode.choices!=null)
            {
                EditorGUILayout.BeginVertical("box");
                GUILayout.Label("CHOICES");
                int i = dialogNode.choices.Count;
                foreach (Choice choice in dialogNode.choices)
                {
                    EditorGUILayout.BeginVertical("box");
                    GUILayout.Label((i-dialogNode.choices.Count+1)+". Choice ");
                    i++;
                    choice.choiceText = EditorGUILayout.TextField("Choice Text", choice.choiceText);
                    choice.typingSpeed = EditorGUILayout.Slider("Type Effect Speed", choice.typingSpeed, 0.01f, 1f);
                    choice.nextNodeID = EditorGUILayout.IntField("Next Node ID", choice.nextNodeID);
                    if (GUILayout.Button("Remove Choice"))
                    {
                        dialogNode.RemoveChoice(choice); // Example of adding a new choice
                    }
                    EditorGUILayout.EndVertical();
                    if (GUI.changed)
                    {
                        EditorUtility.SetDirty(target);
                    }
                }
                EditorGUILayout.EndVertical();
            }
            
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
