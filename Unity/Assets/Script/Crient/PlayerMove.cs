using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerMove : MonoBehaviour
{
    public float curHealth; // ���� ü��
    public float maxHealth; // �ִ� ü��

    public Slider HpBarSlider;
    public Transform spot;
    public Transform player;
    public Transform miniPlayer;
    public Transform hpBar;

    private new Camera camera;
    private CapsuleCollider capsule;
    private NavMeshAgent agent;
    private Vector3 destination;
    private bool isMove = false;


    public void SetHp(float amount) // Hp����
    {
        maxHealth = amount;
        curHealth = maxHealth;
    }

    private void Awake()
    {
        camera = Camera.main;
        capsule = GetComponentInChildren<CapsuleCollider>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;   // NavMeshAgent�� ȸ���� ��Ȱ��ȭ
        curHealth = 100;
    }
    public void SetHealth(float health)
    {
        curHealth = health;
        CheckHp(); // ü�� UI ����
    }

    public void CheckHp() //*HP ����
    {
        if (HpBarSlider != null)
            HpBarSlider.value = curHealth / maxHealth;
    }

    public void Damage(float damage) // ������ �޴� �Լ�
    {
        if (maxHealth == 0 || curHealth <= 0) // �̹� ü�� 0���ϸ� �н�
            return;
        curHealth -= damage;
        Debug.Log("�������� ����");
        CheckHp(); // ü�� ����
        if (curHealth <= 0)
        {
            SceneManager.LoadScene("Dead");
        }
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
            // spot ũ�� �ٿ��ֱ�
            spot.transform.localScale = new Vector3(
                Mathf.Max(0, spot.transform.localScale.x - 0.05f),
                Mathf.Max(0, spot.transform.localScale.y - 0.05f),
                Mathf.Max(0, spot.transform.localScale.z - 0.05f));

            // spot ũ�Ⱑ 0�� �Ǹ� ��Ȱ��ȭ
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
