using UnityEngine;

/// <summary>
/// HealthSystem with delayed automatic regeneration.
/// - Starts regen after `baseRegenDelay` seconds without taking damage.
/// - Regenerates at `shieldRegenRate` HP per second while below maxHealth.
/// - Call TakeDamage(...) to apply damage (resets regen timer).
/// - Call Heal(...) to instantly heal and update UI.
/// - Use ApplyRegenTier(int tiers) (or SetRegenDelay(float)) from UpgradeManager to modify regen behavior.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth; // visible in inspector if you have a ReadOnly attribute; otherwise shows normally

    [Header("Regeneration")]
    [Tooltip("Default delay (seconds) after taking damage before regen starts.")]
    public float baseRegenDelay = 7f;      // default 7 seconds
    [Tooltip("Current regen delay used by system (can be reduced by upgrades)")]
    public float regenDelay;              // runtime effective delay (initializes from baseRegenDelay)
    [Tooltip("Health per second restored during regeneration.")]
    public float shieldRegenRate = 10f;   // 10 HP / sec default

    float timeSinceDamage = 0f;
    bool isRegenerating = false;

    void Awake()
    {
        // initialize runtime values
        regenDelay = baseRegenDelay;
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    void Update()
    {
        // Do nothing if at full health
        if (currentHealth >= maxHealth)
        {
            // still reset regen flags so UI stays correct
            isRegenerating = false;
            timeSinceDamage = 0f;
            return;
        }

        // Count time since last damage using scaled time (pause should pause regen)
        timeSinceDamage += Time.deltaTime;

        // Start regenerating after regenDelay
        if (!isRegenerating && timeSinceDamage >= regenDelay)
        {
            isRegenerating = true;
        }

        // Regenerate while flagged
        if (isRegenerating)
        {
            float healThisFrame = shieldRegenRate * Time.deltaTime;
            currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.CeilToInt(healThisFrame));
            UpdateUI();

            // If we reached max, stop regen
            if (currentHealth >= maxHealth)
            {
                isRegenerating = false;
                timeSinceDamage = 0f;
            }
        }
    }

    public void TakeDamage(float dmg)
    {
        int idmg = Mathf.CeilToInt(dmg);
        currentHealth -= idmg;
        currentHealth = Mathf.Max(0, currentHealth);

        // reset regen timer and stop regenerating immediately
        timeSinceDamage = 0f;
        isRegenerating = false;

        UpdateUI();

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(float amt)
    {
        int iamt = Mathf.CeilToInt(amt);
        currentHealth = Mathf.Min(maxHealth, currentHealth + iamt);
        UpdateUI();
    }

    public void RestoreToFullHealth()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    // Replace Die() implementation (or add call) so it notifies the GameManager
    void Die()
    {
        Debug.Log("[HealthSystem] Player died.");
        // Notify GameManager to handle death + restart logic
        GameManager.Instance?.HandlePlayerDeath();
    }

    void UpdateUI()
    {
        UIManager.Instance?.UpdateHealth((float)currentHealth, (float)maxHealth);
    }

    // ---------- Upgrade-friendly API ----------

    /// <summary>
    /// Apply an "energy regulator" tier upgrade.
    /// Each tier will increase regen rate by 25% and shorten the delay proportionally.
    /// For example: tiers = 1 => regenRate *= 1.25 ; regenDelay /= 1.25
    /// This keeps the effective "time to full" shorter.
    /// </summary>
    public void ApplyRegenTier(int tiers)
    {
        if (tiers <= 0) return;

        float multiplier = Mathf.Pow(1.25f, tiers); // 25% faster per tier
        shieldRegenRate *= multiplier;
        regenDelay = Mathf.Max(0.1f, regenDelay / multiplier); // never let delay fall to zero: clamp to 0.1s
        Debug.Log($"[HealthSystem] ApplyRegenTier({tiers}) -> regenRate={shieldRegenRate}, regenDelay={regenDelay}");
    }

    /// <summary>
    /// Directly set the effective regen delay (seconds).
    /// Use if you want more direct control from UpgradeManager.
    /// </summary>
    public void SetRegenDelay(float seconds)
    {
        regenDelay = Mathf.Max(0f, seconds);
    }

    /// <summary>
    /// Directly set the regen rate (hp per second).
    /// </summary>
    public void SetRegenRate(float hpPerSec)
    {
        shieldRegenRate = Mathf.Max(0f, hpPerSec);
    }

    /// <summary>
    /// Resets regen values to defaults (baseRegenDelay and a default regenRate).
    /// Useful when restarting a run.
    /// </summary>
    public void ResetRegenToDefaults(float defaultRegenRate = 10f)
    {
        regenDelay = baseRegenDelay;
        shieldRegenRate = defaultRegenRate;
    }
    
    public void ResetToDefault()
{
    // Restore base stats
    currentHealth = maxHealth;
    regenDelay = baseRegenDelay;
    shieldRegenRate = 10f;   // default regen per second (adjust if you want)

    // Reset regen state
    timeSinceDamage = 0f;
    isRegenerating = false;

    // Update UI
    UpdateUI();

    Debug.Log("[HealthSystem] ResetToDefault() applied.");
}
}
