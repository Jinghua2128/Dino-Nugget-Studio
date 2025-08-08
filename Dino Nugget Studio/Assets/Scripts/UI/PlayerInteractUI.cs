using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractUI : MonoBehaviour
{
    public CanvasGroup interactCanvas;
    public Button interactButton;
    public Button accuseButton;
    private NPCBase currentNPC;
    private DialogueUI dialogueUI; // Reference to DialogueUI

    void Start()
    {
        if (interactCanvas == null)
        {
            Debug.LogError("InteractCanvas not assigned in PlayerInteractUI!", this);
        }
        if (interactButton == null)
        {
            Debug.LogError("InteractButton not assigned in PlayerInteractUI!", this);
        }
        if (accuseButton == null)
        {
            Debug.LogError("AccuseButton not assigned in PlayerInteractUI!", this);
        }
        else
        {
            interactButton.onClick.AddListener(OnInteractButtonClicked);
            accuseButton.onClick.AddListener(OnAccuseButtonClicked);
        }
        dialogueUI = FindObjectOfType<DialogueUI>();
        if (dialogueUI == null)
        {
            Debug.LogError("DialogueUI not found in scene!", this);
        }
        HideUI();
    }

    public void ShowUI(NPCBase npc)
    {
        if (interactCanvas == null || interactButton == null || accuseButton == null) return;

        currentNPC = npc;
        accuseButton.gameObject.SetActive(npc.npcType == NPCBase.NPCType.Shoplifter || npc.npcType == NPCBase.NPCType.Distractor);
        interactCanvas.alpha = 1f;
        interactCanvas.interactable = true;
        interactCanvas.blocksRaycasts = true;
    }

    public void HideUI()
    {
        if (interactCanvas == null) return;

        interactCanvas.alpha = 0f;
        interactCanvas.interactable = false;
        interactCanvas.blocksRaycasts = false;
        currentNPC = null;
    }

    private void OnInteractButtonClicked()
    {
        if (currentNPC != null && dialogueUI != null)
        {
            dialogueUI.ShowDialogue(currentNPC.Interact());
            HideUI();
        }
    }

    private void OnAccuseButtonClicked()
    {
        if (currentNPC != null)
        {
            if (currentNPC.npcType == NPCBase.NPCType.Shoplifter)
            {
                if (currentNPC is ShoplifterNPC shoplifter && shoplifter.CurrentState == ShoplifterNPC.ShoplifterState.Run)
                {
                    shoplifter.Accuse();
                    GameManager.Instance.OnSuccessfulAccusation(currentNPC);
                }
            }
            else if (currentNPC.npcType == NPCBase.NPCType.Distractor)
            {
                GameManager.Instance.OnSuccessfulAccusation(currentNPC);
                Destroy(currentNPC.gameObject);
            }
            else
            {
                GameManager.Instance.OnWrongAccusation();
            }
            HideUI();
            if (currentNPC != null) currentNPC.Resume();
        }
    }
}