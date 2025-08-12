using UnityEngine;
using UnityEngine.AI;

public class DistractorNPC : NPCBase
{
    private enum DistractorState { Wander, LookForPlayer, Distract, Run }
    private DistractorState currentState = DistractorState.Wander;

    private NavMeshAgent navAgent;
    public ShoplifterNPC partner;
    public Material distractorMaterial;
    public float distractTime = 5f;
    [SerializeField] private float wanderRadius = 10f; // Radius within stall
    public Transform stallCenter; // Changed to public
    private float timer = 0f;
    private bool hasReachedDestination = false;
    private Transform player;

    void Start()
    {
        npcType = NPCType.Distractor;
        hasStolen = false;
        navAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (stallCenter == null || partner == null || player == null)
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

        SetState(DistractorState.Wander);
    }

    void Update()
    {
        if (!enabled || isPaused) return;

        if (partner != null && partner.hasStolen) hasStolen = true;

        switch (currentState)
        {
            case DistractorState.Wander:
                HandleWander();
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

    private void HandleWander()
    {
        if (!hasReachedDestination)
        {
            if (TryFindRandomPoint(stallCenter.position, wanderRadius, out Vector3 point))
            {
                navAgent.SetDestination(point);
            }
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
        else if (partner != null && partner.CurrentState == ShoplifterNPC.ShoplifterState.Run)
        {
            SetState(DistractorState.Run);
        }
    }

    private void HandleDistract()
    {
        timer += Time.deltaTime;
        navAgent.SetDestination(player.position);
        if (timer >= distractTime || (partner != null && partner.CurrentState == ShoplifterNPC.ShoplifterState.Run))
        {
            SetState(DistractorState.Run);
        }
    }

    private void HandleRun()
    {
        if (!hasReachedDestination)
        {
            if (TryFindRandomPoint(stallCenter.position, wanderRadius * 2f, out Vector3 point))
            {
                navAgent.SetDestination(point);
                navAgent.speed *= 1.5f;
                if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
                {
                    hasReachedDestination = true;
                    Destroy(gameObject);
                }
            }
        }
    }

    private bool TryFindRandomPoint(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * radius;
            randomPoint.y = center.y;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = center;
        return false;
    }
}