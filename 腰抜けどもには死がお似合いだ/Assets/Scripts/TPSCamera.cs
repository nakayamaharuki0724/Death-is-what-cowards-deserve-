using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    public Transform player;

    public float sensitivity = 3f;
    public float distance = 5f;

    public Vector3 offset = new Vector3(0, 1.0f, 0);

    public float minY = -30f;
    public float maxY = 60f;

    float rotX = 0f;
    float rotY = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        // ƒ}ƒEƒX“ü—Í
        float h = Input.GetAxis("Mouse X");
        float v = Input.GetAxis("Mouse Y");

        // ¶‰E‰ñ“]
        rotY += h * sensitivity;

        // ã‰º‰ñ“]
        rotX -= v * sensitivity;
        rotX = Mathf.Clamp(rotX, minY, maxY);

        // ‰ñ“]ì¬
        Quaternion rotation = Quaternion.Euler(rotX, rotY, 0f);

        // Player‚ğ‚İ‚é
        Vector3 target = player.position + new Vector3(0, 1.6f, 0);
        Vector3 camPos = target + rotation * new Vector3(0, 0, -distance);

        transform.position = camPos;
        transform.LookAt(target);
    }
}