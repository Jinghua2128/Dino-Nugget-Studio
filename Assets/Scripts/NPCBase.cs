using UnityEngine;

public abstract class NPCBase : MonoBehaviour
{
    public enum NPCType { NormalShopper, Shoplifter, Distractor }
    public NPCType npcType;
    protected bool isPaused = false;
    public bool hasStolen = false;

    public virtual string Interact()
    {
        isPaused = true;
        return "Default NPC interaction";
    }

    public virtual void Resume()
    {
        isPaused = false;
    }

    protected virtual void Start()
    {
        if (npcType == NPCType.Shoplifter || npcType == NPCType.Distractor)
        {
            hasStolen = false; // Set true only on successful theft
        }
    }
}