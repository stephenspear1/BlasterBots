// Assets/Scripts/Enemies/WaveManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform[] spawnPoints;
    public GameObject basicEnemyPrefab;
    public int baseCount = 5;
    public float spawnDelay = 0.25f;
    public float timeBetweenWaves = 2f;

    [Header("Wave State")]
    public int currentWave = 1;

    // runtime state
    private List<GameObject> active = new List<GameObject>();
    private bool spawning = false;
    private bool spawnScheduled = false; // ensures NextWaveDelay is scheduled once

    void Start()
    {
        // Prefer an explicit SpawnPoints parent to auto-populate spawnPoints
        var spParent = GameObject.Find("SpawnPoints");
        if (spParent != null)
        {
            var childTransforms = spParent.GetComponentsInChildren<Transform>();
            // Exclude the parent transform itself
            List<Transform> temp = new List<Transform>();
            foreach (var t in childTransforms)
                if (t != spParent.transform) temp.Add(t);
            spawnPoints = temp.ToArray();
        }
        else if (spawnPoints == null || spawnPoints.Length == 0)
        {
            // fallback: find by type (unsorted for speed)
            spawnPoints = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        }

        // Start the first wave
        StartCoroutine(SpawnWave(currentWave));
    }

    IEnumerator SpawnWave(int wave)
    {
        // Double-guard: if already spawning, abort
        if (spawning) yield break;
        spawning = true;

        Debug.Log($"[WaveManager] Spawning wave {wave}");
        UIManager.Instance?.UpdateWave(wave);

        // short lead-in so UI updates are visible
        yield return new WaitForSeconds(0.5f);

        int count = Mathf.Max(1, baseCount + (wave - 1) * 2);
        for (int i = 0; i < count; i++)
        {
            Transform sp = null;
            if (spawnPoints != null && spawnPoints.Length > 0)
                sp = spawnPoints[Random.Range(0, spawnPoints.Length)];

            Vector3 spawnPos = sp != null ? sp.position : Vector3.zero;

            GameObject e = Instantiate(basicEnemyPrefab, spawnPos, Quaternion.identity);
            active.Add(e);

            yield return new WaitForSeconds(spawnDelay);
        }

        // finished spawning this wave
        spawning = false;
        spawnScheduled = false; // allow scheduling the next wave after enemies die

        Debug.Log($"[WaveManager] Finished spawning wave {wave}. Active enemies: {active.Count}");

        // increment wave so the next call to SpawnWave uses the next number
        currentWave++;
        yield break;
    }

    void Update()
    {
        // cleanup dead/destroyed entries first
        if (active != null && active.Count > 0)
            active.RemoveAll(e => e == null);

        // If not currently spawning, no active enemies, and we haven't scheduled the next wave,
        // schedule it once so it won't be spammed every frame.
        if (!spawning && (active == null || active.Count == 0) && !spawnScheduled)
        {
            spawnScheduled = true;
            StartCoroutine(NextWaveDelay());
        }
    }

    IEnumerator NextWaveDelay()
    {
        // small delay before starting the next wave (gives player breathing room)
        yield return new WaitForSeconds(timeBetweenWaves);

        // start spawning the next wave (currentWave has already been incremented at end of SpawnWave)
        StartCoroutine(SpawnWave(currentWave));
    }

    // Optional helpers if you want enemies to register/unregister themselves instead
    public void RegisterEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        if (!active.Contains(enemy)) active.Add(enemy);
    }

    public void UnregisterEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        if (active.Contains(enemy)) active.Remove(enemy);
    }
}
