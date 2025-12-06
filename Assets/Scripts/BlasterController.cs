using UnityEngine;
using System.Collections;

public class BlasterController : MonoBehaviour
{
    [Header("References")]
    public Transform muzzle;
    public GameObject projectilePrefab;
    public ParticleSystem muzzleFlash;
    public AudioSource audioSource;
    public AudioClip fireSound;
    public AudioClip reloadSound;

    [Header("Aiming")]
    public LayerMask aimLayerMask;
    public float defaultAimDistance = 50f;

    [Header("Stats (upgradeable)")]
    public float projectileSpeed = 40f;
    public float fireRate = 0.25f;         // seconds between shots
    public int maxAmmo = 30;
    public float reloadTime = 1.5f;

    // Upgradeable fields (used by UpgradeManager)
    public int projectileDamage = 10;
    public int multiShotCount = 1;         // 1 = single, 2+ = burst/parallel
    public float multiShotDelay = 0.05f;

    int currentAmmo;
    bool isReloading;
    float fireTimer;
    Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        currentAmmo = maxAmmo;
        fireTimer = 0f;

        // Safe UI update
        UIManager.Instance?.UpdateBlasterStatus(currentAmmo, maxAmmo);
    }

    void Update()
    {
        fireTimer -= Time.deltaTime;

        // handle reload key
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
            return;
        }

        // auto reload if empty
        if (currentAmmo <= 0 && !isReloading)
        {
            StartCoroutine(Reload());
            return;
        }

        // firing (hold to auto-fire)
        if (Input.GetMouseButton(0) && fireTimer <= 0f && !isReloading && currentAmmo > 0)
        {
            Vector3 aimPoint = GetAimPoint();
            FireAt(aimPoint);
            fireTimer = fireRate;
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;

        if (audioSource && reloadSound)
            audioSource.PlayOneShot(reloadSound);

        // Wait (honors Time.timeScale)
        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        UIManager.Instance?.UpdateBlasterStatus(currentAmmo, maxAmmo);
        isReloading = false;
    }

    void FireAt(Vector3 point)
    {
        if (projectilePrefab == null || muzzle == null)
        {
            Debug.LogWarning("[BlasterController] missing projectilePrefab or muzzle.");
            return;
        }

        int count = Mathf.Max(1, multiShotCount);

        // consume 1 ammo per trigger pull (as requested)
        currentAmmo = Mathf.Max(0, currentAmmo - 1);
        UIManager.Instance?.UpdateBlasterStatus(currentAmmo, maxAmmo);

        // play sound and muzzle flash once per trigger pull
        if (audioSource && fireSound) audioSource.PlayOneShot(fireSound);
        if (muzzleFlash)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlash.Play(true);
        }

        // compute base direction
        Vector3 baseDir = (point - muzzle.position).normalized;
        if (baseDir.sqrMagnitude <= 0f) baseDir = muzzle.forward;

        // start coroutine that spawns bolts with tiny delay between them
        StartCoroutine(SpawnBoltSequence(baseDir, count));
    }

    IEnumerator SpawnBoltSequence(Vector3 baseDir, int count)
    {
        float spreadAngleDeg = 6f; // tweak as needed (kept for compatibility, though shots are parallel in spawn)
        float forwardSpawnOffset = 0.25f; // move spawn point forward to avoid inside-collider spawns

        // normalize baseDir
        baseDir = baseDir.normalized;
        if (baseDir.sqrMagnitude <= 0f) baseDir = muzzle.forward;

        // compute a right vector relative to the (flattened) aim direction for lateral offsets
        Vector3 flatDir = baseDir;
        flatDir.y = 0f;
        if (flatDir.sqrMagnitude < 0.001f) flatDir = transform.forward;
        Vector3 right = Vector3.Cross(Vector3.up, flatDir).normalized;
        if (right.sqrMagnitude < 0.001f) right = transform.right;

        float half = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            // lateral offset so bolts appear side-by-side but keep the same forward direction
            float lateralSpacing = 0.35f; // you can expose as a field if you want to tweak in inspector
            float lateralOffset = (i - half) * lateralSpacing;
            Vector3 spawnPos = muzzle.position + baseDir * forwardSpawnOffset + right * lateralOffset;

            Vector3 shotDir = baseDir; // SAME direction for all bolts

            Quaternion rot = Quaternion.LookRotation(shotDir);
            GameObject bolt = Instantiate(projectilePrefab, spawnPos, rot);
            if (bolt != null)
            {
                if (bolt.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.linearVelocity = shotDir * projectileSpeed;
                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                }

                var boltDamageComp = bolt.GetComponent<PhotonBolt>();
                if (boltDamageComp != null)
                    boltDamageComp.damage = projectileDamage;

                // OPTIONAL: if you have a player collider and want to explicitly ignore collisions:
                var playerObj = FindObjectOfType<PlayerMovement>()?.gameObject;
                if (playerObj != null)
                {
                    Collider playerCol = playerObj.GetComponent<Collider>();
                    Collider boltCol = bolt.GetComponent<Collider>();
                    if (playerCol != null && boltCol != null)
                    {
                        Physics.IgnoreCollision(boltCol, playerCol, true);
                    }
                }
            }

            // spacing between bolts so they don't immediately collide with each other or the player
            if (i < count - 1)
                yield return new WaitForSeconds(multiShotDelay);
        }

        yield break;
    }

    Vector3 GetAimPoint()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return transform.position + transform.forward * defaultAimDistance;

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, defaultAimDistance, aimLayerMask))
            return hit.point;

        // fallback: intersect a horizontal plane at the player's height
        Plane plane = new Plane(Vector3.up, transform.position);
        if (plane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return ray.origin + ray.direction * defaultAimDistance;
    }

    // Optional helper if you want to reset stats when restarting
    public void ResetToDefault()
    {
        fireRate = 0.25f;
        projectileSpeed = 40f;
        maxAmmo = 30;
        reloadTime = 1.5f;
        projectileDamage = 10;
        multiShotCount = 1;
        currentAmmo = maxAmmo;
        isReloading = false;
        fireTimer = 0f;
        UIManager.Instance?.UpdateBlasterStatus(currentAmmo, maxAmmo);
    }
}
