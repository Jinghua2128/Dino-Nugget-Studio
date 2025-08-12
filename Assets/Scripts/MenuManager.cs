using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenu;
    public GameObject settingsMenu;

    [Header("Scene To Load")]
    public string sceneToLoad = "03_Street";

    [Header("Audio Settings")]
    public AudioSource backgroundMusicSource;
    public AudioClip backgroundMusicClip;

    void Start()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);

        // Initialize background music
        if (backgroundMusicSource != null && backgroundMusicClip != null)
        {
            backgroundMusicSource.clip = backgroundMusicClip;
            backgroundMusicSource.loop = true;
            backgroundMusicSource.Play();
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public void OpenSettings()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void BackToMainMenu()
    {
        settingsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void ExitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit();
    }
}