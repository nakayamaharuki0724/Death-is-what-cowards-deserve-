using System.Collections;
using UnityEngine;

public class Boss : MonoBehaviour
{
    public Transform player;
    public ParticleSystem flame;
    public GameObject biteHitBox;
    public GameObject clawHitBox;
    public GameObject fireHitBox;

    public float BossHP = 1000.0f;

    public float speed = 5f;
    public float stopDistance = 3f;
    public float rotationSpeed = 5f;
    public float flyAttackRange = 10f; // 好きな距離に調整してください
    public int maxHP = 300;

    private int currentHP;

    Animator animator;

    bool canMove = false;
    bool hasStarted = false;
    bool isAttacking = false;
    bool isFlying = false;

    float attackCooldown = 2f;
    float attackTimer = 0f;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        currentHP = maxHP;

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

        if (!isAttacking && !isFlying)
            TryAttack();

        if (canMove) // Run / Fly Forward のときだけ移動・回転
        {
            Move();
            RotateTowardsPlayer();
        }
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
        Vector3 lookPos;

        if (isFlying)
        {
            // 飛行中は高さも考慮して、実際にプレイヤーのいる方向を向く
            lookPos = player.position;
        }
        else
        {
            // 地上では今まで通り水平方向のみ
            lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        }

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
        animator.SetBool("Fly Flame Attack", false);
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

        if (!isFlying && Random.value < 0.1f)
        {
            StartCoroutine(FlyRoutine());
            yield break;
        }

        SetAction("Run");
    }

    IEnumerator FlyRoutine()
    {
        isFlying = true;
        isAttacking = true;

        SetAction("Take Off");
        yield return new WaitForSeconds(4.0f);

        SetAction("Fly Forward");

        // 攻撃範囲に入るまで追尾を続ける
        while (Vector3.Distance(transform.position, player.position) > flyAttackRange)
        {
            yield return null;
        }

        yield return StartCoroutine(FlyFlameAttackRoutine());

        SetAction("Land");
        yield return new WaitForSeconds(4.0f);

        isFlying = false;
        isAttacking = false;

        SetAction("Run");
    }

    IEnumerator FlyFlameAttackRoutine()
    {
        SetAction("Fly Flame Attack");

        fireHitBox.SetActive(true);
        flame.Play();

        yield return new WaitForSeconds(3f);

        fireHitBox.SetActive(false);
        flame.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        if (currentHP < 0)
            currentHP = 0;

        Debug.Log("Boss HP : " + currentHP);

        if (currentHP <= 0)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameClearScene");
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