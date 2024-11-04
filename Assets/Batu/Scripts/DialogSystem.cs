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
    private bool isTyping;
    private bool isDisplayingChoices;

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

        isTyping = true;
        typingCoroutine = StartCoroutine(TypeText(node.npcText, node.typingSpeed));
    }

    private IEnumerator TypeText(string text, float typingSpeed)
    {
        npcTextUI.text = "";
        nextButton.gameObject.SetActive(true);
        nextButton.onClick.RemoveAllListeners();

        nextButton.onClick.AddListener(() =>
        {
            if (isTyping)
            {
                npcTextUI.text = currentNode.npcText;
                StopCoroutine(typingCoroutine);
                isTyping = false;
                UpdateChoicesUI(currentNode);
            }
            else if (isDisplayingChoices)
            {
                // Skip and show all choices immediately
                StopAllCoroutines();
                ShowAllChoicesInstantly(currentNode);
            }
            else
            {
                if (currentNode.willEndConversation)
                {
                    dialogUI.SetActive(false);
                }
                else
                {
                    StartDialog(currentNode.nextNodeID);
                }
            }
        });

        foreach (char letter in text)
        {
            npcTextUI.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        UpdateChoicesUI(currentNode);
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
            isDisplayingChoices = true;

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

            // Display choice text with typewriter effect
            yield return StartCoroutine(TypeChoiceText(choice.choiceText, choiceTextUI, button.gameObject, choice.typingSpeed));
            yield return new WaitForSeconds(0.3f);
        }

        // Enable all choice buttons after all are displayed
        foreach (Button choiceButton in choiceButtons)
        {
            choiceButton.interactable = true;
        }

        // Hide the skip button once all choices are fully displayed
        nextButton.gameObject.SetActive(false);
        isDisplayingChoices = false;
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
        // Clear any existing choices to prevent duplicates
        foreach (Transform child in choicesContainer)
        {
            Destroy(child.gameObject);
        }

        // Immediately show all choices and enable them for selection
        foreach (Choice choice in node.choices)
        {
            Button button = Instantiate(choiceButtonPrefab, choicesContainer);
            TextMeshProUGUI choiceTextUI = button.GetComponentInChildren<TextMeshProUGUI>();
            choiceTextUI.text = choice.choiceText;
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnChoiceSelected(choice));
        }

        // Hide the skip button after showing all choices
        nextButton.gameObject.SetActive(false);
        isDisplayingChoices = false;
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
