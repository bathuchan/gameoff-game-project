using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogSystem : MonoBehaviour
{
    public DialogData dialogData;
    public TextMeshProUGUI npcTextUI;
    public Button nextButton;
    public Transform choicesContainer;
    public Button choiceButtonPrefab;
    public GameObject dialogUI;

    private DialogNode currentNode;
    private Coroutine typingCoroutine;
    //private bool isTyping;
    //private bool isDisplayingChoices;

    private void Start()
    {
        StartDialog(0);
    }

    public void StartDialog(int startNodeID)
    {
        dialogUI.SetActive(true);
        currentNode = FindNodeByID(startNodeID);
        if (currentNode != null)
        {
            DisplayNode(currentNode);
        }
        else
        {
            Debug.LogError("Dialog node with ID " + startNodeID + " not found.");
        }
    }

    private void DisplayNode(DialogNode node)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        //isTyping = true;
        typingCoroutine = StartCoroutine(TypeText(node.npcTextSegments, node.typingSpeed));
    }

    private IEnumerator TypeText(List<TextSegment> textSegments, float typingSpeed)
    {
        npcTextUI.text = ""; // Clear the text field
        nextButton.gameObject.SetActive(true);
        nextButton.onClick.RemoveAllListeners();

        foreach (var segment in textSegments)
        {
            // Get the formatted text for the current segment
            string formattedText = ApplyFormatting(segment);

            int i = 0;
            while (i < formattedText.Length)
            {
                if (formattedText[i] == '<')
                {
                    int closingBracketIndex = formattedText.IndexOf('>', i);
                    if (closingBracketIndex != -1)
                    {
                        // Append the entire tag immediately
                        npcTextUI.text += formattedText.Substring(i, closingBracketIndex - i + 1);
                        i = closingBracketIndex + 1;
                        continue;
                    }
                }

                npcTextUI.text += formattedText[i];
                i++;
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        //isTyping = false;
        UpdateChoicesUI(currentNode);
    }

    // Method to apply formatting based on TextSegment properties
    private string ApplyFormatting(TextSegment segment)
    {
        string text = segment.text;

        // Apply size multiplier if different from 1
        if (segment.sizeMultiplier != 1f)
        {
            text = $"<size={(int)(npcTextUI.fontSize * segment.sizeMultiplier)}>{text}</size>";
        }

        // Apply color if specified
        Color color;
        string colorHex;
        if (currentNode.useFixedColor)
        {
            color = currentNode.fixedColor;
            colorHex = ColorUtility.ToHtmlStringRGB(color);

        }
        else
        {
            color = segment.color;
            colorHex = ColorUtility.ToHtmlStringRGB(color);

        }

        text = $"<color=#{colorHex}>{text}</color>";
        // Apply bold and italic if true
        if (segment.bold)
        {
            text = $"<b>{text}</b>";
        }

        if (segment.italic)
        {
            text = $"<i>{text}</i>";
        }

        // Apply font if a custom font is assigned
        if (currentNode.useFixedFont && currentNode.fixedFontAsset != null)
        {
            text = $"<font=\"{segment.font.name}\">{text}</font>";
        }
        else
        {
            if (segment.font == null)
                segment.font = npcTextUI.GetComponent<TMP_Text>().font;

            text = $"<font=\"{segment.font.name}\">{text}</font>";
        }

        return text;
    }


    // Helper function to build the full text from segments instantly
    private string BuildFullText(List<TextSegment> segments)
    {
        string fullText = "";
        foreach (var segment in segments)
        {
            fullText += BuildRichText(segment);
        }
        return fullText;
    }

    // Helper function to wrap text in Rich Text tags
    private string BuildRichText(TextSegment segment)
    {
        string styledText = segment.text;

        if (segment.bold) styledText = "<b>" + styledText + "</b>";
        if (segment.italic) styledText = "<i>" + styledText + "</i>";
        styledText = $"<color=#{ColorUtility.ToHtmlStringRGB(segment.color)}>" + styledText + "</color>";
        styledText = $"<size={(int)(npcTextUI.fontSize * segment.sizeMultiplier)}>" + styledText + "</size>";

        return styledText;
    }

    private void UpdateChoicesUI(DialogNode node)
    {
        // Clear any existing choices before updating
        foreach (Transform child in choicesContainer)
        {
            Destroy(child.gameObject);
        }

        if (node.isMonologue)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() =>
            {
                if (node.willEndConversation)
                {
                    dialogUI.SetActive(false);
                }
                else
                {
                    StartDialog(node.nextNodeID);
                }
            });
            choicesContainer.gameObject.SetActive(false);
        }
        else
        {
            nextButton.gameObject.SetActive(true);
            choicesContainer.gameObject.SetActive(true);
            //isDisplayingChoices = true;

            // Start coroutine to display choices but disable interaction until all choices are shown
            StartCoroutine(DisplayAllChoicesSequentially(node));
        }
    }

    private IEnumerator DisplayAllChoicesSequentially(DialogNode node)
    {
        List<Button> choiceButtons = new List<Button>();

        foreach (Choice choice in node.choices)
        {
            Button button = Instantiate(choiceButtonPrefab, choicesContainer);
            button.interactable = false;
            TextMeshProUGUI choiceTextUI = button.GetComponentInChildren<TextMeshProUGUI>();
            choiceTextUI.text = "";
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnChoiceSelected(choice));
            button.gameObject.SetActive(false);

            choiceButtons.Add(button);

            yield return StartCoroutine(TypeChoiceText(choice.choiceText, choiceTextUI, button.gameObject, choice.typingSpeed));
            yield return new WaitForSeconds(0.3f);
        }

        // Enable choices after all are displayed
        foreach (Button choiceButton in choiceButtons)
        {
            choiceButton.interactable = true;
        }

        nextButton.gameObject.SetActive(false);
        //isDisplayingChoices = false;
    }

    private IEnumerator TypeChoiceText(string choiceText, TextMeshProUGUI choiceTextUI, GameObject buttonGameObject, float typingSpeed)
    {
        buttonGameObject.SetActive(true);

        foreach (char letter in choiceText)
        {
            choiceTextUI.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void ShowAllChoicesInstantly(DialogNode node)
    {
        foreach (Transform child in choicesContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Choice choice in node.choices)
        {
            Button button = Instantiate(choiceButtonPrefab, choicesContainer);
            TextMeshProUGUI choiceTextUI = button.GetComponentInChildren<TextMeshProUGUI>();
            choiceTextUI.text = choice.choiceText;
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnChoiceSelected(choice));
        }

        nextButton.gameObject.SetActive(false);
        //isDisplayingChoices = false;
    }

    private void OnChoiceSelected(Choice choice)
    {
        foreach (Transform child in choicesContainer)
        {
            Destroy(child.gameObject);
        }

        StartDialog(choice.nextNodeID);
    }

    private DialogNode FindNodeByID(int nodeID)
    {
        foreach (DialogNode node in dialogData.dialogNodes)
        {
            if (node.nodeID == nodeID)
                return node;
        }
        Debug.LogError("Node with ID " + nodeID + " not found.");
        return null;
    }
}
