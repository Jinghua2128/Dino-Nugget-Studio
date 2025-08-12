using UnityEngine;
using UnityEngine.AI;

public class NormalShopperNPC : NPCBase
{
    private enum ShopperState { Wander, Browse, Buy }
    private ShopperState currentState = ShopperState.Wander;

    private NavMeshAgent navAgent;
    [SerializeField] private float minBrowseTimePerShelf = 4f;
    [SerializeField] private float maxBrowseTimePerShelf = 7f;
    [SerializeField] private float buyTime = 3f;
    private float timer = 0f;
    private bool hasReachedDestination = false;
    [SerializeField] private float wanderRadius = 10f; // Radius within stall
    public Transform stallCenter; // Changed to public
    public Material shopperMaterial;

    void Start()
    {
        npcType = NPCType.NormalShopper;
        hasStolen = false;
        navAgent = GetComponent<NavMeshAgent>();

        if (stallCenter == null)
        {
            Debug.LogError($"Missing stallCenter reference in {gameObject.name}", gameObject);
            enabled = false;
            return;
        }

        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material = shopperMaterial;
        }

        SetState(ShopperState.Wander);
    }

    void Update()
    {
        if (!enabled || isPaused) return;

        switch (currentState)
        {
            case ShopperState.Wander:
                HandleWander();
                break;
            case ShopperState.Browse:
                HandleBrowse();
                break;
            case ShopperState.Buy:
                HandleBuy();
                break;
        }
    }

    private void SetState(ShopperState newState)
    {
        currentState = newState;
        hasReachedDestination = false;
        timer = 0f;
        Debug.Log($"NormalShopper {gameObject.name} transitioned to state: {newState}");
    }

    public override string Interact()
    {
        base.Interact();
        string[] dialogues = new[]
        {
            "I'm picking up some groceries for my family tonight!",
            "Getting some sweets for the kids, they love chocolate!"
        };
        return dialogues[Random.Range(0, dialogues.Length)];
    }

    public override void Resume()
    {
        base.Resume();
        Debug.Log($"NormalShopper {gameObject.name} resumed");
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
                SetState(ShopperState.Browse);
            }
        }
    }

    private void HandleBrowse()
    {
        if (!hasReachedDestination)
        {
            Collider[] shelves = Physics.OverlapSphere(transform.position, 3f, LayerMask.GetMask("Shelf"));
            if (shelves.Length > 0)
            {
                navAgent.SetDestination(shelves[Random.Range(0, shelves.Length)].transform.position);
                if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
                {
                    hasReachedDestination = true;
                    timer = Random.Range(minBrowseTimePerShelf, maxBrowseTimePerShelf);
                }
            }
            else
            {
                SetState(ShopperState.Wander);
            }
        }
        else
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                SetState(ShopperState.Buy);
            }
        }
    }

    private void HandleBuy()
    {
        timer += Time.deltaTime;
        if (timer >= buyTime)
        {
            SetState(ShopperState.Wander);
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