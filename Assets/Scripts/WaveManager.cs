// Assets/Scripts/WaveManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class WaveDefinition
{
    public string waveName = "Wave";
    public GameObject enemyPrefab;
    public int enemyCount = 5;
    public float enemySpeed = -1f;   // -1 = don't override
    public float enemyDamage = -1f;
    public float enemyHealth = -1f;

    // boss
    public bool spawnBoss = false;
    public GameObject bossPrefab;
    public int bossCount = 0;
    public float bossSpawnDelayAfter = 1.0f;
}

public class WaveManager : MonoBehaviour
{
    [Header("Spawn")]
    public Transform[] spawnPoints;

    [Header("Waves (define 5)")]
    public WaveDefinition[] waves; // assign 5 in inspector

    [Header("General")]
    public float spawnDelay = 0.25f;
    public float timeBetweenWaves = 2f;

    private List<GameObject> active = new List<GameObject>();
    private bool spawning = false;
    private bool spawnScheduled = false;
    private int currentWaveIndex = 0; // 0-based; wave 1 = index 0

    Coroutine runningSpawner = null;

    void Awake()
    {
        // listen for scene loads so persistent instances can rebind
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        EnsureSpawnPoints();
        if (waves == null || waves.Length == 0)
            Debug.LogError("[WaveManager] No waves defined! Please set up waves in the inspector.");

        // Start the first wave only if we have spawnPoints and waves defined
        if (HasValidSpawnPoints() && waves != null && waves.Length > 0)
        {
            runningSpawner = StartCoroutine(SpawnWaveFromDefinition(currentWaveIndex));
        }
        else
        {
            Debug.LogWarning("[WaveManager] Not starting spawner: no valid spawn points or waves missing.");
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // rebind scene-specific references if required (useful if this manager is persistent)
        Debug.Log("[WaveManager] Scene loaded - rebinding spawn points.");
        EnsureSpawnPoints();

        // reset state when scene changes so we don't start mid-wave
        StopAllSpawning();
        active.Clear();
        currentWaveIndex = 0;
        spawnScheduled = false;
        spawning = false;

        if (HasValidSpawnPoints() && waves != null && waves.Length > 0)
        {
            runningSpawner = StartCoroutine(SpawnWaveFromDefinition(currentWaveIndex));
        }
    }

    /// <summary>
    /// Ensure spawnPoints is populated by looking for a parent named "SpawnPoints"
    /// or by finding GameObjects tagged "SpawnPoint". Accepts only non-null Transforms.
    /// </summary>
    void EnsureSpawnPoints()
    {
        // prune any null entries if inspector had references to destroyed Transforms
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            List<Transform> tmp = new List<Transform>();
            foreach (var t in spawnPoints)
                if (t != null) tmp.Add(t);
            spawnPoints = tmp.ToArray();
        }

        // If still empty, try to find a parent container "SpawnPoints" in scene
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            var spParent = GameObject.Find("SpawnPoints");
            if (spParent != null)
            {
                var children = spParent.GetComponentsInChildren<Transform>();
                List<Transform> tmp = new List<Transform>();
                foreach (var t in children)
                    if (t != spParent.transform) tmp.Add(t);
                spawnPoints = tmp.ToArray();
            }
        }

