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

    [Header("Stats")]
    public float projectileSpeed = 40f;
    public float fireRate = 0.25f;
    public int maxAmmo = 30;
    public float reloadTime = 1.5f;

    int currentAmmo;
    bool isReloading;
    float fireTimer;
    Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        currentAmmo = maxAmmo;
        UIManager.Instance.UpdateBlasterStatus(currentAmmo, maxAmmo);
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

        // firing
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

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        UIManager.Instance.UpdateBlasterStatus(currentAmmo, maxAmmo);
        isReloading = false;
    }

    void FireAt(Vector3 point)
    {
        currentAmmo--;
        UIManager.Instance.UpdateBlasterStatus(currentAmmo, maxAmmo);

        if (audioSource && fireSound)
            audioSource.PlayOneShot(fireSound);
        if (muzzleFlash)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlash.Play();
        }

        // TEST CODE
        ScoreManager.Instance.AddPoints(100);
        // TEST CODE
        Vector3 dir = (point - muzzle.position).normalized;
        Quaternion rot = Quaternion.LookRotation(dir);
        GameObject bolt = Instantiate(projectilePrefab, muzzle.position, rot);
        if (bolt.TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = dir * projectileSpeed;
    }

    Vector3 GetAimPoint()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, aimLayerMask))
            return hit.point;

        Plane plane = new Plane(Vector3.up, transform.position);
        if (plane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return ray.origin + ray.direction * defaultAimDistance;
    }
}
