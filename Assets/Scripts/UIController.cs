using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text reputationText;
    [SerializeField] private GameObject endingPanel;
    [SerializeField] private TMP_Text endingScoreText;
    [SerializeField] private TMP_Text endingMoneyText;
    [SerializeField] private TMP_Text endingReputationText;

    private void Start()
    {
        if (endingPanel != null) endingPanel.SetActive(false);

        UpdateScore(GameManager.Instance.Score);
        UpdateMoney(GameManager.Instance.Money);
        UpdateReputation(GameManager.Instance.Reputation);

        GameManager.Instance.OnScoreChanged.AddListener(UpdateScore);
        GameManager.Instance.OnMoneyChanged.AddListener(UpdateMoney);
        GameManager.Instance.OnReputationChanged.AddListener(UpdateReputation);
        GameManager.Instance.OnGameOver.AddListener(OnGameOver);
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
    }

    private void UpdateMoney(float money)
    {
        if (moneyText != null) moneyText.text = $"Money: ${money:F2}";
    }

    private void UpdateReputation(float reputation)
    {
        if (reputationText != null) reputationText.text = $"Reputation: {reputation:F1}";
    }

    private void OnGameOver(string message)
    {
        DisplayEndingStats(GameManager.Instance.Score, GameManager.Instance.Money, GameManager.Instance.Reputation);
    }

    public void DisplayEndingStats(int score, float money, float reputation)
    {
        if (endingPanel != null)
        {
            if (endingScoreText != null) endingScoreText.text = $"Score: {score}";
            if (endingMoneyText != null) endingMoneyText.text = $"Money: ${money:F2}";
            if (endingReputationText != null) endingReputationText.text = $"Reputation: {reputation:F1}";
            endingPanel.SetActive(true);
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
        SceneManager.LoadScene("Menu");
    }
}