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
    private Coroutine choicesCoroutine;
    private bool isTyping = false;
    private bool isDisplayingChoices = false;
    [SerializeField] private PlayerInputController playerInput;

    private void Start()
    {
        if (playerInput == null) playerInput = GameObject.FindAnyObjectByType<PlayerInputController>();

        nextButton.onClick.AddListener(OnSkipOrAdvance); // Attach the skip/advance logic to the button
        //StartDialog(0);
    }

    public void StartDialog(int startNodeID)
    {
        playerInput.enabled = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
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

        typingCoroutine = StartCoroutine(TypeText(node.npcTextSegments, node.typingSpeed));
    }

    private IEnumerator TypeText(List<TextSegment> textSegments, float typingSpeed)
    {
        npcTextUI.text = ""; // Clear the text field
        nextButton.gameObject.SetActive(true);
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnSkipOrAdvance);

        isTyping = true;

        foreach (var segment in textSegments)
        {
            string formattedText = ApplyFormatting(segment);

            int i = 0;
            while (i < formattedText.Length)
            {
                if (!isTyping) // Stop typing if the skip button was pressed to show full text
                {
                    npcTextUI.text = BuildFullText(textSegments);
                    break;
                }

                if (formattedText[i] == '<')
                {
                    int closingBracketIndex = formattedText.IndexOf('>', i);
                    if (closingBracketIndex != -1)
                    {
                        npcTextUI.text += formattedText.Substring(i, closingBracketIndex - i + 1);
                        i = closingBracketIndex + 1;
                        continue;
                    }
                }

                npcTextUI.text += formattedText[i];
                i++;
                yield return new WaitForSeconds(typingSpeed);
            }

            if (!isTyping) break; // Exit typing early if the skip button was pressed
        }

        isTyping = false;
        UpdateChoicesUI(currentNode);
    }

    private void OnSkipOrAdvance()
    {
        if (isTyping)
        {
            isTyping = false; // Stop typing to show the full text instantly
        }
        else if (isDisplayingChoices)
        {
            if (choicesCoroutine != null) // Stop the choices coroutine if it's running
            {
                StopCoroutine(choicesCoroutine);
            }
            ShowAllChoicesInstantly(currentNode); // Show all choices if choices are being typed
            isDisplayingChoices = false;
        }
        else
        {
            if (currentNode.willEndConversation)
            {
                EndDialog();
            }
            else
            {
                StartDialog(currentNode.nextNodeID); // Advance to the next dialog node
            }
        }
    }

    private void EndDialog()
    {
        dialogUI.SetActive(false);
        playerInput.enabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private string ApplyFormatting(TextSegment segment)
    {
        string text = segment.text;

        if (segment.sizeMultiplier != 1f)
        {
            text = $"<size={(int)(npcTextUI.fontSize * segment.sizeMultiplier)}>{text}</size>";
        }

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

        if (segment.bold)
        {
            text = $"<b>{text}</b>";
        }

        if (segment.italic)
        {
            text = $"<i>{text}</i>";
        }

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

    private string BuildFullText(List<TextSegment> segments)
    {
        string fullText = "";
        foreach (var segment in segments)
        {
            fullText += ApplyFormatting(segment);
        }
        return fullText;
    }

    private void UpdateChoicesUI(DialogNode node)
    {
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
                    EndDialog();
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
            isDisplayingChoices = true;
            choicesCoroutine = StartCoroutine(DisplayAllChoicesSequentially(node));
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

        foreach (Button choiceButton in choiceButtons)
        {
            choiceButton.interactable = true;
        }

        nextButton.gameObject.SetActive(false);
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
    }

    private IEnumerator TypeChoiceText(string choiceText, TextMeshProUGUI choiceTextUI, GameObject buttonGameObject, float typingSpeed)
    {
        buttonGameObject.SetActive(true);

        foreach (char letter in choiceText)
        {
            if (!isDisplayingChoices) // Skip typing if all choices should be shown instantly
            {
                choiceTextUI.text = choiceText;
                yield break;
            }

            choiceTextUI.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
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