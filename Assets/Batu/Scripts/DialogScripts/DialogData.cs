using System.Collections.Generic;
using UnityEditor;
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
    private void OnDestroy()
    {
        // Delete all child DialogNode assets
        foreach (var node in dialogNodes)
        {
            // Delete all TextSegment assets associated with each DialogNode
            foreach (var textSegment in node.npcTextSegments)
            {
                string textSegmentPath = AssetDatabase.GetAssetPath(textSegment);
                if (!string.IsNullOrEmpty(textSegmentPath))
                {
                    AssetDatabase.DeleteAsset(textSegmentPath);
                }
            }

            // Delete the DialogNode asset itself
            string nodePath = AssetDatabase.GetAssetPath(node);
            if (!string.IsNullOrEmpty(nodePath))
            {
                AssetDatabase.DeleteAsset(nodePath);
            }
        }

        // Clear the dialogNodes list (optional, as the asset will be deleted)
        dialogNodes.Clear();

        // Save changes to the Asset Database
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
