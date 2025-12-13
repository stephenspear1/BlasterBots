using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject aboutPanel;
    public GameObject howToPanel;

    [Header("Settings")]
    public string gameSceneName = "Gameplay"; // set to your gameplay scene name

    void Start()
    {
        // Ensure panels are in the correct initial state
        ShowMainMenu();

        // Make cursor visible for menus
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // ensure the game isn't paused when in menu
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (aboutPanel.activeSelf || howToPanel.activeSelf) ShowMainMenu();
            else QuitGame();
        }
    }

    // Button hookup methods
    public void ShowMainMenu()
    {
        mainMenuPanel?.SetActive(true);
        aboutPanel?.SetActive(false);
        howToPanel?.SetActive(false);
    }

    public void ShowAbout()
    {
        mainMenuPanel?.SetActive(false);
        aboutPanel?.SetActive(true);
        howToPanel?.SetActive(false);
    }

    public void ShowHowTo()
    {
        mainMenuPanel?.SetActive(false);
        aboutPanel?.SetActive(false);
        howToPanel?.SetActive(true);
    }

    public void PlayGame()
    {
        // optional: disable audio/music and start loading
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // If you use async loading, you can show a loading screen here.
        // For now load scene normally:
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("[MainMenuController] gameSceneName not set.");
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        // Stop play mode in Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
