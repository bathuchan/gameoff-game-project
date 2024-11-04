using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(DialogNode))]
public class DialogNodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DialogNode dialogNode = (DialogNode)target;

        dialogNode.nodeID = EditorGUILayout.IntField("Node ID", dialogNode.nodeID);
        dialogNode.npcText = EditorGUILayout.TextField("NPC Text", dialogNode.npcText);
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

            foreach (Choice choice in dialogNode.choices)
            {
                EditorGUILayout.BeginVertical("box");
                choice.choiceText = EditorGUILayout.TextField("Choice Text", choice.choiceText);
                choice.nextNodeID = EditorGUILayout.IntField("Next Node ID", choice.nextNodeID);
                EditorGUILayout.EndVertical();
            }
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
