// PoliceCar.cs
// Very simple police car that drives to target NPC and arrests it on contact.
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PoliceCar : MonoBehaviour
{
    NavMeshAgent agent;
    NPCController targetNPC;
    public float arrestDistance = 2.2f;
    public float driveSpeed = 6f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = driveSpeed;
    }

    public void ChaseAndArrest(NPCController npc)
    {
        targetNPC = npc;
        StartCoroutine(GoToTarget());
    }

    IEnumerator GoToTarget()
    {
        if (targetNPC == null) yield break;
        agent.SetDestination(targetNPC.transform.position);
        while (targetNPC != null && Vector3.Distance(transform.position, targetNPC.transform.position) > arrestDistance)
        {
            // update destination every frame to follow moving target
            agent.SetDestination(targetNPC.transform.position);
            yield return null;
        }
        if (targetNPC != null)
        {
            // Arrest
            targetNPC.OnArrested();
            // small pause then leave
            yield return new WaitForSeconds(0.6f);
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
