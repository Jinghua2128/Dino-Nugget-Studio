using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerBehaviour : MonoBehaviour
{
    public static PlayerBehaviour Instance { get; private set; }
    
    public Camera mainCamera;
    public LayerMask npcLayer;
    public PlayerInteractUI interactUI;
    public float interactionRange = 5f;
    
    public GameObject HeldItem { get; private set; }
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
        
        if (interactUI == null)
        {
            Debug.LogError("PlayerInteractUI not assigned!", this);
        }
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
            if (npcLayer.value == 0) return;
            
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, npcLayer))
            {
                HandleNPCHit(hit.collider.GetComponent<NPCBase>());
            }
            else if (selectedNPC != null)
            {
                selectedNPC.Resume();
                interactUI.HideUI();
                selectedNPC = null;
            }
        }
    }

    private void HandleKeyboardInteractions()
    {
        if (Input.GetKeyDown(KeyCode.E) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (npcLayer.value == 0) return;
            
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = mainCamera.nearClipPlane;
            Vector3 direction = (mainCamera.ScreenToWorldPoint(mousePos) - transform.position).normalized;
            Ray ray = new Ray(transform.position, direction);
            
            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, npcLayer))
            {
                HandleNPCHit(hit.collider.GetComponent<NPCBase>());
            }
        }
    }

    private void HandleNPCHit(NPCBase npc)
    {
        if (npc != null)
        {
            selectedNPC = npc;
            interactUI.ShowUI(npc);
        }
    }

    public void HoldItem(GameObject item)
    {
        HeldItem = item;
    }

    public void ReleaseItem()
    {
        HeldItem = null;
    }
}