using UnityEngine;
using System.Reflection;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;
    public Transform cameraTransform;
    public Vector3 offset = new Vector3(0, 2.5f, -6f);
    public float mouseSensitivity = 3f;
    public float smoothSpeed = 10f;
    public float minPitch = -20f;
    public float maxPitch = 60f;

    float yaw, pitch;

    UpgradeManager upgradeMgr;

    // internal cached cursor state so we don't reassign every frame
    bool cursorLocked = true;
    bool cursorVisible = false;

    void Start()
    {
        upgradeMgr = UpgradeManager.Instance;

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        // Initial gameplay cursor state
        SetCursorLocked(true);
    }

    void LateUpdate()
    {
        if (!target) return;

        // If the game is paused (timescale == 0) then release cursor & stop camera.
        if (Mathf.Approximately(Time.timeScale, 0f))
        {
            SetCursorLocked(false);
            return;
        }

        // If upgrade menu is open, STOP camera rotation AND release cursor.
        if (IsUpgradeOpen())
        {
            SetCursorLocked(false);
            return;
        }

        // Gameplay mode → lock/hide cursor if not manually unlocked
        SetCursorLocked(true);

        // Mouse look input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch = Mathf.Clamp(pitch - mouseY, minPitch, maxPitch);

        // Rotate camera rig
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPos = target.position + rot * offset;

        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, smoothSpeed * Time.deltaTime);

        // Aim camera at player's head
        if (cameraTransform)
            cameraTransform.LookAt(target.position + Vector3.up * 1.6f);
    }

    // safely determine if the UpgradeManager reports open (uses reflection to avoid compile errors)
    bool IsUpgradeOpen()
    {
        if (upgradeMgr == null) return false;

        // try property "IsOpen" first (public)
        var prop = upgradeMgr.GetType().GetProperty("IsOpen", BindingFlags.Public | BindingFlags.Instance);
        if (prop != null && prop.PropertyType == typeof(bool))
        {
            try { return (bool)prop.GetValue(upgradeMgr); }
            catch { /* ignore reflection errors */ }
        }

        // try field "IsOpen" or "isOpen" (public or non-public)
        var field = upgradeMgr.GetType().GetField("IsOpen", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                 ?? upgradeMgr.GetType().GetField("isOpen", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null && field.FieldType == typeof(bool))
        {
            try { return (bool)field.GetValue(upgradeMgr); }
            catch { /* ignore reflection errors */ }
        }

        // fallback: no info available -> assume closed
        return false;
    }

    // helper: only set cursor state when it actually changes
    void SetCursorLocked(bool locked)
    {
        if (locked)
        {
            if (!cursorLocked || cursorVisible)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                cursorLocked = true;
                cursorVisible = false;
            }
        }
        else
        {
            if (cursorLocked || !cursorVisible)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                cursorLocked = false;
                cursorVisible = true;
            }
        }
    }
}
