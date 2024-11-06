using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class DialogLayoutManager : MonoBehaviour
{
    public float maxLineWidth = 400f; // Adjust as needed for line width
    public GameObject segmentPrefab; // Prefab with a TextMeshProUGUI component
    public List<TextSegment> textSegments; // Segments to display

    private void Start()
    {
        ArrangeSegments();
    }

    private void ArrangeSegments()
    {
        float currentLineWidth = 0;
        float currentYOffset = 0;
        float lineHeight = 0; // Track the tallest segment on the current line

        foreach (TextSegment segment in textSegments)
        {
            // Instantiate each segment as a separate TextMeshProUGUI object
            GameObject segmentObject = Instantiate(segmentPrefab, transform);
            TextMeshProUGUI segmentText = segmentObject.GetComponent<TextMeshProUGUI>();

            // Apply the segment's text, font, color, etc.
            segmentText.text = segment.text;
            segmentText.color = segment.color;
            segmentText.font = segment.font ?? segmentText.font;
            segmentText.fontSize *= segment.sizeMultiplier;
            segmentText.fontStyle = (segment.bold ? FontStyles.Bold : FontStyles.Normal) |
                                    (segment.italic ? FontStyles.Italic : FontStyles.Normal);

            // Measure the width and height of the formatted segment
            segmentText.ForceMeshUpdate(); // Ensure the layout is updated for accurate measurements
            float segmentWidth = segmentText.preferredWidth;
            float segmentHeight = segmentText.preferredHeight;

            // Check if adding this segment would exceed the max line width
            if (currentLineWidth + segmentWidth > maxLineWidth)
            {
                // Move to the next line
                currentYOffset -= lineHeight; // Move down by the line height
                currentLineWidth = 0; // Reset the line width
                lineHeight = 0; // Reset line height for the new line
            }

            // Position the segment
            segmentObject.transform.localPosition = new Vector3(currentLineWidth, currentYOffset, 0);

            // Update line width and line height
            currentLineWidth += segmentWidth;
            lineHeight = Mathf.Max(lineHeight, segmentHeight); // Ensure adequate spacing for tallest element

            // Optional: Add some spacing between segments
            currentLineWidth += 10f; // Adjust spacing as needed
        }
    }
}
