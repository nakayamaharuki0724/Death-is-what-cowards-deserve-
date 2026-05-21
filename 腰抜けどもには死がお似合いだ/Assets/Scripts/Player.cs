using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rb;

    private Vector3 moveInput;
    private bool isAttacking = false;
    private bool isParrying = false;

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleInput();
        HandleMovement();
    }

    private void HandleInput()
    {
        // Left stick movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        moveInput = new Vector3(horizontal, 0, vertical).normalized;

        // Y button attack
        if (Input.GetButtonDown("Fire2")) // Y button
        {
            Attack();
        }

        // X button parry
        if (Input.GetButtonDown("Fire3")) // X button
        {
            Parry();
        }
    }

    private void HandleMovement()
    {
        if (moveInput.magnitude > 0)
        {
            // Move the player
            Vector3 moveDirection = moveInput * moveSpeed;
            if (rb != null)
            {
                rb.velocity = new Vector3(moveDirection.x, rb.velocity.y, moveDirection.z);
            }
            else
            {
                transform.Translate(moveDirection * Time.deltaTime);
            }

            // Rotate player to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Play walk animation
            if (animator != null)
                animator.SetBool("IsWalking", true);
        }
        else
        {
            // Stop movement
            if (rb != null)
                rb.velocity = new Vector3(0, rb.velocity.y, 0);

            // Play idle animation
            if (animator != null)
                animator.SetBool("IsWalking", false);
        }
    }

    private void Attack()
    {
        if (isAttacking || isParrying)
            return;

        isAttacking = true;
        if (animator != null)
            animator.SetTrigger("Attack");

        // Attack duration - adjust as needed
        Invoke(nameof(ResetAttack), 0.6f);
    }

    private void ResetAttack()
    {
        isAttacking = false;
    }

    private void Parry()
    {
        if (isAttacking || isParrying)
            return;

        isParrying = true;
        if (animator != null)
            animator.SetTrigger("Parry");

        // Parry duration - adjust as needed
        Invoke(nameof(ResetParry), 0.8f);
    }

    private void ResetParry()
    {
        isParrying = false;
    }
}
