// NPCController.cs
// Handles FSM for types: Shoplifter, Shopper, Distractor.
// Requires NavMeshAgent on the same GameObject.
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum NPCType { Shopper, Shoplifter, Distractor }
public enum NPCState { Enter, Wander, LookAround, AttemptSteal, Flee, Distract, Checkout, ExitStore, Idle }

[RequireComponent(typeof(NavMeshAgent))]
public class NPCController : MonoBehaviour, INPCInteractable
{
    [Header("Identity")]
    public NPCType type = NPCType.Shopper;
    public string npcName = "NPC";

    [Header("Movement")]
    public float wanderRadius = 6f;
    public float lookAroundDuration = 2f;
    public Transform storeEntrance; // the store exit/entrance transform
    public Transform checkoutPoint; // checkout counter position
    public Transform[] waypoints; // optional predetermined browsing points

    [Header("Shoplifting")]
    public Transform preferedStealTarget; // assign an item transform or leave null
    public float stealDistance = 1.2f;
    public bool hasStolen = false;
    public GameObject stolenObjectPrefab; // optional visual

    [Header("Distractor")]
    public NPCController partnerThief; // for distractor to coordinate with thief

    [Header("Visual/Behavior")]
    public Animator animator; // optional animator to play steal animations

    // Internal
    NavMeshAgent agent;
    NPCState state = NPCState.Enter;
    float stateTimer = 0f;

    // Interaction flag
    public bool isAccused = false;

