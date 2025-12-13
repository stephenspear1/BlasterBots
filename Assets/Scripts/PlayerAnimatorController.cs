using UnityEngine;

[RequireComponent(typeof(PlayerMovement))] // there should be some movement script on player
public class PlayerAnimationController : MonoBehaviour
{
    Animator animator;
    readonly int moveXHash = Animator.StringToHash("moveX");
    readonly int moveZHash = Animator.StringToHash("moveZ");

    void Awake()
    {
        // find animator on this object or any child model
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
            Debug.LogError("[PlayerAnimationController] Animator not found in children. Assign an Animator to the model child.");
    }

    void Update()
    {
        if (animator == null) return;

        // legacy input axes (WASD or arrows)
        float moveZ = Input.GetAxisRaw("Vertical");   // W=+1, S=-1
        float moveX = Input.GetAxisRaw("Horizontal"); // D=+1, A=-1

        // push values to animator with tiny smoothing to avoid jitter
        animator.SetFloat(moveXHash, moveX, 0.05f, Time.deltaTime);
        animator.SetFloat(moveZHash, moveZ, 0.05f, Time.deltaTime);
    }
}
