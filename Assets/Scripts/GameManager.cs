using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Stats")]
    [SerializeField] private int startingScore = 0;
    [SerializeField] private float startingMoney = 1000f;
    [SerializeField] private float startingReputation = 100f;
    
    private int score;
    private float money;
    private float reputation;

    [Header("Game Events")]
    public UnityEvent<int> OnScoreChanged;
    public UnityEvent<float> OnMoneyChanged;
    public UnityEvent<float> OnReputationChanged;
    public UnityEvent<string> OnGameOver;

    [Header("NPC System")]
    [SerializeField] public GameObject accusationUI;
    public NPCBase CurrentNPC { get; private set; }

    [Header("Game Balance")]
    [SerializeField] private float wrongAccusationPenalty = 10f;
    [SerializeField] private float theftAmount = 50f;
    [SerializeField] private int successfulAccusationReward = 100;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeGame()
    {
        score = startingScore;
        money = startingMoney;
        reputation = startingReputation;

        // Initialize events if null
        OnScoreChanged ??= new UnityEvent<int>();
        OnMoneyChanged ??= new UnityEvent<float>();
        OnReputationChanged ??= new UnityEvent<float>();
        OnGameOver ??= new UnityEvent<string>();
    }

    #region NPC Management
    public void SetCurrentNPC(NPCBase npc)
    {
        CurrentNPC = npc;
    }

    public void ClearCurrentNPC()
    {
        CurrentNPC = null;
    }

    public void AccuseCurrentNPC()
    {
        if (CurrentNPC == null) return;

        if (CurrentNPC.hasStolen)
        {
            OnSuccessfulAccusation(CurrentNPC);
        }
        else
        {
            OnWrongAccusation();
        }

        ClearCurrentNPC();
        SetAccusationUIVisibility(false);
    }

    public void LeaveCurrentNPC()
    {
        ClearCurrentNPC();
        SetAccusationUIVisibility(false);
    }

    private void SetAccusationUIVisibility(bool visible)
    {
        accusationUI?.SetActive(visible);
    }
    #endregion

    #region Game Events
    public void OnTheftDetected(NPCBase npc)
    {
        Debug.Log($"Theft detected by {npc.name} (Type: {npc.npcType})!");
    }

    public void OnTheftSuccessful(NPCBase npc)
    {
        ModifyMoney(-theftAmount);
        Debug.Log($"Theft successful! Money: {money}");
        CheckGameOver();
    }

    public void OnSuccessfulAccusation(NPCBase npc)
    {
        ModifyScore(successfulAccusationReward);
        Debug.Log($"Successful accusation! Score: {score}");
    }

    public void OnWrongAccusation()
    {
        ModifyReputation(-wrongAccusationPenalty);
        Debug.Log($"Wrong accusation! Reputation: {reputation}");
        CheckGameOver();
    }

    public void OnPoliceCalled(NPCBase npc)
    {
        Debug.Log($"Police called for {npc.name}! Arrested!");
    }
    #endregion

    #region Stat Management
    public void ModifyScore(int amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score);
    }

    public void ModifyMoney(float amount)
    {
        money += amount;
        money = Mathf.Max(0, money); // Ensure money doesn't go negative
        OnMoneyChanged?.Invoke(money);
    }

    public void ModifyReputation(float amount)
    {
        reputation += amount;
        reputation = Mathf.Clamp(reputation, 0, 100); // Keep reputation between 0-100
        OnReputationChanged?.Invoke(reputation);
    }

    private void CheckGameOver()
    {
        if (money <= 0)
        {
            OnGameOver?.Invoke("Game Over: Store closed due to bankruptcy!");
        }
        else if (reputation <= 0)
        {
            OnGameOver?.Invoke("Game Over: Fired due to low reputation!");
        }
    }
    #endregion

    #region Public Accessors
    public int Score => score;
    public float Money => money;
    public float Reputation => reputation;
    #endregion
}