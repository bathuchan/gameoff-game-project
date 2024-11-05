[System.Serializable]
public class Choice
{
    public string choiceText;
    public int nextNodeID; // Next node ID specific to this choice
    public float typingSpeed = 0.05f; // Unique typing speed for this choice
    
}
