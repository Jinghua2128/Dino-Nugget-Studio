// UIManager.cs
// Minimal UI manager to update HUD and show floating text.
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI reputationText;
    public GameObject floatingTextPrefab;
    public Canvas worldCanvas;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void UpdateHUD(int score, float money, float reputation)
    {
        if (scoreText) scoreText.text = "Score: " + score;
        if (moneyText) moneyText.text = "Money: $" + Mathf.RoundToInt(money);
        if (reputationText) reputationText.text = "Reputation: " + Mathf.RoundToInt(reputation);
    }

    public void ShowFloatingText(string text, Vector3 worldPos)
    {
        if (floatingTextPrefab == null) return;
        var go = Instantiate(floatingTextPrefab, worldCanvas.transform);
        go.transform.position = Camera.main.WorldToScreenPoint(worldPos + Vector3.up * 1.6f);
        var t = go.GetComponentInChildren<Text>();
        if (t) t.text = text;
        Destroy(go, 1.6f);
    }
}
