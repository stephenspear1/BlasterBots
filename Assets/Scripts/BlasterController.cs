using UnityEngine;

public class BlasterController : MonoBehaviour
{
    [Header("References")]
    public Transform muzzle;
    public GameObject projectilePrefab;
    public float projectileSpeed = 40f;
    public float fireRate = 0.25f;

    [Header("Aiming")]
    public LayerMask aimLayerMask;
    public float defaultAimDistance = 50f;

    float fireTimer;
    Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        fireTimer -= Time.deltaTime;
        if (Input.GetMouseButton(0) && fireTimer <= 0f)
        {
            Vector3 aimPoint = GetAimPoint();
            FireAt(aimPoint);
            fireTimer = fireRate;
        }
    }

    Vector3 GetAimPoint()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, aimLayerMask))
            return hit.point;

        // Fallback: plane at player height
        Plane plane = new Plane(Vector3.up, transform.position);
        return plane.Raycast(ray, out float enter)
            ? ray.GetPoint(enter)
            : ray.origin + ray.direction * defaultAimDistance;
    }

    void FireAt(Vector3 point)
    {
        Vector3 dir = (point - muzzle.position).normalized;
        Quaternion rot = Quaternion.LookRotation(dir);
        GameObject bolt = Instantiate(projectilePrefab, muzzle.position, rot);
        if (bolt.TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = dir * projectileSpeed;
    }
}
