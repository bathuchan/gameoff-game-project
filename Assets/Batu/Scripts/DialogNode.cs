using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogNode", menuName = "DialogObjects/DialogNode")]
[System.Serializable]
public class DialogNode : ScriptableObject
{
    public int nodeID;               // Unique identifier for this node
    public string npcText;           // Text displayed by the NPC
    public bool isMonologue;         // Is this node a monologue?
    public int nextNodeID;           // Next node ID for sequential dialog (only used if isMonologue is true)
    public float typingSpeed = 0.05f;
    public List<Choice> choices;     // Choices available at this node
    [HideInInspector] public bool showDetails = false;
    [HideInInspector] public bool showChoices=false;
    public bool willEndConversation = false;

    public DialogNode()
    {
        choices = new List<Choice>();
    }

    public void AddChoice(Choice choice)
    {
        if (!isMonologue)
        {
            choices.Add(choice); // Only add choices if it's not a monologue
            
        }
        else
        {
            Debug.LogWarning("Cannot add choices to a monologue node.");
        }
    }

    public void RemoveChoice(Choice choice)
    {
        if (!isMonologue)
        {
            choices.Remove(choice); // Only add choices if it's not a monologue
        }
        else
        {
            Debug.LogWarning("Cannot add choices to a monologue node.");
        }
    }

    


}
