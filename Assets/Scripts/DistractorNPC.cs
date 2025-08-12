using UnityEngine;
using UnityEngine.AI;

public class DistractorNPC : NPCBase
{
    private enum DistractorState { EnterStore, LookForPlayer, Distract, Run }
    private DistractorState currentState = DistractorState.EnterStore;

    private NavMeshAgent navAgent;
    public Transform storeEntrance;
    public Transform exitPoint;
    public ShoplifterNPC partner;
    public Material distractorMaterial;
    public float distractTime = 5f;
    private float timer = 0f;
    private bool hasReachedDestination = false;
    private Transform player;

    void Start()
    {
        npcType = NPCBase.NPCType.Distractor;
        navAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (storeEntrance == null || exitPoint == null || partner == null || player == null)
        {
            Debug.LogError($"Missing references in {gameObject.name}. Disabling NPC.", gameObject);
            enabled = false;
            return;
        }

        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material = distractorMaterial;
        }

        SetState(DistractorState.EnterStore);
    }

    void Update()
    {
        if (!enabled || isPaused) return;

        switch (currentState)
        {
            case DistractorState.EnterStore:
                HandleEnterStore();
                break;
            case DistractorState.LookForPlayer:
                HandleLookForPlayer();
                break;
            case DistractorState.Distract:
                HandleDistract();
                break;
            case DistractorState.Run:
                HandleRun();
                break;
        }
    }

    private void SetState(DistractorState newState)
    {
        currentState = newState;
        hasReachedDestination = false;
        timer = 0f;
        Debug.Log($"Distractor {gameObject.name} transitioned to state: {newState}");
        if (newState == DistractorState.Distract)
        {
            Debug.Log($"Distractor {gameObject.name} knocked over groceries!");
        }
    }

    public override string Interact()
    {
        base.Interact();
        return "Oops, I knocked over some groceries! Gotta run!";
    }

    public override void Resume()
    {
        base.Resume();
        Debug.Log($"Distractor {gameObject.name} resumed");
    }

    private void HandleEnterStore()
    {
        if (!hasReachedDestination)
        {
            navAgent.SetDestination(storeEntrance.position);
            if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                hasReachedDestination = true;
                SetState(DistractorState.LookForPlayer);
            }
        }
    }

    private void HandleLookForPlayer()
    {
        navAgent.SetDestination(player.position);
        if (Vector3.Distance(transform.position, player.position) < 2f)
        {
            SetState(DistractorState.Distract);
        }
        else if (partner.CurrentState == ShoplifterNPC.ShoplifterState.Run)
        {
            SetState(DistractorState.Run);
        }
    }

    private void HandleDistract()
    {
        timer += Time.deltaTime;
        navAgent.SetDestination(player.position);
        if (timer >= distractTime || partner.CurrentState == ShoplifterNPC.ShoplifterState.Run)
        {
            SetState(DistractorState.Run);
        }
    }

    private void HandleRun()
    {
        if (!hasReachedDestination)
        {
            navAgent.SetDestination(exitPoint.position);
            navAgent.speed *= 1.5f;
            if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                hasReachedDestination = true;
                Destroy(gameObject);
            }
        }
    }
}