using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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
    public UnityEvent<int, int> OnTaskCompleted;

    [Header("UI Elements")]
    [SerializeField] private GameObject accusationPanel;
    [SerializeField] private TMP_Text accusationText;
    [SerializeField] private Button accuseButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private GameObject endingPanel;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text reputationText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text endingScoreText;
    [SerializeField] private TMP_Text endingMoneyText;
    [SerializeField] private TMP_Text endingReputationText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private GameObject taskPanel;
    [SerializeField] private TMP_Text taskText;

    [Header("NPC System")]
    public NPCBase CurrentNPC { get; private set; }

    [Header("Game Balance")]
    [SerializeField] private float wrongAccusationPenalty = 10f;
    [SerializeField] private float theftAmount = 50f;
    [SerializeField] private int successfulAccusationReward = 100;

    [Header("Police System")]
    [SerializeField] private GameObject policeCarPrefab;
    [SerializeField] private Transform stallPosition;
    [SerializeField] private Transform policeSpawnPosition;
    [SerializeField] private Transform policeExitPosition;

    [Header("Game Timer")]
    [SerializeField] private float gameDuration = 540f; // 9 minutes
    private float remainingTime;

    [Header("Task System")]
    [SerializeField] private Transform[] taskShelves;
    [SerializeField] private int totalTasks = 3;
    [SerializeField] private AudioSource taskAudioSource;
    [SerializeField] private AudioClip taskCompleteClip;
    private int tasksCompleted = 0;
    private bool[] taskShelfCompleted;

    [Header("Rain System")]
    [SerializeField] private ParticleSystem rainParticleSystem;
    [SerializeField] private AudioSource rainAudioSource;
    [SerializeField] private AudioClip rainAudioClip;
    [SerializeField] private float rainInterval = 180f; // Rain every 3 minutes
    [SerializeField] private float rainDuration = 30f; // Rain lasts 30 seconds
    private float rainTimer;

    [Header("Background Music")]
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioClip backgroundMusicClip;

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
        remainingTime = gameDuration;
        tasksCompleted = 0;
        taskShelfCompleted = new bool[taskShelves.Length];
        rainTimer = rainInterval;

        OnScoreChanged = new UnityEvent<int>();
        OnMoneyChanged = new UnityEvent<float>();
        OnReputationChanged = new UnityEvent<float>();
        OnGameOver = new UnityEvent<string>();
        OnTaskCompleted = new UnityEvent<int, int>();

        // Setup UI listeners
        if (accuseButton != null) accuseButton.onClick.AddListener(AccuseCurrentNPC);
        if (leaveButton != null) leaveButton.onClick.AddListener(LeaveCurrentNPC);
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);

        // Initialize UI
        if (accusationPanel != null) accusationPanel.SetActive(false);
        if (endingPanel != null) endingPanel.SetActive(false);
        if (taskPanel != null) taskPanel.SetActive(true);
        UpdateUI();

        // Initialize rain
        if (rainParticleSystem != null) rainParticleSystem.Stop();
        if (rainAudioSource != null && rainAudioClip != null)
        {
            rainAudioSource.clip = rainAudioClip;
            rainAudioSource.loop = true;
            rainAudioSource.Stop();
        }

        // Play background music
        if (backgroundMusicSource != null && backgroundMusicClip != null)
        {
            backgroundMusicSource.clip = backgroundMusicClip;
            backgroundMusicSource.loop = true;
            backgroundMusicSource.Play();
        }
    }

    void Update()
    {
        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            UpdateUI();
            UpdateRain();
            if (remainingTime <= 0)
            {
                ShowEndingScreen();
            }
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
        if (moneyText != null) moneyText.text = $"Money: ${money:F2}";
        if (reputationText != null) reputationText.text = $"Reputation: {reputation:F1}";
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timerText.text = $"Time: {minutes:D2}:{seconds:D2}";
        }
        if (taskText != null)
        {
            taskText.text = $"Tasks Completed: {tasksCompleted}/{totalTasks}";
        }
    }

    private void UpdateRain()
    {
        rainTimer -= Time.deltaTime;
        if (rainTimer <= 0)
        {
            if (rainParticleSystem != null && !rainParticleSystem.isPlaying)
            {
                rainParticleSystem.Play();
                if (rainAudioSource != null) rainAudioSource.Play();
                Debug.Log("Rain started");
            }
            if (rainTimer <= -rainDuration)
            {
                if (rainParticleSystem != null) rainParticleSystem.Stop();
                if (rainAudioSource != null) rainAudioSource.Stop();
                rainTimer = rainInterval;
                Debug.Log("Rain stopped");
            }
        }
    }

    public void SetCurrentNPC(NPCBase npc)
    {
        CurrentNPC = npc;
        if (accusationPanel != null && accusationText != null)
        {
            accusationText.text = "Accuse this person of theft?";
            accusationPanel.SetActive(true);
            Time.timeScale = 0f;
        }
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
        if (accusationPanel != null) accusationPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void LeaveCurrentNPC()
    {
        ClearCurrentNPC();
        if (accusationPanel != null) accusationPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void ClearCurrentNPC()
    {
        CurrentNPC = null;
    }

    public void OnTheftDetected(NPCBase npc)
    {
        Debug.Log($"Theft detected by {npc.name} (Type: {npc.npcType})!");
    }

    public void OnTheftSuccessful(NPCBase npc)
    {
        ModifyMoney(-theftAmount);
        npc.hasStolen = true;
        Debug.Log($"Theft successful! Money: {money}");
        CheckGameOver();
    }

    public void OnSuccessfulAccusation(NPCBase npc)
    {
        ModifyScore(successfulAccusationReward);
        Debug.Log($"Successful accusation! Score: {score}");

        if (policeCarPrefab != null && stallPosition != null && policeSpawnPosition != null && policeExitPosition != null)
        {
            GameObject policeCar = Instantiate(policeCarPrefab, policeSpawnPosition.position, Quaternion.identity);
            PoliceCar policeScript = policeCar.GetComponent<PoliceCar>();
            if (policeScript != null)
            {
                policeScript.Initialize(npc, stallPosition.position, policeExitPosition.position);
            }
        }
    }

    public void OnWrongAccusation()
    {
        ModifyReputation(-wrongAccusationPenalty);
        Debug.Log($"Wrong accusation! Reputation: {reputation}");
        CheckGameOver();
    }

    public void ModifyScore(int amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score);
        UpdateUI();
    }

    public void ModifyMoney(float amount)
    {
        money += amount;
        money = Mathf.Max(0, money);
        OnMoneyChanged?.Invoke(money);
        UpdateUI();
    }

    public void ModifyReputation(float amount)
    {
        reputation += amount;
        reputation = Mathf.Clamp(reputation, 0, 100);
        OnReputationChanged?.Invoke(reputation);
        UpdateUI();
    }

    private void CheckGameOver()
    {
        if (money <= 0)
        {
            OnGameOver?.Invoke("Game Over: Store closed due to bankruptcy!");
            ShowEndingScreen();
        }
        else if (reputation <= 0)
        {
            OnGameOver?.Invoke("Game Over: Fired due to low reputation!");
            ShowEndingScreen();
        }
    }

    private void ShowEndingScreen()
    {
        if (endingPanel != null)
        {
            endingPanel.SetActive(true);
            if (endingScoreText != null) endingScoreText.text = $"Score: {score}";
            if (endingMoneyText != null) endingMoneyText.text = $"Money: ${money:F2}";
            if (endingReputationText != null) endingReputationText.text = $"Reputation: {reputation:F1}";
            Time.timeScale = 0f;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("03_Street");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Scene 1"); // Updated to load Scene 1
    }

    public void HandleTaskCompletion(Transform shelf)
    {
        for (int i = 0; i < taskShelves.Length; i++)
        {
            if (taskShelves[i] == shelf && !taskShelfCompleted[i])
            {
                taskShelfCompleted[i] = true;
                tasksCompleted++;
                OnTaskCompleted?.Invoke(tasksCompleted, totalTasks);
                UpdateUI();
                if (taskAudioSource != null && taskCompleteClip != null)
                {
                    taskAudioSource.PlayOneShot(taskCompleteClip);
                }
                Debug.Log($"Task completed: Restocked shelf {i + 1}. Total tasks: {tasksCompleted}/{totalTasks}");
                break;
            }
        }
    }

    public bool IsTaskShelf(Transform shelf)
    {
        for (int i = 0; i < taskShelves.Length; i++)
        {
            if (taskShelves[i] == shelf && !taskShelfCompleted[i])
            {
                return true;
            }
        }
        return false;
    }

    public int Score => score;
    public float Money => money;
    public float Reputation => reputation;
}