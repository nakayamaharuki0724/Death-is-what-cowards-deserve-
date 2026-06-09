using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    public Transform player;

    public float sensitivity = 3f;
    public float distance = 5f;

    public Vector3 offset = new Vector3(0, 1.6f, 0);

    public float minY = -30f;
    public float maxY = 60f;

    [Header("Controller")]
    [SerializeField] private string rightStickHorizontalAxis = "CameraHorizontal";
    [SerializeField] private string rightStickVerticalAxis = "CameraVertical";
    [SerializeField] private float stickSensitivity = 120f;
    [SerializeField] private float stickDeadZone = 0.2f;

    float rotX = 0f;
    float rotY = 0f;

    public Vector3 PlanarForward
    {
        get
        {
            Vector3 forward = Quaternion.Euler(0f, rotY, 0f) * Vector3.forward;
            forward.y = 0f;
            return forward.normalized;
        }
    }

    public Vector3 PlanarRight
    {
        get
        {
            Vector3 right = Quaternion.Euler(0f, rotY, 0f) * Vector3.right;
            right.y = 0f;
            return right.normalized;
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        if (player == null)
            return;

        float h = Input.GetAxis("Mouse X") * sensitivity;
        float v = Input.GetAxis("Mouse Y") * sensitivity;

        float stickH = ReadAxis(rightStickHorizontalAxis);
        float stickV = ReadAxis(rightStickVerticalAxis);

        if (Mathf.Abs(stickH) > stickDeadZone)
            h += stickH * stickSensitivity * Time.deltaTime;

        if (Mathf.Abs(stickV) > stickDeadZone)
            v += stickV * stickSensitivity * Time.deltaTime;

        rotY += h;

        rotX -= v;
        rotX = Mathf.Clamp(rotX, minY, maxY);

        Quaternion rotation = Quaternion.Euler(rotX, rotY, 0f);

        Vector3 target = player.position + offset;
        Vector3 camPos = target + rotation * new Vector3(0, 0, -distance);

        transform.position = camPos;
        transform.LookAt(target);
    }

    private float ReadAxis(string axisName)
    {
        if (string.IsNullOrEmpty(axisName))
            return 0f;

        try
        {
            return Input.GetAxis(axisName);
        }
        catch (UnityException)
        {
            return 0f;
        }
    }
}