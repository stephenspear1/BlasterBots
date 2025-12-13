using UnityEngine;

public class AnimDebug : MonoBehaviour
{
    Animator animator;
    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        Debug.Log($"[AnimDebug] Awake. Animator found? { (animator!=null) } on { (animator!=null ? animator.gameObject.name : "null") }");
        if (animator != null)
        {
            var names = "";
            foreach (var p in animator.parameters) names += p.name + " ";
            Debug.Log("[AnimDebug] Animator parameters: " + names);
        }
    }

    void Update()
    {
        float moveZ = Input.GetAxisRaw("Vertical");
        float moveX = Input.GetAxisRaw("Horizontal");
        bool anyKey = Input.anyKey;

        Debug.Log($"[AnimDebug] Input: moveX={moveX}, moveZ={moveZ}, anyKey={anyKey}");

        if (animator != null)
        {
            animator.SetFloat("moveX", moveX);
            animator.SetFloat("moveZ", moveZ);
            Debug.Log($"[AnimDebug] SetFloat called. Current params from Animator: moveX={animator.GetFloat("moveX")}, moveZ={animator.GetFloat("moveZ")}");
        }
    }
}
