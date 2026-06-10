using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 2.2f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float runInputThreshold = 0.75f;
    [SerializeField] private float acceleration = 14f;
    [SerializeField] private float deceleration = 18f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private bool lockMovementWhileAction = true;
    [SerializeField] private float attackLockDuration = 0.6f;
    [SerializeField] private float parryLockDuration = 0.8f;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform cameraTransform;

    private OrbitCamera orbitCamera;

    private Vector3 moveInput;
    private float currentSpeed;
    private float actionLockTimer;

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (rb == null)
            rb = GetComponent<Rigidbody>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform != null)
            orbitCamera = cameraTransform.GetComponent<OrbitCamera>();

        if (orbitCamera == null)
            orbitCamera = FindObjectOfType<OrbitCamera>();

        if (rb != null)
            rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Update()
    {
        HandleInput();

        if (actionLockTimer > 0f)
            actionLockTimer -= Time.deltaTime;

        if (rb == null)
            HandleMovement(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (rb != null)
            HandleMovement(Time.fixedDeltaTime);
    }

    private void HandleInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        moveInput = new Vector3(horizontal, 0, vertical);
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        if (actionLockTimer > 0f)
            return;

        if (Input.GetButtonDown("Fire2"))
        {
            Attack();
        }
        else if (Input.GetButtonDown("Fire3"))
        {
            Parry();
        }
    }

    private void HandleMovement(float deltaTime)
    {
        bool isInAction = actionLockTimer > 0f;
        float inputMagnitude = moveInput.magnitude;

        Vector3 moveDirection = GetMoveDirectionFromInput();
        if (lockMovementWhileAction && isInAction)
        {
            moveDirection = Vector3.zero;
            inputMagnitude = 0f;
        }

        float targetSpeed = 0f;
        if (inputMagnitude > 0.0001f)
        {
            if (inputMagnitude < runInputThreshold)
            {
                float t = inputMagnitude / Mathf.Max(0.01f, runInputThreshold);
                targetSpeed = walkSpeed * t;
            }
            else
            {
                float t = (inputMagnitude - runInputThreshold) / Mathf.Max(0.01f, 1f - runInputThreshold);
                targetSpeed = Mathf.Lerp(walkSpeed, runSpeed, t);
            }
        }

        float speedChange = targetSpeed > currentSpeed ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedChange * deltaTime);

        Vector3 velocity = moveDirection * currentSpeed;

        if (rb != null)
            rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
        else
            transform.Translate(velocity * deltaTime, Space.World);

        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * deltaTime);
        }

        if (animator != null)
            animator.SetBool("IsWalking", currentSpeed > 0.05f);
    }

    private Vector3 GetMoveDirectionFromInput()
    {
        Vector3 camForward;
        Vector3 camRight;

        if (orbitCamera != null)
        {
            camForward = orbitCamera.PlanarForward;
            camRight = orbitCamera.PlanarRight;
        }
        else if (cameraTransform != null)
        {
            camForward = cameraTransform.forward;
            camRight = cameraTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            camForward.Normalize();
            camRight.Normalize();
        }
        else
        {
            return moveInput;
        }

        Vector3 direction = camForward * moveInput.z + camRight * moveInput.x;
        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        return direction;
    }

    private void Attack()
    {
        if (animator != null)
        {
            animator.ResetTrigger("Parry");
            animator.SetTrigger("Attack");
        }

        actionLockTimer = attackLockDuration;
    }

    private void Parry()
    {
        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Parry");
        }

        actionLockTimer = parryLockDuration;
    }
}
