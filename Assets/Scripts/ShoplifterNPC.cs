using UnityEngine;
using UnityEngine.AI;

public class ShoplifterNPC : NPCBase
{
    public enum ShoplifterState { Wander, LookAround, AttemptSteal, Run }
    private ShoplifterState currentState = ShoplifterState.Wander;
    public ShoplifterState CurrentState => currentState;

    private NavMeshAgent navAgent;
    public Material shoplifterMaterial;
    public float lookAroundTime = 2f;
    public float stealTime = 3f;
    public LayerMask playerLayer;
    [SerializeField] private float wanderRadius = 10f; // Radius within stall
    public Transform stallCenter; // Changed to public
    private float timer = 0f;
    private bool hasReachedDestination = false;
    private bool isObserved = false;
    private Transform targetShelf;

    void Start()
    {
        npcType = NPCType.Shoplifter;
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
            renderer.material = shoplifterMaterial;
        }
        else
        {
            Debug.LogWarning($"No Renderer found on {gameObject.name} children!", this);
        }

        SetState(ShoplifterState.Wander);
    }

    void Update()
    {
        if (!enabled || isPaused) return;

        isObserved = IsPlayerObserving();
        Debug.Log($"Shoplifter {gameObject.name}: State={currentState}, Observed={isObserved}");

        switch (currentState)
        {
            case ShoplifterState.Wander:
                HandleWander();
                break;
            case ShoplifterState.LookAround:
                HandleLookAround();
                break;
            case ShoplifterState.AttemptSteal:
                HandleAttemptSteal();
                break;
            case ShoplifterState.Run:
                HandleRun();
                break;
        }
    }

    private void SetState(ShoplifterState newState)
    {
        currentState = newState;
        hasReachedDestination = false;
        timer = 0f;
        targetShelf = null;
        Debug.Log($"Shoplifter {gameObject.name} transitioned to state: {newState}");
    }

    private bool IsPlayerObserving()
    {
        Vector3 directionToPlayer = Camera.main.transform.position - transform.position;
        Ray ray = new Ray(transform.position, directionToPlayer.normalized);
        if (Physics.Raycast(ray, out RaycastHit hit, 10f, playerLayer))
        {
            bool isPlayer = hit.collider.CompareTag("Player");
            Debug.Log($"Raycast hit: {hit.collider?.name}, IsPlayer: {isPlayer}");
            return isPlayer;
        }
        Debug.Log("Raycast missed player");
        return false;
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
                SetState(ShoplifterState.LookAround);
            }
        }
    }

    private void HandleLookAround()
    {
        timer += Time.deltaTime;
        if (timer >= lookAroundTime)
        {
            if (isObserved)
            {
                SetState(ShoplifterState.Wander);
            }
            else
            {
                Collider[] shelves = Physics.OverlapSphere(transform.position, 3f, LayerMask.GetMask("Shelf"));
                if (shelves.Length > 0)
                {
                    targetShelf = shelves[Random.Range(0, shelves.Length)].transform;
                    SetState(ShoplifterState.AttemptSteal);
                }
                else
                {
                    SetState(ShoplifterState.Wander);
                }
            }
        }
    }

    private void HandleAttemptSteal()
    {
        if (targetShelf == null)
        {
            SetState(ShoplifterState.Wander);
            return;
        }

        if (!hasReachedDestination)
        {
            navAgent.SetDestination(targetShelf.position);
            Debug.DrawLine(transform.position, targetShelf.position, Color.red, 1f);
            if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                hasReachedDestination = true;
                timer = stealTime;
                Debug.Log($"Reached shelf for stealing");
            }
        }
        else
        {
            Transform item = targetShelf.Find("StealableItem");
            if (item == null)
            {
                Debug.Log("No item to steal on this shelf, moving on.");
                SetState(ShoplifterState.Wander);
                return;
            }

            if (isObserved)
            {
                Debug.Log("Theft detected by player!");
                GameManager.Instance.OnTheftDetected(this);
                SetState(ShoplifterState.Run);
            }
            else
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    Debug.Log($"Theft successful at shelf");
                    Destroy(item.gameObject);
                    GameManager.Instance.OnTheftSuccessful(this);
                    SetState(ShoplifterState.Run);
                }
            }
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
                Debug.DrawLine(transform.position, point, Color.yellow, 1f);
                if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
                {
                    hasReachedDestination = true;
                    Destroy(gameObject);
                }
            }
        }
    }

    public override string Interact()
    {
        base.Interact();
        if (currentState == ShoplifterState.Run)
        {
            return "Hey! I saw you grab those items! Stop right there!";
        }
        return "What are you doing sneaking around like that?";
    }

    public override void Resume()
    {
        base.Resume();
        Debug.Log($"Shoplifter {gameObject.name} resumed");
    }

    public void Accuse()
    {
        if (currentState == ShoplifterState.Run && hasStolen)
        {
            GameManager.Instance.OnSuccessfulAccusation(this);
            Destroy(gameObject);
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

    void OnDrawGizmos()
    {
        if (targetShelf != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetShelf.position, 0.5f);
        }
    }
}