// Assets/Scripts/Enemies/EnemyHealth.cs
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 30f;
    float current;

    void Start()
    {
        current = maxHealth;
    }

    public void TakeDamage(float dmg)
    {
        current -= dmg;
        if (current <= 0f) Die();
    }

    void Die()
    {
        ScoreManager.Instance?.AddPoints(50); // award points
        Destroy(gameObject);
    }
}

