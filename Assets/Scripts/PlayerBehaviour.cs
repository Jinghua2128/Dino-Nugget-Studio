using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class PlayerBehaviour : MonoBehaviour
{
    public static PlayerBehaviour Instance { get; private set; }
    
    public Camera mainCamera;
    public LayerMask npcLayer;
    public LayerMask itemLayer;
    public float interactionRange = 5f;
    
    [Header("UI Elements")]
    public CanvasGroup interactCanvas;
    public Button interactButton;
    public Button accuseButton;
    public TMP_Text promptText;
    
    [Header("Item Handling")]
    public GameObject HeldItem { get; private set; }
    private Vector3 holdOffset = new Vector3(0f, 1f, 1f);
    private float shelfPlaceHeight = 0.5f;
    
    private NPCBase selectedNPC;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No Main Camera found!", this);
            }
        }

        if (interactCanvas != null)
        {
            interactCanvas.alpha = 0f;
            interactCanvas.interactable = false;
            interactCanvas.blocksRaycasts = false;
        }

        if (interactButton != null) interactButton.onClick.AddListener(OnInteractButtonClicked);
        if (accuseButton != null) accuseButton.onClick.AddListener(OnAccuseButtonClicked);
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        HandleMouseInteractions();
        HandleKeyboardInteractions();
    }

    private void HandleMouseInteractions()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, npcLayer | itemLayer))
            {
                HandleHit(hit);
            }
            else if (selectedNPC != null)
            {
                selectedNPC.Resume();
                HideUI();
                selectedNPC = null;
            }
        }
    }

    private void HandleKeyboardInteractions()
    {
        if (Input.GetKeyDown(KeyCode.E) && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = mainCamera.nearClipPlane;
            Vector3 direction = (mainCamera.ScreenToWorldPoint(mousePos) - transform.position).normalized;
            Ray ray = new Ray(transform.position, direction);
            
            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, npcLayer | itemLayer))
            {
                HandleHit(hit);
            }
        }
    }

    private void HandleHit(RaycastHit hit)
    {
        NPCBase npc = hit.collider.GetComponent<NPCBase>();
        if (npc != null)
        {
            HandleNPCHit(npc);
        }
        else if (hit.collider.CompareTag("StealableItem") && HeldItem == null)
        {
            HandleItemInteraction(hit.collider.gameObject);
        }
        else if (hit.collider.CompareTag("Shelf") && HeldItem != null)
        {
            HandleShelfInteraction(hit.collider.transform);
        }
    }

    private void HandleNPCHit(NPCBase npc)
    {
        if (npc != null)
        {
            selectedNPC = npc;
            ShowInteractUI(npc);
        }
    }

    public void ShowInteractUI(NPCBase npc)
    {
        if (interactCanvas == null || interactButton == null || accuseButton == null) return;

        selectedNPC = npc;
        accuseButton.gameObject.SetActive(npc.npcType == NPCBase.NPCType.Shoplifter || npc.npcType == NPCBase.NPCType.Distractor);
        interactCanvas.alpha = 1f;
        interactCanvas.interactable = true;
        interactCanvas.blocksRaycasts = true;
    }

    public void HideUI()
    {
        if (interactCanvas != null)
        {
            interactCanvas.alpha = 0f;
            interactCanvas.interactable = false;
            interactCanvas.blocksRaycasts = false;
        }
        if (promptText != null) promptText.gameObject.SetActive(false);
        selectedNPC = null;
    }

    private void OnInteractButtonClicked()
    {
        if (selectedNPC != null)
        {
            selectedNPC.Interact();
            HideUI();
        }
    }

    private void OnAccuseButtonClicked()
    {
        if (selectedNPC != null)
        {
            GameManager.Instance.SetCurrentNPC(selectedNPC);
            HideUI();
        }
    }

    public void HoldItem(GameObject item)
    {
        HeldItem = item;
        item.transform.SetParent(transform);
        item.transform.localPosition = holdOffset;
        item.transform.localRotation = Quaternion.identity;
        if (item.TryGetComponent<Collider>(out var col))
        {
            col.enabled = false;
        }
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    public void ReleaseItem()
    {
        if (HeldItem == null) return;
        HeldItem.transform.SetParent(null);
        if (HeldItem.TryGetComponent<Collider>(out var col))
        {
            col.enabled = true;
        }
        HeldItem = null;
    }

    private void HandleItemInteraction(GameObject item)
    {
        if (HeldItem != null) return;
        HoldItem(item);
    }

    private void HandleShelfInteraction(Transform shelf)
    {
        if (HeldItem == null) return;
        HeldItem.transform.SetParent(shelf); // Attach to shelf
        HeldItem.transform.localPosition = Vector3.up * shelfPlaceHeight;
        HeldItem.transform.localRotation = Quaternion.identity;

        // Check if this is a task shelf and not already completed
        if (GameManager.Instance.IsTaskShelf(shelf))
        {
            GameManager.Instance.HandleTaskCompletion(shelf);
        }

        ReleaseItem();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("StealableItem") && HeldItem == null)
        {
            if (promptText != null)
            {
                promptText.text = "Press E to Pick Up";
                promptText.gameObject.SetActive(true);
            }
        }
        else if (other.CompareTag("Shelf") && HeldItem != null)
        {
            if (promptText != null)
            {
                promptText.text = "Press E to Place Item";
                promptText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("StealableItem") || other.CompareTag("Shelf"))
        {
            if (promptText != null) promptText.gameObject.SetActive(false);
        }
    }
}