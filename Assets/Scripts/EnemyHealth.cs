// Assets/Scripts/Enemies/EnemyHealth.cs
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 30f;
    public int pointsAwarded = 10;
    float current;

    public AudioClip popSound;
    public AudioSource audioSource;
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
        AudioSource.PlayClipAtPoint(popSound, transform.position, 1f);
        ScoreManager.Instance?.AddPoints(pointsAwarded); // award points
        Destroy(gameObject);
    }
}

