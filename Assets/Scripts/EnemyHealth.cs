// Assets/Scripts/Enemies/EnemyHealth.cs
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 30f;
    public int pointsAwarded = 10;
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
        ScoreManager.Instance?.AddPoints(pointsAwarded); // award points
        Destroy(gameObject);
    }
}

