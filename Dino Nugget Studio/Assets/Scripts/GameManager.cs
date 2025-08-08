using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private int score = 0;
    private float money = 1000f;
    private float reputation = 100f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnTheftDetected(NPCBase npc)
    {
        Debug.Log($"Theft detected by {npc.name} (Type: {npc.npcType})!");
    }

    public void OnTheftSuccessful(NPCBase npc)
    {
        money -= 50f;
        Debug.Log($"Theft successful! Money: {money}");
        CheckGameOver();
    }

    public void OnSuccessfulAccusation(NPCBase npc)
    {
        score += 100;
        Debug.Log($"Successful accusation! Score: {score}");
    }

    public void OnWrongAccusation()
    {
        reputation -= 10f;
        Debug.Log($"Wrong accusation! Reputation: {reputation}");
        CheckGameOver();
    }

    public void OnPoliceCalled(NPCBase npc)
    {
        Debug.Log($"Police called for {npc.name}! Arrested!");
    }

    private void CheckGameOver()
    {
        if (money <= 0)
        {
            Debug.Log("Game Over: Store closed due to bankruptcy!");
        }
        else if (reputation <= 0)
        {
            Debug.Log("Game Over: Fired due to low reputation!");
        }
    }
}