        // fallback: look for objects tagged "SpawnPoint"
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            var tagged = GameObject.FindGameObjectsWithTag("SpawnPoint");
            if (tagged != null && tagged.Length > 0)
            {
                List<Transform> tmp = new List<Transform>();
                foreach (var go in tagged) if (go != null) tmp.Add(go.transform);
                spawnPoints = tmp.ToArray();
            }
        }

        if (!HasValidSpawnPoints())
        {
            Debug.LogWarning("[WaveManager] No valid spawn points found in scene. Please create a parent named 'SpawnPoints' with child Transforms or tag spawn point GameObjects with 'SpawnPoint'.");
        }
        else
        {
            Debug.Log($"[WaveManager] Found {spawnPoints.Length} spawn points.");
        }
    }

    bool HasValidSpawnPoints()
    {
        return spawnPoints != null && spawnPoints.Length > 0;
    }

    IEnumerator SpawnWaveFromDefinition(int waveIndex)
    {
        if (spawning) yield break;
        if (waveIndex < 0 || waveIndex >= waves.Length) yield break;

        // guard: ensure we have valid spawn points
        if (!HasValidSpawnPoints())
        {
            Debug.LogWarning("[WaveManager] SpawnWave aborted: no spawn points.");
            yield break;
        }

        spawning = true;
        WaveDefinition w = waves[waveIndex];

        UIManager.Instance?.UpdateWave(waveIndex + 1);
        Debug.Log($"[WaveManager] Spawning {w.waveName} (index {waveIndex})");

        yield return new WaitForSeconds(0.5f);

        // spawn normal enemies
        for (int i = 0; i < w.enemyCount; i++)
        {
            // pick a random valid spawn transform
            Transform sp = null;
            int tries = 0;
            while (tries < 10 && (sp == null || sp == spawnPoints[Mathf.Clamp(Random.Range(0, spawnPoints.Length), 0, spawnPoints.Length - 1)] && sp == null))
            {
                sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
                tries++;
            }

            if (sp == null)
            {
                Debug.LogWarning("[WaveManager] Couldn't find a non-null spawn point for enemy. Skipping spawn.");
                continue;
            }

            GameObject go = Instantiate(w.enemyPrefab, sp.position, Quaternion.identity);

            // apply per-wave overrides if components exist
            var ai = go.GetComponent<EnemyAI_Chase>();
            if (ai != null)
            {
                if (w.enemySpeed > 0f) ai.moveSpeed = w.enemySpeed;
                if (w.enemyDamage > 0f) ai.damage = w.enemyDamage;
            }
            var eh = go.GetComponent<EnemyHealth>();
            if (eh != null && w.enemyHealth > 0f) eh.maxHealth = w.enemyHealth;

            active.Add(go);
            yield return new WaitForSeconds(spawnDelay);
        }

        // spawn bosses if flagged
        if (w.spawnBoss && w.bossPrefab != null)
        {
            yield return new WaitForSeconds(w.bossSpawnDelayAfter);
            for (int b = 0; b < w.bossCount; b++)
            {
                Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
                if (sp == null)
                {
                    Debug.LogWarning("[WaveManager] Boss spawn point was null, skipping boss spawn.");
                    continue;
                }
                GameObject boss = Instantiate(w.bossPrefab, sp.position, Quaternion.identity);
                active.Add(boss);
                yield return new WaitForSeconds(0.5f);
            }
        }

        spawning = false;
        spawnScheduled = false;
        runningSpawner = null;
        yield break;
    }

    void Update()
    {
        // cleanup null/destroyed entries
        active.RemoveAll(e => e == null);

        // If no active enemies and not spawning and we didn't already schedule next wave
        if (!spawning && (active == null || active.Count == 0) && !spawnScheduled)
        {
            spawnScheduled = true;

            // if we just completed last wave (index == waves.Length -1) => Victory
            if (currentWaveIndex >= waves.Length - 1)
            {
                StartCoroutine(EndGameVictory());
                return;
            }

            StartCoroutine(NextWaveDelay());
        }
    }

    IEnumerator NextWaveDelay()
    {
        yield return new WaitForSeconds(timeBetweenWaves);

        // between waves: open upgrade menu
        UpgradeManager.Instance?.OpenMenu(); // pauses timeScale = 0
        // When UpgradeManager closes, it should unpause and call ContinueToNextWave() below
    }

    // Called by UpgradeManager when player finishes upgrades and presses "Continue"
    public void ContinueToNextWave()
    {
        if (currentWaveIndex < waves.Length - 1)
        {
            currentWaveIndex++;
            if (runningSpawner != null) StopCoroutine(runningSpawner);
            runningSpawner = StartCoroutine(SpawnWaveFromDefinition(currentWaveIndex));
        }
    }

    IEnumerator EndGameVictory()
    {
        // short delay for clarity
        yield return new WaitForSeconds(0.5f);
        VictoryController.Instance?.ShowVictory();
        yield break;
    }

    // Optional explicit registration if enemies spawn by other means
    public void RegisterEnemy(GameObject enemy)
    {
        if (!active.Contains(enemy)) active.Add(enemy);
    }

    public void UnregisterEnemy(GameObject enemy)
    {

        if (active.Contains(enemy)) active.Remove(enemy);
    }

    // stop and cleanup any running coroutines used for spawning
    void StopAllSpawning()
    {
        if (runningSpawner != null)
        {
            StopCoroutine(runningSpawner);
            runningSpawner = null;
        }
        spawning = false;
        spawnScheduled = false;
    }

    public void ResetToWave1()
    {
        Debug.Log("[WaveManager] ResetToWave1() called.");

        // stop any running spawners/coroutines
        StopAllSpawning();

        // clear active tracking and destroy leftover enemies if any
        for (int i = active.Count - 1; i >= 0; i--)
        {
            var go = active[i];
            if (go != null) Destroy(go);
        }
        active.Clear();

        // reset internal state
        spawning = false;
        spawnScheduled = false;
        currentWaveIndex = 0;

        // ensure spawnPoints are valid for this scene
        EnsureSpawnPoints();

        // start first wave if valid
        if (HasValidSpawnPoints() && waves != null && waves.Length > 0)
        {
            runningSpawner = StartCoroutine(SpawnWaveFromDefinition(currentWaveIndex));
        }
        else
        {
            Debug.LogWarning("[WaveManager] ResetToWave1 aborted - no valid spawn points or waves.");
        }
    }
}
