// GameManager.cs
// Singleton to manage score, money, reputation and police spawning
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player stats")]
    public int score = 0;
    public float money = 100f;
    public float reputation = 100f;

    [Header("Accuse values")]
    public int accuseCorrectScore = 10;
    public int accuseWrongPenalty = 8;
    public float moneyLossOnMiss = 10f;

    [Header("Police")]
    public GameObject policeCarPrefab;
    public Transform policeSpawnPoint;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void AddPoints(float p)
    {
        score += Mathf.RoundToInt(p);
        UIManager.Instance.UpdateHUD(score, money, reputation);
    }

    public void OnAccuseSuccess(NPCController npc)
    {
        // increase score, call police, remove NPC when police arrives
        score += accuseCorrectScore;
        UIManager.Instance.ShowFloatingText("Correct Accuse! +" + accuseCorrectScore, npc.transform.position);
        UIManager.Instance.UpdateHUD(score, money, reputation);
        SpawnPoliceToArrest(npc);
    }

    public void OnAccuseFail(NPCController npc)
    {
        score = Mathf.Max(0, score - accuseWrongPenalty);
        reputation = Mathf.Max(0f, reputation - 5f);
        money = Mathf.Max(0f, money - moneyLossOnMiss);
        UIManager.Instance.ShowFloatingText("Wrong Accuse! -" + accuseWrongPenalty, npc.transform.position);
        UIManager.Instance.UpdateHUD(score, money, reputation);
    }

    public void SpawnPoliceToArrest(NPCController npc)
    {
        if (policeCarPrefab == null || policeSpawnPoint == null) 
        {
            // fallback: instantly remove NPC
            npc.OnArrested();
            return;
        }
        var carGO = Instantiate(policeCarPrefab, policeSpawnPoint.position, policeSpawnPoint.rotation);
        var police = carGO.GetComponent<PoliceCar>();
        if (police)
        {
            police.ChaseAndArrest(npc);
        }
        else
        {
            npc.OnArrested();
        }
    }

    public void OnThiefEscaped(NPCController npc)
    {
        // thief left store
        score = Mathf.Max(0, score - 5);
        money = Mathf.Max(0f, money - moneyLossOnMiss);
        UIManager.Instance.ShowFloatingText("Thief Escaped! -5 score", npc.transform.position);
        UIManager.Instance.UpdateHUD(score, money, reputation);
    }
}
