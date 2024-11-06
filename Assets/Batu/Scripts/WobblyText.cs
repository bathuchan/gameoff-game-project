using System.Collections;
using UnityEngine;
using TMPro;

public class WobblyText : MonoBehaviour
{
    public float amplitude = 5f;       // The distance of wobble (higher values make the characters move more)
    public float frequency = 2f;       // The speed of wobble
    public float rotationAmount = 5f;  // The rotation angle in degrees for each character

    private TMP_Text textMesh;
    private TMP_TextInfo textInfo;

    void Start()
    {
        textMesh = GetComponent<TMP_Text>();
        textInfo = textMesh.textInfo;
    }

    void Update()
    {
        textMesh.ForceMeshUpdate();
        textInfo = textMesh.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible)
                continue;

            // Get the vertices of the character at index i
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[textInfo.characterInfo[i].materialReferenceIndex].vertices;

            // Calculate the wave offset based on the character's index and time
            float offset = Mathf.Sin(Time.time * frequency + i * 0.5f) * amplitude;

            // Apply the wobble effect by offsetting each vertex position
            Vector3 wobbleOffset = new Vector3(0, offset, 0);
            vertices[vertexIndex + 0] += wobbleOffset;
            vertices[vertexIndex + 1] += wobbleOffset;
            vertices[vertexIndex + 2] += wobbleOffset;
            vertices[vertexIndex + 3] += wobbleOffset;

            // Apply rotation effect to each character
            float rotation = Mathf.Sin(Time.time * frequency + i * 0.5f) * rotationAmount;
            Quaternion rotationQuat = Quaternion.Euler(0, 0, rotation);
            Vector3 charCenter = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2;

            for (int j = 0; j < 4; j++)
            {
                vertices[vertexIndex + j] = rotationQuat * (vertices[vertexIndex + j] - charCenter) + charCenter;
            }
        }

        // Update the mesh with the new vertex positions
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textMesh.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}
