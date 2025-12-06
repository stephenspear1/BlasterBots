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

        // optional safety
        if (rb == null) Debug.LogError("[EnemyAI_Chase] missing Rigidbody");
    }

    void Update()
    {
        // decrement attack timer on real time (frame) basis
        attackTimer -= Time.deltaTime;

        // attack check handled in Update (non-physics)
        if (player == null) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        float sqrDist = dir.sqrMagnitude;
        float attackRangeSqr = attackRange * attackRange;

        if (sqrDist <= attackRangeSqr)
        {
            TryAttack();
        }
        // movement happens in FixedUpdate for physics consistency
    }

    void FixedUpdate()
    {
        if (player == null || rb == null) return;

        Vector3 dir = player.position - rb.position;
        dir.y = 0f;

        float sqrDist = dir.sqrMagnitude;
        float attackRangeSqr = attackRange * attackRange;

        if (sqrDist > attackRangeSqr)
        {
            Vector3 moveDir = dir.normalized;
            Vector3 next = rb.position + moveDir * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(next);

            // rotate smoothly using physics-friendly MoveRotation
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            Quaternion newRot = Quaternion.Slerp(rb.rotation, targetRot, 8f * Time.fixedDeltaTime);
            rb.MoveRotation(newRot);
        }
        else
        {
            // optional: ensure velocity zero when in attack range
            rb.linearVelocity = Vector3.zero;
        }
    }

    void TryAttack()
    {
        if (attackTimer <= 0f && player != null)
        {
            var hs = player.GetComponent<HealthSystem>();
            if (hs != null) hs.TakeDamage(damage);
            attackTimer = attackCooldown;
        }
    }
}
