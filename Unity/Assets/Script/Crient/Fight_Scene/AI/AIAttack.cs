using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIAttack : MonoBehaviour
{
    public float detectionRadius = 50f; // ���� �ݰ�
    public float moveSpeed = 1f;

    private Transform target;
    private NavMeshAgent agent;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    void Update()
    {
        FindClosestTarget();

        if (target != null)
            agent.SetDestination(target.position);
    }

    void FindClosestTarget()
    {
        GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag("Player"); // �÷��̾�
        List<GameObject> allTargets = new List<GameObject>(potentialTargets);

        // �߰��� AI�鵵 ���� ��� ����
        GameObject[] aiTargets = GameObject.FindGameObjectsWithTag("AI");

        foreach (GameObject ai in aiTargets)
        {
            // �ڱ� �ڽ��� ����
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
}