// PlayerInteractor.cs
// Attach to player. Handles raycast-based interaction and the UI for Accuse/Leave.
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction")]
    public float interactRange = 2.5f;
    public LayerMask interactMask;
    public Camera playerCamera;

    [Header("UI")]
    public GameObject npcUIPrefab; // small Canvas prefab with Accuse and Leave buttons
    GameObject activeNPCUI;
    NPCController currentTarget;

    [Header("Movement (optional)")]
    public UnityEngine.AI.NavMeshAgent agent;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 50f, interactMask))
        {
            var interactable = hit.collider.GetComponentInParent<INPCInteractable>();
            if (interactable != null)
            {
                // move player close first if agent assigned
                Vector3 targetPos = hit.point;
                if (agent != null)
                {
                    agent.SetDestination(hit.point);
                }
                // direct call (for simplicity, open UI immediately)
                var npc = hit.collider.GetComponentInParent<NPCController>();
                if (npc != null)
                {
                    currentTarget = npc;
                    ShowNPCUI(npc);
                }
            }
            else
            {
                // maybe clicked on item to pick up
                ItemPickable ip = hit.collider.GetComponent<ItemPickable>();
                if (ip != null)
                {
                    // move to item and pick up (instant for prototype)
                    if (Vector3.Distance(transform.position, ip.transform.position) <= interactRange)
                    {
                        ip.OnPicked(this);
                    }
                    else if (agent != null)
                    {
                        agent.SetDestination(ip.transform.position);
                    }
                }
            }
        }
    }

    public void ShowNPCUI(NPCController npc)
    {
        HideNPCUI();
        // instantiate UI canvas near screen center or world space above NPC (choose screen for simplicity)
        activeNPCUI = Instantiate(npcUIPrefab, GameObject.Find("Canvas").transform);
        // set texts/buttons
        activeNPCUI.transform.SetAsLastSibling();
        var nameText = activeNPCUI.transform.Find("NameText").GetComponent<Text>();
        nameText.text = npc.GetName();
        var accuseBtn = activeNPCUI.transform.Find("AccuseButton").GetComponent<Button>();
        var leaveBtn = activeNPCUI.transform.Find("LeaveButton").GetComponent<Button>();
        accuseBtn.onClick.AddListener(() => OnAccuseClicked(npc));
        leaveBtn.onClick.AddListener(() => OnLeaveClicked(npc));
        // optional: highlight NPC visually
    }

    public void HideNPCUI()
    {
        if (activeNPCUI) Destroy(activeNPCUI);
    }

    void OnAccuseClicked(NPCController npc)
    {
        HideNPCUI();
        // Decide if accusation is correct
        bool correct = npc.type == NPCType.Shoplifter && npc.hasStolen;
        if (correct)
        {
            GameManager.Instance.OnAccuseSuccess(npc);
        }
        else
        {
            GameManager.Instance.OnAccuseFail(npc);
        }
    }

    void OnLeaveClicked(NPCController npc)
    {
        HideNPCUI();
        // nothing else to do
    }
}
