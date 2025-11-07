using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public float maxHealth = 100f;
    float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
    }

    void Update()
{
    // --- TEST CODE ONLY ---
    // Press H to take 10 damage
    if (Input.GetKeyDown(KeyCode.H))
    {
        Debug.Log("[HealthSystem] H pressed -> TakeDamage(10)");
        TakeDamage(10f);
    }

    // Press J to heal 10 health
    if (Input.GetKeyDown(KeyCode.J))
    {
        Heal(10f);
    }
}

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        currentHealth = Mathf.Max(0, currentHealth);
        UIManager.Instance.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log("Player died");
        // Later: trigger restart or game over UI
    }

    public void Heal(float amt)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amt);
        UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
    }
}

