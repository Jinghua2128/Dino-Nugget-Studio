using UnityEngine;
using UnityEngine.AI;

public class PoliceCar : MonoBehaviour
{
    [SerializeField] private AudioSource sirenSource;
    [SerializeField] private AudioClip sirenClip;

    private NavMeshAgent navAgent;
    private NPCBase targetNPC;
    private Vector3 stallPos;
    private Vector3 exitPos;
    private bool arrivedAtStall = false;

    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if (sirenSource == null)
        {
            sirenSource = gameObject.AddComponent<AudioSource>();
        }
        sirenSource.clip = sirenClip;
        sirenSource.loop = true;
    }

    public void Initialize(NPCBase npc, Vector3 stallPosition, Vector3 exitPosition)
    {
        targetNPC = npc;
        stallPos = stallPosition;
        exitPos = exitPosition;

        if (navAgent != null)
        {
            navAgent.SetDestination(stallPos);
        }
        if (sirenSource != null)
        {
            sirenSource.Play();
        }
    }

    void Update()
    {
        if (navAgent == null) return;

        if (!arrivedAtStall && navAgent.remainingDistance < 0.5f && !navAgent.pathPending)
        {
            arrivedAtStall = true;
            if (targetNPC != null)
            {
                Destroy(targetNPC.gameObject);
            }
            navAgent.SetDestination(exitPos);
            Invoke("DestroySelf", 5f);
        }
    }

    private void DestroySelf()
    {
        if (sirenSource != null)
        {
            sirenSource.Stop();
        }
        Destroy(gameObject);
    }
}