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
    [SerializeField] private bool blockUpwardDrift = true;
    [SerializeField] private float dodgeForwardDistance = 2.2f;
    [SerializeField] private float dodgeForwardDuration = 0.22f;
    [SerializeField] private float dodgeMoveSpeedCarry = 1f;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private int maxHP = 100;

    private const string TriggerAtack = "Atack";
    private const string TriggerAtack2 = "Atack2";
    private const string TriggerDodge = "Dodge";
    private const string TriggerHeal = "Heal";

    private OrbitCamera orbitCamera;

    private Vector3 moveInput;
    private float currentSpeed;
    private Vector3 dodgeVelocity;
    private float dodgeMoveTimer;
    private bool actionLocked;
    private int lockedActionHash;
    private Coroutine actionLockCoroutine;
    private int currentHP;

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

        if (animator != null)
            animator.applyRootMotion = false;

        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        currentHP = maxHP;
        Debug.Log("Start HP = " + currentHP);
    }

    private void Update()
    {
        HandleInput();

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
        if (actionLocked)
        {
            moveInput = Vector3.zero;
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(horizontal, 0, vertical);
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        bool atackPressed = Input.GetButtonDown("Jump");
        bool atack2Pressed = Input.GetButtonDown("Fire2");
        bool dodgePressed = Input.GetButtonDown("Fire1");
        bool healPressed = Input.GetButtonDown("Fire3");

        // Y button
        if (atackPressed)
        {
            Atack();
        }
        // B button
        else if (atack2Pressed)
        {
            Atack2();
        }
        // A button
        else if (dodgePressed)
        {
            Dodge();
        }
        // X button
        else if (healPressed)
        {
            Heal();
        }
    }

    private void HandleMovement(float deltaTime)
    {
        bool isInAction = actionLocked;
        float inputMagnitude = moveInput.magnitude;

        Vector3 moveDirection = GetMoveDirectionFromInput();
        if (isInAction)
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

            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * deltaTime);
        }
        else
        {
            currentSpeed = 0f;
        }

        Vector3 velocity = moveDirection * currentSpeed;

        if (dodgeMoveTimer > 0f)
        {
            velocity = dodgeVelocity;
            dodgeMoveTimer -= deltaTime;
            if (dodgeMoveTimer <= 0f)
            {
                dodgeMoveTimer = 0f;
                dodgeVelocity = Vector3.zero;
            }
        }

        if (rb != null)
        {
            float yVelocity = rb.linearVelocity.y;
            if (blockUpwardDrift && yVelocity > 0f)
                yVelocity = 0f;

            rb.linearVelocity = new Vector3(velocity.x, yVelocity, velocity.z);
        }
        else
        {
            transform.Translate(velocity * deltaTime, Space.World);
        }

        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            if (rb != null)
            {
                Quaternion smoothRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * deltaTime);
                rb.MoveRotation(smoothRotation);
            }
            else
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * deltaTime);
            }
        }

        if (animator != null)
            animator.SetBool("IsWalking", inputMagnitude > 0.05f && !isInAction);
    }

    private void TriggerAction(string triggerName)
    {
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            ClearActionTriggers();

            int previousStateHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;

            animator.SetTrigger(triggerName);
            lockedActionHash = Animator.StringToHash(triggerName);
            actionLocked = true;

            if (actionLockCoroutine != null)
                StopCoroutine(actionLockCoroutine);
            actionLockCoroutine = StartCoroutine(UnlockWhenTriggeredStateFinishes(previousStateHash));
        }
    }

    private System.Collections.IEnumerator UnlockWhenTriggeredStateFinishes(int previousStateHash)
    {
        float timeout = 0.5f;
        while (timeout > 0f && animator != null && animator.GetCurrentAnimatorStateInfo(0).fullPathHash == previousStateHash)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (animator == null)
        {
            actionLocked = false;
            actionLockCoroutine = null;
            yield break;
        }

        int actionStateHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;

        while (animator != null)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

            if (!animator.IsInTransition(0) && state.fullPathHash == actionStateHash && state.normalizedTime >= 1f)
                break;

            if (!animator.IsInTransition(0) && state.fullPathHash != actionStateHash)
                break;

            yield return null;
        }

        actionLocked = false;
        ClearActionTriggers();
        actionLockCoroutine = null;
    }

    private void Atack()
    {
        TriggerAction(TriggerAtack);
    }

    private void Atack2()
    {
        TriggerAction(TriggerAtack2);
    }

    private void Dodge()
    {
        TriggerAction(TriggerDodge);

        float duration = Mathf.Max(0.01f, dodgeForwardDuration);
        Vector3 dodgeDirection = GetMoveDirectionFromInput();
        if (dodgeDirection.sqrMagnitude < 0.0001f)
            dodgeDirection = transform.forward;
        else
            dodgeDirection.Normalize();

        float baseDodgeSpeed = dodgeForwardDistance / duration;
        float carrySpeed = currentSpeed * dodgeMoveSpeedCarry;

        dodgeVelocity = dodgeDirection * (baseDodgeSpeed + carrySpeed);
        dodgeVelocity.y = 0f;
        dodgeMoveTimer = duration;
    }

    private void Heal()
    {
        TriggerAction(TriggerHeal);
    }

    private void ClearActionTriggers()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(TriggerAtack);
        animator.ResetTrigger(TriggerAtack2);
        animator.ResetTrigger(TriggerDodge);
        animator.ResetTrigger(TriggerHeal);
    }

    private bool IsActionAnimationPlaying()
    {
        if (animator == null)
            return false;

        AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(0);
        if (IsActionState(current))
            return true;

        if (animator.IsInTransition(0))
        {
            AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(0);
            if (IsActionState(next))
                return true;
        }

        return false;
    }

    private bool IsActionState(AnimatorStateInfo state)
    {
        return state.IsName(TriggerAtack)
            || state.IsName(TriggerAtack2)
            || state.IsName(TriggerDodge)
            || state.IsName(TriggerHeal);
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

    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        if (currentHP < 0)
            currentHP = 0;

        Debug.Log("HP : " + currentHP);

        if (currentHP <= 0)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
        }
    }

    public int GetCurrentHP()
    {
        return currentHP;
    }

    public int GetMaxHP()
    {
        return maxHP;
    }
}
