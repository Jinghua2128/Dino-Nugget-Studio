using UnityEngine;
using UnityEngine.AI;

public class ShoplifterNPC : NPCBase
{
    public enum ShoplifterState { Wander, LookAround, AttemptSteal, Run }
    private ShoplifterState currentState = ShoplifterState.Wander;
    public ShoplifterState CurrentState => currentState;

    private NavMeshAgent navAgent;
    public Transform[] wanderPoints;
    public Transform[] stealPoints;
    public Transform exitPoint;
    public Material shoplifterMaterial;
    public float lookAroundTime = 2f;
    public float stealTime = 3f;
    public LayerMask playerLayer;
    private float timer = 0f;
    private int currentWanderPointIndex = 0;
    private int currentStealPointIndex = 0;
    private bool hasReachedDestination = false;
    private bool isObserved = false;

    void Start()
    {
        npcType = NPCBase.NPCType.Shoplifter;
        navAgent = GetComponent<NavMeshAgent>();

        if (wanderPoints.Length == 0 || stealPoints.Length == 0 || exitPoint == null)
        {
            Debug.LogError($"Missing references in {gameObject.name}: WanderPoints={wanderPoints.Length}, StealPoints={stealPoints.Length}, ExitPoint={exitPoint}", gameObject);
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
        Debug.Log($"Shoplifter {gameObject.name}: State={currentState}, Observed={isObserved}, StealPointIndex={currentStealPointIndex}, WanderPointIndex={currentWanderPointIndex}");

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
            if (wanderPoints[currentWanderPointIndex] == null)
            {
                Debug.LogError($"Wander point {currentWanderPointIndex} is null!", this);
                return;
            }
            navAgent.SetDestination(wanderPoints[currentWanderPointIndex].position);
            Debug.DrawLine(transform.position, wanderPoints[currentWanderPointIndex].position, Color.blue, 1f);
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
        if (stealPoints == null || stealPoints.Length == 0)
        {
            Debug.LogError("stealPoints array not set or empty!", this);
            SetState(ShoplifterState.Run);
            return;
        }
        if (currentStealPointIndex < 0 || currentStealPointIndex >= stealPoints.Length)
        {
            Debug.LogError($"currentStealPointIndex {currentStealPointIndex} out of bounds!", this);
            SetState(ShoplifterState.Run);
            return;
        }
        if (stealPoints[currentStealPointIndex] == null)
        {
            Debug.LogError($"Steal point {currentStealPointIndex} is null!", this);
            SetState(ShoplifterState.Run);
            return;
        }
        float distanceToStealPoint = Vector3.Distance(transform.position, stealPoints[currentStealPointIndex].position);
        Debug.Log($"Distance to steal point {currentStealPointIndex}: {distanceToStealPoint}");
        if (timer >= lookAroundTime)
        {
            if (!isObserved)
            {
                Debug.Log($"Transitioning to AttemptSteal at steal point {currentStealPointIndex}");
                SetState(ShoplifterState.AttemptSteal);
            }
            else
            {
                Debug.Log($"Cannot steal: Observed={isObserved}, Distance={distanceToStealPoint}");
                currentWanderPointIndex = (currentWanderPointIndex + 1) % wanderPoints.Length;
                SetState(ShoplifterState.Wander);
            }
        }
    }

    private void HandleAttemptSteal()
    {
        // Defensive checks
        if (stealPoints == null || stealPoints.Length == 0)
        {
            Debug.LogError("stealPoints array not set or empty!", this);
            SetState(ShoplifterState.Run);
            return;
        }
        if (currentStealPointIndex < 0 || currentStealPointIndex >= stealPoints.Length)
        {
            Debug.LogError($"currentStealPointIndex {currentStealPointIndex} out of bounds!", this);
            SetState(ShoplifterState.Run);
            return;
        }
        if (stealPoints[currentStealPointIndex] == null)
        {
            Debug.LogError($"Steal point {currentStealPointIndex} is null!", this);
            SetState(ShoplifterState.Run);
            return;
        }

        if (!hasReachedDestination)
        {
            navAgent.SetDestination(stealPoints[currentStealPointIndex].position);
            Debug.DrawLine(transform.position, stealPoints[currentStealPointIndex].position, Color.red, 1f);
            if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                hasReachedDestination = true;
                timer = stealTime;
                Debug.Log($"Reached steal point {currentStealPointIndex}, starting steal timer");
            }
        }
        else
        {
            // Check if the StealableItem still exists
            Transform item = stealPoints[currentStealPointIndex].Find("StealableItem");
            if (item == null)
            {
                Debug.Log("Item already stolen or missing, skipping to next steal point.");
                currentStealPointIndex++;
                if (currentStealPointIndex >= stealPoints.Length)
                {
                    SetState(ShoplifterState.Run);
                }
                else
                {
                    SetState(ShoplifterState.Wander);
                }
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
                    Debug.Log($"Theft successful at steal point {currentStealPointIndex}");
                    Destroy(item.gameObject);

                    GameManager.Instance.OnTheftSuccessful(this);
                    currentStealPointIndex++;
                    if (currentStealPointIndex >= stealPoints.Length)
                    {
                        SetState(ShoplifterState.Run);
                    }
                    else
                    {
                        SetState(ShoplifterState.Wander);
                    }
                }
            }
        }
    }

    private void HandleRun()
    {
        if (!hasReachedDestination)
        {
            if (exitPoint == null)
            {
                Debug.LogError("Exit point is null!", this);
                return;
            }
            navAgent.SetDestination(exitPoint.position);
            navAgent.speed *= 1.5f;
            Debug.DrawLine(transform.position, exitPoint.position, Color.yellow, 1f);
            if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                hasReachedDestination = true;
                Destroy(gameObject);
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
        if (currentState == ShoplifterState.Run)
        {
            GameManager.Instance.OnPoliceCalled(this);
            Destroy(gameObject);
        }
    }

    void OnDrawGizmos()
    {
        if (stealPoints != null)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < stealPoints.Length; i++)
            {
                if (stealPoints[i] != null)
                    Gizmos.DrawSphere(stealPoints[i].position, 0.5f);
            }
        }
    }
}