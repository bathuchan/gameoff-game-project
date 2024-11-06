using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogTextSegment", menuName = "DialogObjects/DialogTextSegment")]
[System.Serializable]
public class TextSegment : ScriptableObject
{
    public int segmentId;
    public string text; // The actual text content
    public Color color = Color.black; // Text color
    public bool bold;
    public bool italic;
    public float sizeMultiplier = 1f; // Font size multiplier
    public TMP_FontAsset font; // Specific font for this segment
    [HideInInspector] public bool showDetails = false;
    public bool animateSegment=false;
}
