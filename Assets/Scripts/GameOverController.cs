using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Shows a Game Over panel and handles Retry/Quit actions.
/// - Retry will reset waves back to wave 1 but keeps upgrades/score (WaveManager.ResetToWave1)
/// - Quit exits application (Editor and build)
/// - Call GameOverController.Instance.ShowGameOver() when player dies.
/// </summary>
public class GameOverController : MonoBehaviour
{
    public static GameOverController Instance;

    [Header("UI refs (preferred)")]
    public GameObject gameOverPanel;   // root panel (inactive by default)
    public string gameOverPanelName = "GameOverPanel";

    [Header("Optional refs")]
    public GameObject crosshair;       // will hide when game over

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        TryAutoBind();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    void TryAutoBind()
    {
        if (gameOverPanel == null && !string.IsNullOrEmpty(gameOverPanelName))
        {
            // find active first
            var go = GameObject.Find(gameOverPanelName);
            if (go == null)
            {
                // find inactive by searching root objects
                var scene = SceneManager.GetActiveScene();
                foreach (var root in scene.GetRootGameObjects())
                {
                    var t = FindChildRecursive(root.transform, gameOverPanelName);
                    if (t != null) { go = t.gameObject; break; }
                }
            }
            if (go != null) gameOverPanel = go;
        }

        if (crosshair == null)
        {
            var ch = GameObject.Find("Crosshair");
            if (ch != null) crosshair = ch;
        }
    }

    Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            var r = FindChildRecursive(parent.GetChild(i), name);
            if (r != null) return r;
        }
        return null;
    }

    /// <summary>
    /// Call this when the player dies (e.g. from HealthSystem.Die()).
    /// </summary>
    public void ShowGameOver()
    {
        TryAutoBind();

        // Pause game
        Time.timeScale = 0f;

        // Show UI
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        // Show cursor and release
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (crosshair != null) crosshair.SetActive(false);
    }

    /// <summary>
    /// Retry: reset to wave 1 but preserve upgrades and score.
    /// Also restore player health to full.
    /// </summary>
    public void OnRetry()
    {
        // Re-enable time for logic while we reset (we will pause/unpause briefly)
        Time.timeScale = 1f;

        // Reset waves (WaveManager must implement ResetToWave1())
        var wm = FindObjectOfType<WaveManager>();
        if (wm != null) wm.ResetToWave1();
        else Debug.LogWarning("[GameOverController] WaveManager not found when retrying.");

        // Restore player health to full
        var hs = FindObjectOfType<HealthSystem>();
        if (hs != null)
        {
            // Heal to max using public maxHealth (HealthSystem has Heal(float))
            hs.Heal(hs.maxHealth);
        }
        else
        {
            Debug.LogWarning("[GameOverController] HealthSystem not found when retrying.");
        }

        // Hide UI and resume gameplay
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshair != null) crosshair.SetActive(true);

        // Ensure time is running
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Quit the application (Editor and build).
    /// </summary>
    public void OnQuit()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
