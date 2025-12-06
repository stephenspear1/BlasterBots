using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles resetting the run and restarting the scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Transform playerSpawnPoint; // assign in inspector (where player should respawn on wave 1)
    public int startingPoints = 0;   // points to reset to on new run

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Reset upgrades/stats and reload the scene to start over.
    /// Hook the Restart button on the victory panel to this.
    /// </summary>
    public void ResetRun()
    {
        ScoreManager.Instance?.SetPoints(startingPoints);

        var pm = FindObjectOfType<PlayerMovement>();
        if (pm != null) pm.ResetToDefault();

        var hs = FindObjectOfType<HealthSystem>();
        if (hs != null) hs.ResetToDefault();

        var bc = FindObjectOfType<BlasterController>();
        if (bc != null) bc.ResetToDefault();

        var um = FindObjectOfType<UpgradeManager>();
        if (um != null)
        {
            um.turboTier = um.platingTier = um.regulatorTier = 0;
            um.rapidPulseTier = um.amplifierTier = um.multiShotTier = um.coolingVentsBought = 0;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void HandlePlayerDeath()
    {
        // Optionally show death UI / sound here
        Debug.Log("[GameManager] Player died -> Restarting to wave 1 (keeping upgrades & score)");

        // 1) Reset wave manager to wave 1
        var wm = FindObjectOfType<WaveManager>();
        wm?.ResetToWave1();

        // 2) Reposition player and restore health
        var player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            // move player to spawn point (if assigned)
            if (playerSpawnPoint != null)
                player.transform.position = playerSpawnPoint.position;

            // restore health to full WITHOUT resetting upgrades
            var hs = player.GetComponent<HealthSystem>() ?? FindObjectOfType<HealthSystem>();
            hs?.RestoreToFullHealth();

            // optionally reset player's velocity if using Rigidbody
            var rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // 3) Unpause if necessary and hide any death UI (ensure game is running)
        Time.timeScale = 1f;
    }
}
