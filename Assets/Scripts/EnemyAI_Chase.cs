// Assets/Scripts/Enemies/EnemyAI_Chase.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI_Chase : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float damage = 10f;
    public float attackCooldown = 1.0f;
    public float attackRange = 1.2f;

    Transform player;
    Rigidbody rb;
    float attackTimer;

    void Start()
    {
        var p = GameObject.FindWithTag("Player");
        player = p ? p.transform : null;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!player) return;
        attackTimer -= Time.deltaTime;

        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir.magnitude > attackRange)
        {
            Vector3 move = dir.normalized * moveSpeed * Time.deltaTime;
            rb.MovePosition(transform.position + move);
            transform.forward = Vector3.Lerp(transform.forward, dir.normalized, 8f * Time.deltaTime);
        }
        else
        {
            TryAttack();
        }
    }

    void TryAttack()
    {
        if (attackTimer <= 0f)
        {
            var hs = player.GetComponent<HealthSystem>();
            if (hs != null) hs.TakeDamage(damage);
            attackTimer = attackCooldown;
        }
    }
}
