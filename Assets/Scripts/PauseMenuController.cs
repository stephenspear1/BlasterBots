using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Lightweight Pause Menu controller
/// - Toggle with 'P' (legacy Input)
/// - Shows pausePanel, hides crosshair, unlocks cursor while paused
/// - Disables only gameplay components (safe for UI)
/// - QuitGame() exits application (Editor and build)
/// - PAUSE is blocked while the Upgrade Menu is open.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("Direct refs (preferred)")]
    [Tooltip("Assign the root PausePanel GameObject (set inactive by default).")]
    public GameObject pausePanel;

    [Tooltip("Assign the crosshair GameObject so it can be hidden while paused.")]
    public GameObject crosshair;

    [Header("Fallback lookup (used only if direct refs are null)")]
    public string pausePanelName = "PausePanel";
    public string crosshairName = "Crosshair";

    [Header("Settings")]
    [Tooltip("Key to toggle pause")]
    public KeyCode toggleKey = KeyCode.P;

    // Explicit gameplay component type names we will try to disable when paused.
    readonly string[] gameplayComponentNamesToDisable = new string[]
    {
        "CameraOrbit",
        "PlayerMovement",
        "BlasterController",
        "WaveManager",
        "EnemyAI_Chase",
        "EnemySpawner",
        // add others as needed by exact class name
    };

    // store disabled behaviours to re-enable later
    System.Collections.Generic.List<Behaviour> disabledBehaviours = new System.Collections.Generic.List<Behaviour>();

    bool isPaused = false;

    void Start()
    {
        TryAutoBind();
        ForceUnpauseState();
    }

    void Update()
    {
        // BLOCK pausing if upgrade menu is open
        if (!IsUpgradeMenuOpen() && Input.GetKeyDown(toggleKey))
        {
            if (isPaused) Resume();
            else Pause();
        }

        // allow Escape to resume when paused, but do not override Upgrade Menu
        if (isPaused && Input.GetKeyDown(KeyCode.Escape) && !IsUpgradeMenuOpen())
            Resume();
    }

    /// <summary>
    /// Try to auto-find referenced UI objects by name if inspector refs were not set.
    /// This simple version attempts GameObject.Find (fast) only.
    /// </summary>
    void TryAutoBind()
    {
        if (pausePanel == null && !string.IsNullOrEmpty(pausePanelName))
        {
            var found = GameObject.Find(pausePanelName);
            if (found != null) pausePanel = found;
        }

        if (crosshair == null && !string.IsNullOrEmpty(crosshairName))
        {
            var found = GameObject.Find(crosshairName);
            if (found != null) crosshair = found;
        }
    }

    /// <summary>
    /// Ensure the game is unpaused and cursor/UI state is correct.
    /// </summary>
    void ForceUnpauseState()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (pausePanel != null) pausePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshair != null) crosshair.SetActive(true);

        // Re-enable any previously disabled behaviours (clean up if persisted)
        if (disabledBehaviours.Count > 0)
        {
            for (int i = disabledBehaviours.Count - 1; i >= 0; i--)
            {
                var b = disabledBehaviours[i];
                if (b != null) b.enabled = true;
            }
            disabledBehaviours.Clear();
        }
    }

    /// <summary>
    /// Pause gameplay: show UI, unlock cursor, disable gameplay scripts.
    /// </summary>
    public void Pause()
    {
        // block pause if upgrade menu is open (upgrade mode has priority)
        if (IsUpgradeMenuOpen())
        {
            Debug.Log("[PauseMenuController] Pause blocked: Upgrade menu is open.");
            return;
        }

        if (isPaused) return;
        isPaused = true;

        Time.timeScale = 0f;

        TryAutoBind();
        if (pausePanel != null) pausePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (crosshair != null) crosshair.SetActive(false);

        // Clear previous list
        disabledBehaviours.Clear();

        // 1) Disable explicit known gameplay components by exact type name (safe)
        foreach (var typeName in gameplayComponentNamesToDisable)
        {
            var t = System.Type.GetType(typeName);
            // try with loaded assemblies if direct GetType fails
            if (t == null)
            {
                foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    t = asm.GetType(typeName);
                    if (t != null) break;
                }
            }
            if (t == null) continue;

            var comps = GameObject.FindObjectsOfType(t);
            foreach (var comp in comps)
            {
                if (comp is Behaviour b && b.enabled)
                {
                    // never disable UI behaviours
                    if (b is UnityEngine.EventSystems.UIBehaviour) continue;
                    b.enabled = false;
                    disabledBehaviours.Add(b);
                }
            }
        }

        // 2) Conservative fallback: disable likely gameplay scripts but NEVER UI/EventSystem/Canvas/GraphicRaycaster
        var all = FindObjectsOfType<MonoBehaviour>();
        foreach (var mb in all)
        {
            if (mb == null) continue;
            if (mb == this) continue;

            string n = mb.GetType().Name;

            // skip UI/EventSystem related items
            if (mb is UnityEngine.EventSystems.UIBehaviour) continue;
            if (n.Contains("UI") || n.Contains("Event") || n.Contains("Graphic") || n.Contains("Canvas") || n.Contains("Button")) continue;

            bool looksLikeGameplay = (n.Contains("Player") || n.Contains("Controller") || n.Contains("Movement") || n.Contains("Input") || n.Contains("AI") || n.Contains("Wave") || n.Contains("Spawner"));
            if (!looksLikeGameplay) continue;

            var b = mb as Behaviour;
            if (b != null && b.enabled)
            {
                if (b.GetComponentInParent<Canvas>() != null) continue;
                b.enabled = false;
                disabledBehaviours.Add(b);
            }
        }
    }

    /// <summary>
    /// Resume gameplay: hide UI, lock cursor, re-enable disabled gameplay scripts.
    /// Respect upgrade menu state (do not re-lock cursor if upgrade menu is open).
    /// </summary>
    public void Resume()
    {
        if (!isPaused) return;
        isPaused = false;

        Time.timeScale = 1f;

        if (pausePanel != null) pausePanel.SetActive(false);

        // If upgrade menu is open, do NOT relock cursor or show crosshair
        if (IsUpgradeMenuOpen())
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (crosshair != null) crosshair.SetActive(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (crosshair != null) crosshair.SetActive(true);
        }

        // Re-enable previously disabled behaviours
        for (int i = disabledBehaviours.Count - 1; i >= 0; i--)
        {
            var b = disabledBehaviours[i];
            if (b != null) b.enabled = true;
        }
        disabledBehaviours.Clear();
    }

    /// <summary>
    /// Quit the game (works in Editor and builds).
    /// </summary>
    public void QuitGame()
    {
        // restore state before quitting
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Returns true if the UpgradeManager reports the upgrade UI is open.
    /// Uses reflection to handle either a public property "IsOpen" or a field "IsOpen"/"isOpen".
    /// </summary>
    bool IsUpgradeMenuOpen()
    {
        var um = UpgradeManager.Instance;
        if (um == null) return false;

        var prop = um.GetType().GetProperty("IsOpen");
        if (prop != null && prop.PropertyType == typeof(bool))
        {
            try { return (bool)prop.GetValue(um); }
            catch { }
        }

        var field = um.GetType().GetField("IsOpen", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                 ?? um.GetType().GetField("isOpen", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null && field.FieldType == typeof(bool))
        {
            try { return (bool)field.GetValue(um); }
            catch { }
        }

        return false;
    }

    /// <summary>
    /// Optional: query pause state from other scripts.
    /// </summary>
    public bool IsPaused() => isPaused;
}
