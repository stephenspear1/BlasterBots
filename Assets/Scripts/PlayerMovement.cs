using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 12f;
    public LayerMask aimLayerMask; // Layer(s) to aim at (e.g., ground)
    public float aimRayMaxDistance = 200f;

    private Rigidbody rb;
    private Camera mainCam;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCam = Camera.main;
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        MovePlayer();
        RotateToMouse();
    }

    void MovePlayer()
    {
        // Get input from legacy axis
        float h = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        // If no movement input, keep vertical velocity only (gravity)
        if (Mathf.Approximately(h, 0f) && Mathf.Approximately(v, 0f))
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        // Get camera forward/right (ignore vertical component)
        Vector3 camForward = mainCam.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = mainCam.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        // Determine move direction
        Vector3 moveDir = (camForward * v + camRight * h).normalized;

        // Set Rigidbody velocity
        Vector3 velocity = moveDir * moveSpeed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }

    void RotateToMouse()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        // Cast a ray to find aim point
        if (Physics.Raycast(ray, out RaycastHit hit, aimRayMaxDistance, aimLayerMask))
        {
            Vector3 lookDir = hit.point - transform.position;
            lookDir.y = 0f; // keep rotation flat on ground
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }
        else
        {
            // fallback if mouse is not hitting ground: look forward camera direction
            Vector3 fallbackDir = mainCam.transform.forward;
            fallbackDir.y = 0f;
            if (fallbackDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(fallbackDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }
    }
}
