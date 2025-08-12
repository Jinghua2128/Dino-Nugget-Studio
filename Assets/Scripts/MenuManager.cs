using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenu;
    public GameObject settingsMenu;

    [Header("Scene To Load")]
    public string sceneToLoad = "03_Street";

    void Start()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
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
