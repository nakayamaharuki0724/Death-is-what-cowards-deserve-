using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damageSE;
    [SerializeField] private string gameOverSceneName = "GameOverScene";
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private string deadTriggerName = "Dead";
    [SerializeField] private string deadStateName = "Dead";
    [SerializeField] private float deadAnimationFallbackWait = 1.5f;
    [SerializeField] private string damageTriggerName = "Damage";
    [SerializeField] private string damageStateName = "Damage";
    [SerializeField] private float damageAnimationFallbackWait = 0.4f;
    [SerializeField] private float damageRecoverInvincibleTime = 0.35f;
    [SerializeField] private int damageAnimationThreshold = 15;
    [SerializeField] private GameObject attackHitBox;
    [SerializeField] private GameObject attack2HitBox;
    [SerializeField] private float attackHitActiveTime = 0.2f;
    [SerializeField] private float attack2HitActiveTime = 0.25f;

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
    private bool isDead;
    private bool isInvincible;
    private Coroutine damageReactionCoroutine;
    private bool isDodging;
    private bool isDamageReacting;

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

        if (attackHitBox != null)
            attackHitBox.SetActive(false);

        if (attack2HitBox != null)
            attack2HitBox.SetActive(false);

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
        if (isDead)
        {
            moveInput = Vector3.zero;
            return;
        }

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

        if (isDead)
        {
            isInAction = true;
            inputMagnitude = 0f;
        }

        if (isDamageReacting)
        {
            isInAction = true;
            inputMagnitude = 0f;
        }

        if (isDodging && !actionLocked)
        {
            isDodging = false;
            dodgeVelocity = Vector3.zero;
            dodgeMoveTimer = 0f;
        }

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

        if (isDodging)
        {
            velocity = dodgeVelocity;
        }
        else if (dodgeMoveTimer > 0f)
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
        {
            bool canWalkAnim = inputMagnitude > 0.05f && !isInAction && !isInvincible && !isDamageReacting;
            animator.SetBool("IsWalking", canWalkAnim);
        }
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
        StartCoroutine(ActivateHitBox(attackHitBox, attackHitActiveTime));
    }

    private void Atack2()
    {
        TriggerAction(TriggerAtack2);
        StartCoroutine(ActivateHitBox(attack2HitBox, attack2HitActiveTime));
    }

    private System.Collections.IEnumerator ActivateHitBox(GameObject hitBox, float activeTime)
    {
        if (hitBox == null)
            yield break;

        hitBox.SetActive(false);
        yield return null;

        hitBox.SetActive(true);
        yield return new WaitForSeconds(Mathf.Max(0.01f, activeTime));
        hitBox.SetActive(false);
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
        isDodging = true;
    }

    private void Heal()
    {
        TriggerAction(TriggerHeal);

        currentHP = Mathf.Min(currentHP + 50, maxHP);
        Debug.Log("HP : " + currentHP);
    }

    private void ClearActionTriggers()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(TriggerAtack);
        animator.ResetTrigger(TriggerAtack2);
        animator.ResetTrigger(TriggerDodge);
        animator.ResetTrigger(TriggerHeal);
        animator.ResetTrigger(damageTriggerName);
        animator.ResetTrigger(deadTriggerName);
    }

    private void ForcePlayState(string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
            return;

        int stateHash = Animator.StringToHash(stateName);
        if (animator.HasState(0, stateHash))
        {
            animator.Play(stateName, 0, 0f);
            return;
        }

        string baseLayerStateName = "Base Layer." + stateName;
        int baseLayerHash = Animator.StringToHash(baseLayerStateName);
        if (animator.HasState(0, baseLayerHash))
            animator.Play(baseLayerStateName, 0, 0f);
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
        if (isDead || isInvincible)
            return;

        currentHP -= damage;

        if (currentHP < 0)
            currentHP = 0;

        Debug.Log("HP : " + currentHP);

        if (audioSource != null && damageSE != null)
        {
            audioSource.PlayOneShot(damageSE);
        }

        if (currentHP <= 0)
        {
            StartCoroutine(HandleDeathSequence());
            return;
        }

        if (damage < damageAnimationThreshold)
            return;

        if (damageReactionCoroutine != null)
            StopCoroutine(damageReactionCoroutine);

        damageReactionCoroutine = StartCoroutine(HandleDamageReaction());
    }

    private System.Collections.IEnumerator HandleDamageReaction()
    {
        isInvincible = true;
        isDamageReacting = true;
        actionLocked = true;
        moveInput = Vector3.zero;
        currentSpeed = 0f;
        dodgeMoveTimer = 0f;
        dodgeVelocity = Vector3.zero;
        isDodging = false;

        if (actionLockCoroutine != null)
        {
            StopCoroutine(actionLockCoroutine);
            actionLockCoroutine = null;
        }

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            ClearActionTriggers();
            ForcePlayState(damageStateName);
            animator.SetTrigger(damageTriggerName);
        }

        yield return WaitForDamageAnimation();

        float recoverWait = Mathf.Max(0f, damageRecoverInvincibleTime);
        if (!isDead && recoverWait > 0f)
            yield return new WaitForSeconds(recoverWait);

        if (!isDead)
            actionLocked = false;

        isDamageReacting = false;
        isInvincible = false;
        damageReactionCoroutine = null;
    }

    private System.Collections.IEnumerator WaitForDamageAnimation()
    {
        if (animator == null)
        {
            yield return new WaitForSeconds(damageAnimationFallbackWait);
            yield break;
        }

        float enterTimeout = 1f;
        bool enteredDamageState = false;

        while (enterTimeout > 0f)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName(damageStateName))
            {
                enteredDamageState = true;
                break;
            }

            enterTimeout -= Time.deltaTime;
            yield return null;
        }

        if (!enteredDamageState)
        {
            yield return new WaitForSeconds(damageAnimationFallbackWait);
            yield break;
        }

        while (animator != null)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (!animator.IsInTransition(0) && state.IsName(damageStateName) && state.normalizedTime >= 1f)
                break;

            yield return null;
        }
    }

    private System.Collections.IEnumerator HandleDeathSequence()
    {
        isDead = true;
        isInvincible = true;
        isDamageReacting = false;
        actionLocked = true;
        moveInput = Vector3.zero;
        currentSpeed = 0f;
        dodgeMoveTimer = 0f;
        dodgeVelocity = Vector3.zero;
        isDodging = false;

        if (damageReactionCoroutine != null)
        {
            StopCoroutine(damageReactionCoroutine);
            damageReactionCoroutine = null;
        }

        if (attackHitBox != null)
            attackHitBox.SetActive(false);

        if (attack2HitBox != null)
            attack2HitBox.SetActive(false);

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            ClearActionTriggers();
            animator.SetTrigger(deadTriggerName);
            ForcePlayState(deadStateName);
        }

        if (rb != null)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        yield return WaitForDeadAnimation();
        yield return FadeToBlack();

        SceneManager.LoadScene(gameOverSceneName);
    }

    private System.Collections.IEnumerator WaitForDeadAnimation()
    {
        if (animator == null)
        {
            yield return new WaitForSeconds(deadAnimationFallbackWait);
            yield break;
        }

        float enterTimeout = 1f;
        bool enteredDeadState = false;

        while (enterTimeout > 0f)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName(deadStateName))
            {
                enteredDeadState = true;
                break;
            }

            enterTimeout -= Time.deltaTime;
            yield return null;
        }

        if (!enteredDeadState)
        {
            yield return new WaitForSeconds(deadAnimationFallbackWait);
            yield break;
        }

        while (animator != null)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (!animator.IsInTransition(0) && state.IsName(deadStateName) && state.normalizedTime >= 1f)
                break;

            yield return null;
        }
    }

    private System.Collections.IEnumerator FadeToBlack()
    {
        Canvas fadeCanvas = new GameObject("FadeCanvas").AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = short.MaxValue;

        CanvasScaler scaler = fadeCanvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        fadeCanvas.gameObject.AddComponent<GraphicRaycaster>();

        GameObject imageObject = new GameObject("FadeImage");
        imageObject.transform.SetParent(fadeCanvas.transform, false);

        RectTransform rect = imageObject.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image fadeImage = imageObject.AddComponent<Image>();
        Color color = Color.black;
        color.a = 0f;
        fadeImage.color = color;

        float timer = 0f;
        float duration = Mathf.Max(0.01f, fadeDuration);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Clamp01(timer / duration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
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
