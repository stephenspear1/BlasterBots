using UnityEngine;

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

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;

    }

    void LateUpdate()
    {
        if (!target) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch = Mathf.Clamp(pitch - mouseY, minPitch, maxPitch);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPos = target.position + rot * offset;

        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, smoothSpeed * Time.deltaTime);

        if (cameraTransform)
            cameraTransform.LookAt(target.position + Vector3.up * 1.6f);
    }
}
