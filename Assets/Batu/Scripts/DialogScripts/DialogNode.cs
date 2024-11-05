using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogNode", menuName = "DialogObjects/DialogNode")]
[System.Serializable]
public class DialogNode : ScriptableObject
{
    public int nodeID;               // Unique identifier for this node
    public List<TextSegment> npcTextSegments; // A list of customizable text segments
    public bool isMonologue;         // Is this node a monologue?
    public int nextNodeID;           // Next node ID for sequential dialog (only used if isMonologue is true)
    public float typingSpeed = 0.05f;
    public List<Choice> choices;     // Choices available at this node
    [HideInInspector] public bool showDetails = false;
    [HideInInspector] public bool showChoices=false;
    [HideInInspector] public bool showTexts = false;
    public bool willEndConversation = false;
    [HideInInspector] public bool useFixedFont = false; // Toggle for using a fixed font
    [HideInInspector] public TMP_FontAsset fixedFontAsset; // Fixed font asset field
    [HideInInspector] public bool useFixedColor = false;
    [HideInInspector] public Color fixedColor = Color.black;

    public DialogNode()
    {
        choices = new List<Choice>();
        npcTextSegments = new List<TextSegment>();
    }
    public void AddTextSegment(TextSegment newSegment)
    {
        npcTextSegments.Add(newSegment);
    }
    public void RemoveTextSegment(TextSegment newSegment)
    {
        npcTextSegments.Remove(newSegment);
    }
    public void RemoveAllTextSegments()
    {
        foreach (TextSegment segment in npcTextSegments) 
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(segment)); // Deletion of a Node
            
        }
        //AssetDatabase.Refresh();
        npcTextSegments.Clear();
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

    public void ChangeAllFont() 
    {
        foreach (TextSegment segment in npcTextSegments) 
        {
            segment.font = fixedFontAsset;
        }
    }

    public void ChangeAllColor()
    {
        foreach (TextSegment segment in npcTextSegments)
        {
            segment.color = fixedColor;
        }
    }




}
