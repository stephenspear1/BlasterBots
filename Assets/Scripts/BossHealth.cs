using UnityEngine;

/// <summary>
/// Boss health component. Mirrors the simple API used by PhotonBolt (TakeDamage(int)) and EnemyHealth.
/// On death it awards a large point value and unregisters itself from the WaveManager list.
/// </summary>
public class BossHealth : MonoBehaviour
{
    [Header("Boss Health")]
    public int maxHealth = 200;
    int currentHealth;

    [Header("Scoring")]
    public int pointsOnDeath = 100;
    public AudioClip popSound;
    public AudioSource audioSource;

    void Start()
    {
        currentHealth = maxHealth;
        // update UI if you show boss HP (optional)
    }

    /// <summary>
    /// Called by projectiles or other damage sources.
    /// </summary>
    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        currentHealth = Mathf.Max(0, currentHealth);

        // Optional: update boss healthbar if you have one
        // BossUI.Instance?.UpdateBossHealth(currentHealth, maxHealth);

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        // Award points
        ScoreManager.Instance?.AddPoints(pointsOnDeath);
        AudioSource.PlayClipAtPoint(popSound, transform.position, 1f);
        // Unregister from WaveManager so it won't be counted as active
        var wm = FindObjectOfType<WaveManager>();
        wm?.UnregisterEnemy(this.gameObject);

        Destroy(gameObject);
    }

    // Optional helper used by WaveManager if you want to set health at spawn time
    public void SetMaxHealth(int newMax)
    {
        maxHealth = newMax;
        currentHealth = maxHealth;
    }
}
