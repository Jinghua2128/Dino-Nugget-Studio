using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public CanvasGroup dialogueCanvas;
    public TextMeshProUGUI dialogueText;
    public Button closeButton;

    void Start()
    {
        if (dialogueCanvas == null)
        {
            Debug.LogError("DialogueCanvas not assigned in DialogueUI!", this);
        }
        if (dialogueText == null)
        {
            Debug.LogError("DialogueText not assigned in DialogueUI!", this);
        }
        if (closeButton == null)
        {
            Debug.LogError("CloseButton not assigned in DialogueUI!", this);
        }
        else
        {
            closeButton.onClick.AddListener(HideDialogue);
        }
        HideDialogue();
    }

    public void ShowDialogue(string dialogue)
    {
        if (dialogueCanvas == null || dialogueText == null) return;

        dialogueText.text = dialogue;
        dialogueCanvas.alpha = 1f;
        dialogueCanvas.interactable = true;
        dialogueCanvas.blocksRaycasts = true;
    }

    public void HideDialogue()
    {
        if (dialogueCanvas == null) return;

        dialogueCanvas.alpha = 0f;
        dialogueCanvas.interactable = false;
        dialogueCanvas.blocksRaycasts = false;
    }
}