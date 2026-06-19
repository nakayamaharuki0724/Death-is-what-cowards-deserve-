using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public Transform player;
    public float speed = 5f;
    public float stopDistance = 3f;

    Animator animator;

    bool canMove = false;
    bool hasStarted = false;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        StartCoroutine(StartBattle());
    }

    IEnumerator StartBattle()
    {
        hasStarted = true;

        SetAction("Scream");
        yield return new WaitForSeconds(2f);

        SetAction("Run");
    }

    void Update()
    {
        if (!hasStarted) return;

        UpdateMoveState();

        if (canMove)
            Move();
    }

    void UpdateMoveState()
    {
        var state = animator.GetCurrentAnimatorStateInfo(0);

        if (state.IsName("Run") || state.IsName("Fly Forward"))
            canMove = true;
        else
            canMove = false;
    }

    void Move()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > stopDistance)
        {
            Vector3 dir = (player.position - transform.position).normalized;

            transform.position += dir * speed * Time.deltaTime;

            // YŽ²‚ÍŒÅ’è‚µ‚Ä‰¡‚¾‚¯Œü‚­
            Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(lookPos);
        }
    }

    void SetAction(string action)
    {
        ResetAllBools();
        animator.SetBool(action, true);
    }

    void ResetAllBools()
    {
        animator.SetBool("Scream", false);
        animator.SetBool("Run", false);
        animator.SetBool("Fly Forward", false);
        animator.SetBool("Take Off", false);
        animator.SetBool("Land", false);
    }
}