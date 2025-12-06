using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Start()
    {
        // populate spawnPoints from SpawnPoints parent (preferred)
        var spParent = GameObject.Find("SpawnPoints");
        if (spParent != null)
        {
            var childTransforms = spParent.GetComponentsInChildren<Transform>();
            List<Transform> temp = new List<Transform>();
            foreach (var t in childTransforms)
                if (t != spParent.transform) temp.Add(t);
            spawnPoints = temp.ToArray();
        }

        // sanity checks
        if (waves == null || waves.Length == 0)
            Debug.LogError("[WaveManager] No waves defined! Please set up 5 wave entries in the inspector.");

        StartCoroutine(SpawnWaveFromDefinition(currentWaveIndex));
    }

    IEnumerator SpawnWaveFromDefinition(int waveIndex)
    {
        if (spawning) yield break;
        if (waveIndex < 0 || waveIndex >= waves.Length) yield break;

        spawning = true;
        WaveDefinition w = waves[waveIndex];

        UIManager.Instance?.UpdateWave(waveIndex + 1);
        Debug.Log($"[WaveManager] Spawning {w.waveName} (index {waveIndex})");

        yield return new WaitForSeconds(0.5f);

        // spawn normal enemies
        for (int i = 0; i < w.enemyCount; i++)
        {
            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
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
                GameObject boss = Instantiate(w.bossPrefab, sp.position, Quaternion.identity);
                active.Add(boss);
                // optional per-boss config can be set on prefab
                yield return new WaitForSeconds(0.5f);
            }
            UIManager.Instance?.ShowBossWarning();
        }

        spawning = false;
        spawnScheduled = false;

        // Wait for wave completion. Update() schedules next wave after active cleared.
        yield break;
    }

    void Update()
    {
        // cleanup dead entries
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
            StartCoroutine(SpawnWaveFromDefinition(currentWaveIndex));
        }
    }

    IEnumerator EndGameVictory()
    {
        // short delay for clarity
        yield return new WaitForSeconds(0.5f);
        UIManager.Instance?.ShowVictoryScreen();
        // You can call GameManager.Instance.ResetRun() if you want to reset vars when they press Restart
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

    public void ResetToWave1()
    {
        // Stop any spawning coroutines
        StopAllCoroutines();

        // Destroy any active enemies
        foreach (var e in active)
        {
            if (e != null) Destroy(e);
        }
        active.Clear();

        // Reset internal state
        spawning = false;
        spawnScheduled = false;
        currentWaveIndex = 0;

        // Update UI
        UIManager.Instance?.UpdateWave(1);

        // Start first wave
        StartCoroutine(SpawnWaveFromDefinition(currentWaveIndex));
    }
}
