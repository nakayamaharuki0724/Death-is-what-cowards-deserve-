using System.Collections;
using UnityEngine;

public class Boss : MonoBehaviour
{
    public Transform player;
    public ParticleSystem flame;
    public GameObject biteHitBox;
    public GameObject clawHitBox;
    public GameObject fireHitBox;

    public float speed = 5f;
    public float stopDistance = 3f;
    public float rotationSpeed = 5f;

    Animator animator;

    bool canMove = false;
    bool hasStarted = false;
    bool isAttacking = false;

    float attackCooldown = 2f;
    float attackTimer = 0f;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        flame.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        biteHitBox.SetActive(false);
        clawHitBox.SetActive(false);
        fireHitBox.SetActive(false);

        StartCoroutine(StartBattle());
    }

    IEnumerator StartBattle()
    {
        SetAction("Scream");
        yield return new WaitForSeconds(2f);
        hasStarted = true;
        SetAction("Run");
    }

    void Update()
    {
        if (!hasStarted) return;

        attackTimer -= Time.deltaTime;

        UpdateMoveState();

        if (!isAttacking)
            TryAttack();

        if (canMove)
            Move();

        if (!isAttacking)
            RotateTowardsPlayer();
    }

    void UpdateMoveState()
    {
        var state = animator.GetCurrentAnimatorStateInfo(0);

        if (state.IsName("Run") || state.IsName("Fly Forward"))
            canMove = true;
        else
            canMove = false;
    }

    void TryAttack()
    {
        if (attackTimer > 0f || isAttacking) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= 9f)
        {
            StartCoroutine(AttackRoutine("Basic Attack", 1f));
        }
        else if (dist <= 15f)
        {
            StartCoroutine(AttackRoutine("Claw Attack", 3f));
        }
        else if (dist <= 19f)
        {
            StartCoroutine(AttackRoutine("Flame Attack", 3f));
        }
    }

    void Move()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > stopDistance)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
        }
    }

    void RotateTowardsPlayer()
    {
        Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        Vector3 dir = (lookPos - transform.position).normalized;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    void SetAction(string action)
    {
        ResetAllBools();
        animator.SetBool(action, true);
    }

    void ResetAllBools()
    {
        animator.SetBool("Idle01", false);
        animator.SetBool("Scream", false);
        animator.SetBool("Run", false);
        animator.SetBool("Fly Forward", false);
        animator.SetBool("Take Off", false);
        animator.SetBool("Land", false);
        animator.SetBool("Basic Attack", false);
        animator.SetBool("Claw Attack", false);
        animator.SetBool("Flame Attack", false);
    }

    IEnumerator AttackRoutine(string action, float duration)
    {
        isAttacking = true;

        SetAction(action);

        if (action == "Basic Attack")
        {
            biteHitBox.SetActive(true);
        }
        else if (action == "Claw Attack")
        {
            clawHitBox.SetActive(true);
        }
        else if (action == "Flame Attack")
        {
            fireHitBox.SetActive(true);
            flame.Play();
        }

        yield return new WaitForSeconds(duration);

        biteHitBox.SetActive(false);
        clawHitBox.SetActive(false);

        if (action == "Flame Attack")
        {
            fireHitBox.SetActive(false);
            flame.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        SetAction("Idle01");
        yield return new WaitForSeconds(1f);

        isAttacking = false;
        attackTimer = attackCooldown;

        SetAction("Run");
    }
}