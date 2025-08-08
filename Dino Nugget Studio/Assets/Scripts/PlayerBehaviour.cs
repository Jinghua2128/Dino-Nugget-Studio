using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerBehaviour : MonoBehaviour
{
    public Camera mainCamera;
    public LayerMask npcLayer;
    public PlayerInteractUI interactUI;
    public float interactionRange = 5f; // Range for "E" key raycast

    private NPCBase selectedNPC;

    void Start()
    {
        Debug.Log($"PlayerBehaviour: Starting on {gameObject.name}");
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No Main Camera found! Assign a camera with MainCamera tag.", this);
            }
        }
        if (interactUI == null)
        {
            Debug.LogError("PlayerInteractUI not assigned!", this);
        }

        // Validate npcLayer (expecting layer 6, "NPC")
        if (npcLayer.value == 0)
        {
            Debug.LogWarning("npcLayer is unassigned or set to 'Nothing'. Please assign the 'NPC' layer (index 6) in the Inspector.", this);
        }
        else
        {
            int layerIndex = Mathf.FloorToInt(Mathf.Log(npcLayer.value, 2));
            string layerName = LayerMask.LayerToName(layerIndex);
            if (layerIndex != 6 || layerName != "NPC")
            {
                Debug.LogWarning($"npcLayer is set to '{layerName}' (index {layerIndex}). Expected 'NPC' (index 6). Please assign the correct layer.", this);
            }
            else
            {
                Debug.Log($"PlayerBehaviour: npcLayer={layerName} (index {layerIndex})");
            }
        }
    }

    void Update()
    {
        Debug.Log("PlayerBehaviour: Update running"); // Confirm Update is called

        // Mouse click interaction
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse button down detected");
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("Click blocked by UI element!");
                return;
            }

            if (npcLayer.value == 0)
            {
                Debug.LogWarning("Cannot raycast: npcLayer is unassigned or set to 'Nothing'!");
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Debug.Log($"Mouse raycast from {ray.origin} in direction {ray.direction}");
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, npcLayer))
            {
                Debug.Log($"Mouse raycast hit: {hit.collider.gameObject.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
                NPCBase npc = hit.collider.GetComponent<NPCBase>();
                if (npc != null)
                {
                    Debug.Log($"NPC found: {npc.gameObject.name}, Type: {npc.npcType}");
                    selectedNPC = npc;
                    interactUI.ShowUI(npc);
                }
                else
                {
                    Debug.LogWarning($"No NPCBase component found on {hit.collider.gameObject.name}! Ensure ShoplifterNPC, NormalShopperNPC, or DistractorNPC is attached.");
                }
            }
            else
            {
                Debug.Log("Mouse raycast missed NPCs!");
                if (selectedNPC != null)
                {
                    Debug.Log($"Resuming NPC: {selectedNPC.gameObject.name}");
                    selectedNPC.Resume();
                    interactUI.HideUI();
                    FindObjectOfType<DialogueUI>()?.HideDialogue();
                    selectedNPC = null;
                }
            }
        }

        // "E" key interaction
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E key pressed");
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("E key blocked by UI element!");
                return;
            }

            if (npcLayer.value == 0)
            {
                Debug.LogWarning("Cannot raycast: npcLayer is unassigned or set to 'Nothing'!");
                return;
            }

            // Raycast from player toward mouse cursor
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = mainCamera.nearClipPlane;
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
            Vector3 direction = (worldPos - transform.position).normalized;
            Ray ray = new Ray(transform.position, direction);
            Debug.Log($"E key raycast from {ray.origin} in direction {ray.direction}");
            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, npcLayer))
            {
                Debug.Log($"E key raycast hit: {hit.collider.gameObject.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
                NPCBase npc = hit.collider.GetComponent<NPCBase>();
                if (npc != null)
                {
                    Debug.Log($"NPC found: {npc.gameObject.name}, Type: {npc.npcType}");
                    selectedNPC = npc;
                    interactUI.ShowUI(npc);
                }
                else
                {
                    Debug.LogWarning($"No NPCBase component found on {hit.collider.gameObject.name}! Ensure ShoplifterNPC, NormalShopperNPC, or DistractorNPC is attached.");
                }
            }
            else
            {
                Debug.Log("E key raycast missed NPCs!");
            }
        }
    }
}