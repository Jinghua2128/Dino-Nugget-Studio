using UnityEngine;
using UnityEngine.AI;

public class NormalShopperNPC : NPCBase
{
    private enum ShopperState { EnterStore, Browse, Buy, Leave }
    private ShopperState currentState = ShopperState.EnterStore;

    private NavMeshAgent navAgent;
    public Transform storeEntrance;
    public Transform[] browsePoints;
    public Transform checkoutPoint;
    public Transform exitPoint;

    [SerializeField] private float minBrowseTimePerShelf = 4f;
    [SerializeField] private float maxBrowseTimePerShelf = 7f;
    [SerializeField] private float buyTime = 3f;
    private float timer = 0f;
    private int currentBrowsePointIndex = 0;
    private bool hasReachedDestination = false;

    public Material shopperMaterial;

    void Start()
    {
        npcType = NPCBase.NPCType.NormalShopper;
        navAgent = GetComponent<NavMeshAgent>();

        if (storeEntrance == null || browsePoints.Length == 0 || checkoutPoint == null || exitPoint == null)
        {
            Debug.LogError($"Missing references in {gameObject.name}: StoreEntrance={storeEntrance}, BrowsePoints={browsePoints.Length}, CheckoutPoint={checkoutPoint}, ExitPoint={exitPoint}", gameObject);
            enabled = false;
            return;
        }

        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material = shopperMaterial;
        }

        SetState(ShopperState.EnterStore);
    }

    void Update()
    {
        if (!enabled || isPaused) return;

        switch (currentState)
        {
            case ShopperState.EnterStore:
                HandleEnterStore();
                break;
            case ShopperState.Browse:
                HandleBrowse();
                break;
            case ShopperState.Buy:
                HandleBuy();
                break;
            case ShopperState.Leave:
                HandleLeave();
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

    private void HandleEnterStore()
    {
        if (!hasReachedDestination)
        {
            navAgent.SetDestination(storeEntrance.position);
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
            navAgent.SetDestination(browsePoints[currentBrowsePointIndex].position);
            if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                hasReachedDestination = true;
                timer = Random.Range(minBrowseTimePerShelf, maxBrowseTimePerShelf);
            }
        }
        else
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                currentBrowsePointIndex++;
                if (currentBrowsePointIndex >= browsePoints.Length)
                {
                    SetState(ShopperState.Buy);
                }
                else
                {
                    hasReachedDestination = false;
                }
            }
        }
    }

    private void HandleBuy()
    {
        if (!hasReachedDestination)
        {
            navAgent.SetDestination(checkoutPoint.position);
            if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                hasReachedDestination = true;
                timer = buyTime;
            }
        }
        else
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                SetState(ShopperState.Leave);
            }
        }
    }

    private void HandleLeave()
    {
        if (!hasReachedDestination)
        {
            navAgent.SetDestination(exitPoint.position);
            if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                hasReachedDestination = true;
                Destroy(gameObject);
            }
        }
    }
}