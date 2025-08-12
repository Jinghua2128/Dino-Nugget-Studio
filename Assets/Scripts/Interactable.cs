using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class Interactable : MonoBehaviour
{
    [Header("Interactable Type")]
    [SerializeField] private bool isNPC = false;
    [SerializeField] private bool isShelf = false;
    
    [Header("UI Elements")]
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Vector3 holdOffset = new Vector3(0f, 1f, 1f);
    [SerializeField] private float shelfPlaceHeight = 0.5f;
    
    private bool playerInRange = false;

    private void Awake()
    {
        if (!TryGetComponent<Collider>(out var col) || !col.isTrigger)
        {
            Debug.LogWarning("Interactable should have a trigger collider", this);
        }
    }

    private void Start()
    {
        SetPromptVisibility(false);
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        playerInRange = true;
        UpdatePromptText();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            SetPromptVisibility(false);
        }
    }

    private void UpdatePromptText()
    {
        if (promptText == null) return;
        
        string prompt = "Press E to ";
        bool shouldShowPrompt = true;

        if (isNPC)
        {
            prompt += "Interact";
        }
        else if (isShelf)
        {
            shouldShowPrompt = PlayerBehaviour.Instance.HeldItem != null;
            prompt += "Place Item";
        }
        else // Item
        {
            shouldShowPrompt = PlayerBehaviour.Instance.HeldItem == null;
            prompt += "Pick Up";
        }

        if (shouldShowPrompt)
        {
            promptText.text = prompt;
            SetPromptVisibility(true);
        }
        else
        {
            SetPromptVisibility(false);
        }
    }

    private void SetPromptVisibility(bool visible)
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(visible);
        }
    }

    private void Interact()
    {
        if (isNPC)
        {
            HandleNPCInteraction();
        }
        else if (isShelf)
        {
            HandleShelfInteraction();
        }
        else
        {
            HandleItemInteraction();
        }

        SetPromptVisibility(false);
        UpdatePromptText();
    }

    private void HandleNPCInteraction()
    {
        if (!TryGetComponent<NPCBase>(out var npc)) return;
        
        if (GameManager.Instance != null && GameManager.Instance.accusationUI != null)
        {
            GameManager.Instance.SetCurrentNPC(npc);
            GameManager.Instance.accusationUI.SetActive(true);
        }
    }

    private void HandleShelfInteraction()
    {
        if (PlayerBehaviour.Instance.HeldItem == null) return;
        
        var heldItem = PlayerBehaviour.Instance.HeldItem;
        PlayerBehaviour.Instance.ReleaseItem();
        
        heldItem.transform.SetParent(null);
        heldItem.transform.position = transform.position + Vector3.up * shelfPlaceHeight;
        heldItem.transform.rotation = Quaternion.identity;
        
        if (heldItem.TryGetComponent<Collider>(out var col))
        {
            col.enabled = true;
        }
    }

    private void HandleItemInteraction()
    {
        if (PlayerBehaviour.Instance.HeldItem != null) return;
        
        PlayerBehaviour.Instance.HoldItem(gameObject);
        transform.SetParent(PlayerBehaviour.Instance.transform);
        transform.localPosition = holdOffset;
        transform.localRotation = Quaternion.identity;
        
        if (TryGetComponent<Collider>(out var col))
        {
            col.enabled = false;
        }
    }
}