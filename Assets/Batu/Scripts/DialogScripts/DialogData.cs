using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogData", menuName = "DialogObjects/DialogData")]
public class DialogData : ScriptableObject
{
    public List<DialogNode> dialogNodes = new List<DialogNode>();

    // Method to add a new DialogNode
    public void AddDialogNode(DialogNode newNode)
    {
        dialogNodes.Add(newNode);
    }

    // Method to remove a DialogNode
    public void RemoveDialogNode(DialogNode nodeToRemove)
    {
        dialogNodes.Remove(nodeToRemove);
    }
}
