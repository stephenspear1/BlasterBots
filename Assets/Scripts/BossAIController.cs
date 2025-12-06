using UnityEngine;
using System.Collections;

/// <summary>
/// Simplified Boss AI: slow chase + single melee attack.
/// - No NavMeshAgent dependency.
/// - Single "melee" attack with windup and cooldown.
/// - Deals damage to player via HealthSystem.TakeDamage(float).
/// - Visuals (meleeVFX) are optional.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BossAIController : MonoBehaviour
{
    [Header("References")]
    public Transform player;                 // optional: auto-find by Player tag in Start()
    public LayerMask playerLayer;            // layer for detecting player in melee

    [Header("Movement")]
    public float moveSpeed = 1.5f;           // slow movement
    public float stoppingDistance = 2.0f;    // distance at which boss stops to melee

    [Header("Melee (single attack)")]
    public float meleeRange = 2.5f;          // trigger melee at this distance
    public float meleeDamage = 20f;          // damage to apply
    public float meleeCooldown = 3.0f;       // time between melee attempts
    public float meleeWindup = 0.35f;        // telegraph before damage applies
    public GameObject meleeVFX;              // optional VFX prefab spawned at boss position

    // internals
    float meleeTimer = 0f;

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        meleeTimer = 0f;
    }

    void Update()
    {
        if (player == null) return;

        // distance to player (planar)
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;

        // face player smoothly
        if (toPlayer.sqrMagnitude > 0.001f)
        {
            Vector3 forward = Vector3.Slerp(transform.forward, toPlayer.normalized, 6f * Time.deltaTime);
            transform.forward = forward;
        }

        // move toward player if outside stopping distance
        if (dist > stoppingDistance + 0.1f)
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }

        // melee logic
        meleeTimer -= Time.deltaTime;
        if (dist <= meleeRange && meleeTimer <= 0f)
        {
            StartCoroutine(DoMelee());
            meleeTimer = meleeCooldown;
        }
    }

    IEnumerator DoMelee()
    {
        // windup / telegraph
        // (play telegraph animation or VFX here if you want)
        if (meleeVFX != null)
            Instantiate(meleeVFX, transform.position + Vector3.down * 0.5f, Quaternion.identity);

        yield return new WaitForSeconds(meleeWindup);

        // damage players inside meleeRange
        Collider[] hits = Physics.OverlapSphere(transform.position, meleeRange, playerLayer);
        foreach (var c in hits)
        {
            var hs = c.GetComponent<HealthSystem>();
            if (hs != null)
            {
                hs.TakeDamage(meleeDamage);
            }
            else
            {
                // fallback: damage EnemyHealth if player uses that (unlikely)
                var eh = c.GetComponent<EnemyHealth>();
                if (eh != null)
                {
                    eh.TakeDamage((int)meleeDamage);
                }
            }
        }

        // small recovery (optional)
        yield return null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}
