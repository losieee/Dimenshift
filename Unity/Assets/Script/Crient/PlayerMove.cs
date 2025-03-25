using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
    // �÷��̾� �̵� �ӵ�
    public float speed;

    // �÷��̾� ü��
    float curHealth;    // ���� ü��
    public float maxHealth;     // �ִ� ü��
    
    private Camera camera;
    private CapsuleCollider capsule;
    private NavMeshAgent agent;

    public Transform spot;

    private bool isMove = false;

    private Vector3 destination;


    private void Awake()
    {
        camera = Camera.main;
        capsule = GetComponentInChildren<CapsuleCollider>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;   // NavMeshAgent�� ȸ���� ��Ȱ��ȭ
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit))
            {
                spot.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                spot.position = hit.point;
                spot.gameObject.SetActive(true);
                SetDestination(hit.point);
            }
        }
        LookMoveDirection();
    }

    private void SetDestination(Vector3 dest)
    {
        agent.SetDestination(dest);
        destination = dest;
        isMove = true;

        Vector3 direction = (destination - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            direction.y = 0; // y ���� 0���� �����Ͽ� ���� �������θ� ȸ���ϵ��� ����
            capsule.transform.forward = direction;
        }
    }

    private void LookMoveDirection()
    {
        if (isMove)
        {
            spot.transform.localScale = new Vector3(
                Mathf.Max(0, spot.transform.localScale.x - 0.05f),
                Mathf.Max(0, spot.transform.localScale.y - 0.05f),
                Mathf.Max(0, spot.transform.localScale.z - 0.05f));


            if (spot.transform.localScale.x <= 0)
            {
                spot.gameObject.SetActive(false);
            }

            Vector3 moveDirection = agent.velocity.normalized;
            if (moveDirection != Vector3.zero) // velocity�� 0�� �ƴ� ���� ȸ��
            {
                moveDirection.y = 0;    // y ���� 0���� �����Ͽ� ���� �������θ� ȸ��
                capsule.transform.forward = moveDirection;
            }
        }

        if (agent.velocity.magnitude == 0f)
        {
            isMove = false;
            return;
        }

        if (!isMove)
        {
            spot.gameObject.SetActive(false);
        }

    }
}