    // Checkout timing
    public float checkoutDuration = 2f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (storeEntrance == null)
        {
            var exit = GameObject.FindGameObjectWithTag("StoreExit");
            if (exit) storeEntrance = exit.transform;
        }
    }

    void Start()
    {
        StartCoroutine(StateMachine());
    }

    IEnumerator StateMachine()
    {
        yield return null;
        while (true)
        {
            switch (type)
            {
                case NPCType.Shopper:
                    yield return ShopBehavior();
                    break;
                case NPCType.Shoplifter:
                    yield return ThiefBehavior();
                    break;
                case NPCType.Distractor:
                    yield return DistractorBehavior();
                    break;
            }
            yield return null;
        }
    }

    #region Behaviors

    IEnumerator ShopBehavior()
    {
        // Enter store -> browse -> checkout -> exit
        state = NPCState.Enter;
        if (waypoints != null && waypoints.Length > 0)
        {
            int idx = Random.Range(0, waypoints.Length);
            agent.SetDestination(waypoints[idx].position);
            yield return new WaitUntil(() => Vector3.Distance(transform.position, waypoints[idx].position) < 1.2f);
        }

        // browse for a bit
        stateTimer = Random.Range(2f, 5f);
        yield return new WaitForSeconds(stateTimer);

        // Possibly go to checkout (random)
        if (Random.value < 0.7f)
        {
            state = NPCState.Checkout;
            if (checkoutPoint != null)
            {
                agent.SetDestination(checkoutPoint.position);
                yield return new WaitUntil(() => Vector3.Distance(transform.position, checkoutPoint.position) < 1.2f);
                yield return new WaitForSeconds(checkoutDuration);
            }
        }

        // exit store
        if (storeEntrance != null)
        {
            state = NPCState.ExitStore;
            agent.SetDestination(storeEntrance.position);
            yield return new WaitUntil(() => Vector3.Distance(transform.position, storeEntrance.position) < 1.5f);
            Destroy(gameObject);
        }
    }

    IEnumerator ThiefBehavior()
    {
        state = NPCState.Enter;

        if (hasStolen)
        {
            // Already stolen: go to checkout then exit
            if (checkoutPoint != null)
            {
                state = NPCState.Checkout;
                agent.speed = 1.5f; // normal walk speed for checkout
                agent.SetDestination(checkoutPoint.position);
                yield return new WaitUntil(() => Vector3.Distance(transform.position, checkoutPoint.position) < 1.2f);
                yield return new WaitForSeconds(checkoutDuration);
            }

            if (storeEntrance != null)
            {
                state = NPCState.Flee;
                agent.speed *= 1.6f; // run to exit
                agent.SetDestination(storeEntrance.position);
                float fleeTime = 0f;
                while (Vector3.Distance(transform.position, storeEntrance.position) > 1.5f)
                {
                    fleeTime += Time.deltaTime;
                    if (Random.value < 0.01f)
                    {
                        agent.isStopped = true;
                        yield return new WaitForSeconds(Random.Range(0.15f, 0.5f));
                        agent.isStopped = false;
                    }
                    yield return null;
                }
                GameManager.Instance.OnThiefEscaped(this);
                Destroy(gameObject);
            }
            yield break;
        }

        // Not stolen yet: go to steal target
        if (preferedStealTarget != null)
        {
            agent.speed = 1.2f;
            agent.SetDestination(preferedStealTarget.position);
            yield return new WaitUntil(() => Vector3.Distance(transform.position, preferedStealTarget.position) < stealDistance);

            // Begin steal animation
            state = NPCState.AttemptSteal;
            if (animator) animator.SetTrigger("Steal");
            yield return new WaitForSeconds(0.9f);

            // Mark stolen
            hasStolen = true;
            if (stolenObjectPrefab)
            {
                Instantiate(stolenObjectPrefab, transform).transform.localPosition = Vector3.up * 0.9f;
            }

            // After stealing, go to checkout then exit
            if (checkoutPoint != null)
            {
                state = NPCState.Checkout;
                agent.speed = 1.5f;
                agent.SetDestination(checkoutPoint.position);
                yield return new WaitUntil(() => Vector3.Distance(transform.position, checkoutPoint.position) < 1.2f);
                yield return new WaitForSeconds(checkoutDuration);
            }

            if (storeEntrance != null)
            {
                state = NPCState.Flee;
                agent.speed *= 1.6f;
                agent.SetDestination(storeEntrance.position);
                while (Vector3.Distance(transform.position, storeEntrance.position) > 1.5f)
                {
                    if (Random.value < 0.01f)
                    {
                        agent.isStopped = true;
                        yield return new WaitForSeconds(Random.Range(0.15f, 0.5f));
                        agent.isStopped = false;
                    }
                    yield return null;
                }
                GameManager.Instance.OnThiefEscaped(this);
                Destroy(gameObject);
            }
        }
        else
        {
            // roam randomly until steal target assigned
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * wanderRadius;
            NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas);
            agent.SetDestination(hit.position);
            yield return new WaitForSeconds(Random.Range(1.5f, 3f));
        }
    }

    IEnumerator DistractorBehavior()
    {
        state = NPCState.Enter;
        float lifetime = Random.Range(6f, 12f);
        float timer = 0f;

        while (timer < lifetime)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * (wanderRadius * 0.6f);
            NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas);
            agent.SetDestination(hit.position);
            yield return new WaitUntil(() => Vector3.Distance(transform.position, hit.position) < 1.2f);

            // Possibly push down an item nearby
            Collider[] coll = Physics.OverlapSphere(transform.position, 2f);
            foreach (var c in coll)
            {
                ItemPickable ip = c.GetComponent<ItemPickable>();
                if (ip && !ip.isPushed && Random.value < 0.35f)
                {
                    ip.PushDown();
                    break;
                }
            }
            timer += Random.Range(1.5f, 3f);
            yield return null;
        }

        // After distracting, go to checkout then exit
        if (checkoutPoint != null)
        {
            state = NPCState.Checkout;
            agent.SetDestination(checkoutPoint.position);
            yield return new WaitUntil(() => Vector3.Distance(transform.position, checkoutPoint.position) < 1.2f);
            yield return new WaitForSeconds(checkoutDuration);
        }

        if (storeEntrance != null)
        {
            state = NPCState.ExitStore;
            agent.SetDestination(storeEntrance.position);
            yield return new WaitUntil(() => Vector3.Distance(transform.position, storeEntrance.position) < 1.5f);
            Destroy(gameObject);
        }
    }
    #endregion

    #region Interaction (INPCInteractable)
    public string GetName()
    {
        return npcName;
    }

    public void OnInteract(PlayerInteractor interactor)
    {
        interactor.ShowNPCUI(this);
    }

    public void OnLeave(PlayerInteractor interactor)
    {
        interactor.HideNPCUI();
    }
    #endregion

    // Called when police arrives and removes this NPC
    public void OnArrested()
    {
        // TODO: play arrest animation here
        Destroy(gameObject);
    }
}
