using UnityEngine;
using System.Collections;

/// <summary>
/// Simplified Boss AI: slow chase + single melee attack.
/// Minimal animation integration: calls animator states "WalkForward", "Attack", and "Idle".
/// Prevents Update from overriding attack animation by using an isAttacking flag.
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

    // internals
    float meleeTimer = 0f;

    // --- animation helpers (minimal additions) ---
    Animator animator;
    string currentState = "";
    readonly string STATE_IDLE = "Idle";
    readonly string STATE_WALK = "WalkForward";
    readonly string STATE_ATTACK = "Attack";

    // prevents Update from stomping the attack animation
    bool isAttacking = false;

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        meleeTimer = 0f;

        // animator: find the Animator on a child model (do not put Animator on root)
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
            Debug.LogWarning("[BossAIController] Animator not found in children. States will be skipped.");
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

        // If currently performing attack, skip movement->state overrides
        if (!isAttacking)
        {
            // move toward player if outside stopping distance
            if (dist > stoppingDistance + 0.1f)
            {
                transform.position += transform.forward * moveSpeed * Time.deltaTime;
                RequestState(STATE_WALK); // play walk when moving
            }
            else
            {
                RequestState(STATE_IDLE); // stopped
            }
        }

        // melee logic (can still start while not attacking)
        meleeTimer -= Time.deltaTime;
        if (dist <= meleeRange && meleeTimer <= 0f && !isAttacking)
        {
            StartCoroutine(DoMelee());
            meleeTimer = meleeCooldown;
        }
    }

    IEnumerator DoMelee()
    {
        // set attacking flag so Update doesn't override animation
        isAttacking = true;

        // switch to attack animation before windup
        RequestState(STATE_ATTACK);


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

        // small recovery (optional) — wait a tiny bit so the attack animation can finish a frame
        yield return new WaitForSeconds(0.1f);

        // clear attacking state so Update resumes normal behavior
        isAttacking = false;

        yield break;
    }

    // minimal helper: only call Play when state actually changes
    void RequestState(string nextState)
    {
        if (animator == null) return;
        if (nextState == currentState) return;

        int layer = 0;
        int hash = Animator.StringToHash(nextState);
        if (animator.HasState(layer, hash))
        {
            animator.Play(nextState);
            currentState = nextState;
        }
        else
        {
            // if requested state missing, try idle fallback (prevents errors)
            int idleHash = Animator.StringToHash(STATE_IDLE);
            if (animator.HasState(layer, idleHash))
            {
                animator.Play(STATE_IDLE);
                currentState = STATE_IDLE;
            }
            else
            {
                // nothing to do
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}
