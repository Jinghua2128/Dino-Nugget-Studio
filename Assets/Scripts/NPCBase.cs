using UnityEngine;

public abstract class NPCBase : MonoBehaviour
{
    public enum NPCType { NormalShopper, Shoplifter, Distractor }
    public NPCType npcType;
    protected bool isPaused = false;
    public bool hasStolen = false; // Add this field (set dynamically based on npcType or game logic)

    public virtual string Interact()
    {
        isPaused = true;
        return "Default NPC interaction";
    }

    public virtual void Resume()
    {
        isPaused = false;
    }

    // Optional: Override in subclasses or set in Awake/Start based on npcType
    protected virtual void Awake()
    {
        // Example: Set hasStolen based on type (customize as needed)
        if (npcType == NPCType.Shoplifter)
        {
            hasStolen = true; // Or randomize: hasStolen = Random.value > 0.5f;
        }
    }
}