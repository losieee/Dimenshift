using Fusion.Encryption;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIAttack : MonoBehaviour
{
    [Header("AI ����")]
    public float detectionRadius = 50f;
    public float moveSpeed = 1f;
    public float attackRange = 1f;
    public float attackCooldown = 1f;
    public int basicAttackToSkill = 4;

    [Header("ü�� ����")]
    public float maxHealth = 150f;
    public float currentHealth;

    private float attackTimer = 0f;
    private int basicAttackCount = 0;
    private bool isDead = false;

    private Transform target;
    private NavMeshAgent agent;
    private Animator animator;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        FindClosestTarget();

        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            if (distance <= attackRange)
            {
                // ���ڸ����� ����
                agent.ResetPath();
                transform.LookAt(target);

                attackTimer += Time.deltaTime;
                if (attackTimer >= attackCooldown)
                {
                    attackTimer = 0f;
                    BasicAttack();
                }
            }
            else
            {
                agent.SetDestination(target.position);
            }
            // �÷��̾ �׾����� Ȯ���ϰ� AI �ൿ ����
            Tutorial_Knight_Move playerMove = target.GetComponentInParent<Tutorial_Knight_Move>();
            if (playerMove != null && playerMove.isDead)
            {
                // Ÿ���� �׾����Ƿ� ���� �� ���� �ߴ�
                target = null;
                agent.ResetPath();
                agent.isStopped = true;
                agent.velocity = Vector3.zero;

                if (animator != null)
                {
                    animator.SetBool("isRunning", false);
                    animator.SetTrigger("Idle");
                }
            }
            else if (playerMove != null && !playerMove.isDead && distance <= attackRange)
            {
                // ���� ���� ���� ����ִ� �÷��̾ �ִٸ� ���� ���� ����
                agent.isStopped = false;
            }
            else if (target != null)
            {
                // Ÿ���� ����ְ� ���� ���� ���̶�� �߰�
                agent.isStopped = false;
            }
        }
        else
        {
            // Ÿ���� ������ �̵� ����
            agent.ResetPath();
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            if (animator != null)
            {
                animator.SetBool("isRunning", false);
                animator.SetTrigger("Idle");
            }
        }

        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (animator == null || agent == null) return;

        bool isMoving = agent.velocity.magnitude > 0.1f;
        animator.SetBool("isRunning", isMoving);
    }

    void FindClosestTarget()
    {
        GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag("Player");
        List<GameObject> allTargets = new List<GameObject>(potentialTargets);

        GameObject[] aiTargets = GameObject.FindGameObjectsWithTag("AI");
        foreach (GameObject ai in aiTargets)
        {
            if (ai != this.gameObject)
                allTargets.Add(ai);
        }

        float closestDist = Mathf.Infinity;
        Transform closestTarget = null;

        foreach (GameObject obj in allTargets)
        {
            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist < detectionRadius && dist < closestDist)
            {
                closestDist = dist;
                closestTarget = obj.transform;
            }
        }

        target = closestTarget;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    void BasicAttack()
    {
        animator.SetTrigger("Attack");
        DealDamageToPlayer();
        basicAttackCount++;

        if (basicAttackCount >= basicAttackToSkill)
        {
            UseSkill();
            basicAttackCount = 0;
        }
    }

    void UseSkill()
    {
        animator.SetTrigger("Qskill");
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        currentHealth -= dmg;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        agent.ResetPath();
        animator.SetTrigger("isDead");
        Destroy(gameObject, 3f);
    }

    // ���� �ݶ��̴��� ����� �� ������ �ޱ�
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            TakeDamage(10f);
        }
    }
    // AI����
    public void DealDamageToPlayer()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= attackRange && target.CompareTag("Player"))
        {
            Tutorial_Knight_Move playerMove = target.GetComponentInParent<Tutorial_Knight_Move>();
            if (playerMove != null)
            {
                Debug.Log("AI�� ����");
                playerMove.Damage(10f);
            }
            if (playerMove.isDead)
            {

            }
        }
    }
}
