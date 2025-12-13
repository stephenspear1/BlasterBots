using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryController : MonoBehaviour
{
    public static VictoryController Instance;

    [Header("UI refs")]
    public GameObject victoryPanel;       // assign in inspector OR found by name
    public string victoryPanelName = "VictoryPanel";

    [Header("Optional refs")]
    public GameObject crosshair;          // hide during victory

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        TryAutoBind();
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    void TryAutoBind()
    {
        if (victoryPanel == null && !string.IsNullOrEmpty(victoryPanelName))
        {
            // active only
            var found = GameObject.Find(victoryPanelName);
            if (found == null)
            {
                // try inactive search
                var scene = SceneManager.GetActiveScene();
                foreach (var root in scene.GetRootGameObjects())
                {
                    var t = FindChildRecursive(root.transform, victoryPanelName);
                    if (t != null)
                    {
                        found = t.gameObject;
                        break;
                    }
                }
            }
            victoryPanel = found;
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
    /// Called when the run is fully completed.
    /// Shows victory screen + unlocks cursor.
    /// </summary>
    public void ShowVictory()
    {
        TryAutoBind();

        // freeze gameplay
        Time.timeScale = 0f;

        // show panel
        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        // cursor active
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (crosshair != null)
            crosshair.SetActive(false);
    }

    public void OnQuit()
    {
        // ensure consistent state before quitting
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
