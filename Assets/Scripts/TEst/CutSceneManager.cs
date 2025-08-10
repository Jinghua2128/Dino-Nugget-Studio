using UnityEngine;
using UnityEngine.UI;

// Cut Scene Manager
public class CutSceneManager : MonoBehaviour
{
    [Header("Cut Scene Triggers")]
    public int lowMoneyThreshold = 200;
    public int midMoneyThreshold = 0;
    public int highMoneyLossThreshold = -500;
    
    [Header("Cut Scene UI")]
    public GameObject cutScenePanel;
    public Text cutSceneText;
    public Button continueButton;
    
    private GameManager gameManager;
    private bool lowMoneyCutScenePlayed = false;
    private bool midMoneyCutScenePlayed = false;
    private bool highLossCutScenePlayed = false;
    
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        
        if (cutScenePanel != null)
            cutScenePanel.SetActive(false);
        
        if (continueButton != null)
            continueButton.onClick.AddListener(CloseCutScene);
    }
    
    void Update()
    {
        CheckCutSceneTriggers();
    }
    
    void CheckCutSceneTriggers()
    {
        if (gameManager == null) return;
        
        if (gameManager.money <= lowMoneyThreshold && !lowMoneyCutScenePlayed)
        {
            PlayLowMoneyCutScene();
        }
        else if (gameManager.money <= midMoneyThreshold && !midMoneyCutScenePlayed)
        {
            PlayMidMoneyCutScene();
        }
        else if (gameManager.money <= highMoneyLossThreshold && !highLossCutScenePlayed)
        {
            PlayHighLossCutScene();
        }
    }
    
    void PlayLowMoneyCutScene()
    {
        lowMoneyCutScenePlayed = true;
        ShowCutScene("Your manager scolds you for the losses. Your pay has been docked, and you'll have to go hungry tonight. Be more careful!");
    }
    
    void PlayMidMoneyCutScene()
    {
        midMoneyCutScenePlayed = true;
        ShowCutScene("You've been fired due to excessive losses. You've lost your source of income. Game Over.");
    }
    
    void PlayHighLossCutScene()
    {
        highLossCutScenePlayed = true;
        ShowCutScene("The store has to close due to lack of profit from your poor performance. Everyone loses their jobs. Game Over.");
    }
    
    void ShowCutScene(string message)
    {
        if (cutScenePanel != null && cutSceneText != null)
        {
            cutScenePanel.SetActive(true);
            cutSceneText.text = message;
            Time.timeScale = 0f; // Pause game
        }
    }
    
    void CloseCutScene()
    {
        if (cutScenePanel != null)
        {
            cutScenePanel.SetActive(false);
            Time.timeScale = 1f; // Resume game
        }
    }
}