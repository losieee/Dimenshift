using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerMove : MonoBehaviour
{
    public float curHealth; // ���� ü��
    public float maxHealth; // �ִ� ü��
    public float healAmount; // ȸ���� ����
    public float interactionRange = 5f; // �� ȸ�� ������ �Ÿ� ����

    public Transform spot;
    public Transform player;
    public Transform miniPlayer;
    public Transform dead;
    public Transform LowHp;
    public TextMeshProUGUI countdownText;   // ī��Ʈ�ٿ� UI �ؽ�Ʈ
    private float countdownTime = 5f;       // 5�� ī��Ʈ�ٿ�

    [SerializeField]
    private Image infoBarImage;
    [SerializeField]
    private Image characterBarImage;
    [SerializeField]
    private FieldOfView fieldOfView;
    [SerializeField]
    private SmallFieldOfView smallFieldOfView;

    private new Camera camera;
    private CapsuleCollider capsule;
    private NavMeshAgent agent;
    private Vector3 destination;
    private bool isMove = false;    // ������ ����
    private bool hasHealed = false; // ü�� ȸ�� ���� ����

    public Transform respawnPoint; // ������ ��ġ
    private bool isDead = false; // ��� ���� Ȯ��

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
        dead.gameObject.SetActive(false);
        LowHp.gameObject.SetActive(false);
        healAmount = maxHealth / 2;
    }

    private void ChangeHealthBarAmount(float amount) //* HP ������ ���� 
    {
        infoBarImage.fillAmount = amount;
        characterBarImage.fillAmount = amount;
    }

    public void Damage(float damage) // ������ �޴� �Լ�
    {
        if (maxHealth == 0 || curHealth <= 0 || isDead) // �̹� ü���� 0 �����̰ų� �׾����� �н�
            return;

        curHealth -= damage;
        if (curHealth <= 0)
        {
            StartCoroutine(RespawnCoroutine()); // �ڷ�ƾ ����
        }
        UpdateHealthUI(); // UI ������Ʈ
    }
    private void HealPlayer()
    {
        curHealth += healAmount;
        if (curHealth > maxHealth)
        {
            curHealth = maxHealth;
        }
        UpdateHealthUI();
        hasHealed = true; // ȸ�� �Ϸ� ����
    }
    private void UpdateHealthUI()
    {
        // ü�� ���� ���
        float healthRatio = curHealth / maxHealth;

        // HP �� ������Ʈ (������ ����)
        infoBarImage.fillAmount = healthRatio;
        characterBarImage.fillAmount = healthRatio;

        // ü�� ������ ���� ���� ���� �� LowHp Ȱ��ȭ/��Ȱ��ȭ
        if (healthRatio <= 0.15f) // 15% ����
        {
            infoBarImage.color = Color.red;
            LowHp.gameObject.SetActive(true);
        }
        else if (healthRatio <= 0.5f) // 15% �ʰ� 50% ����
        {
            infoBarImage.color = new Color(1f, 0.5f, 0f); // ��Ȳ�� (RGB: 255, 128, 0)
            LowHp.gameObject.SetActive(false);
        }
        else // 50% �ʰ�
        {
            infoBarImage.color = Color.white; // ���� ���� (���)
            LowHp.gameObject.SetActive(false);
        }
    }

    private IEnumerator RespawnCoroutine()
    {
        isDead = true; // ��� ���� ����
        capsule.enabled = false; // capsule collider ��Ȱ��ȭ (+�״� �̸�� �߰�)
        LowHp.gameObject.SetActive(false); // ��Ȳ�� ���� ��Ȱ��ȭ
        agent.isStopped = true; // �̵� ����
        spot.gameObject.SetActive(false); // spot ��Ȱ��ȭ 
        agent.enabled = false; // �׺���̼� ��Ȱ��ȭ
        dead.gameObject.SetActive(true);



        // �÷��̾ �׾��� �� viewAngle�� 0���� ����
        if (fieldOfView != null)
        {
            fieldOfView.viewAngle = 0;
            smallFieldOfView.viewAngle = 0;
        }
        countdownTime = 5f; // ī��Ʈ�ٿ� �ð�
        while (countdownTime > 0)
        {
            countdownText.text = Mathf.Round(countdownTime).ToString(); // ī��Ʈ �ٿ� UI�� ǥ��
            yield return new WaitForSeconds(1f);
            countdownTime--; // 1�ʾ� ����
        }

        Respawn(); // ������ �Լ� ����
        UpdateHealthUI(); // HP �� ���� �� LowHp ���� �ʱ�ȭ

        if (fieldOfView != null)
        {
            fieldOfView.viewAngle = 69.47f;   //���� ������ ����
            smallFieldOfView.viewAngle = 360;
        }
    }

    private void Respawn()
    {
        // capsule collider �ٽ� Ȱ��ȭ
        capsule.enabled = true;
        agent.enabled = true; // �׺���̼� �ٽ� Ȱ��ȭ
        agent.isStopped = false; // �ٽ� ������
        isDead = false;

        UpdateHealthUI(); // HP �� ���� �� LowHp ���� �ʱ�ȭ

        hasHealed = false; // ������ �� ȸ�� ���� ���·� �ʱ�ȭ

        dead.gameObject.SetActive(false); // ������ ���� ��Ȱ��ȭ

        // ü�� ȸ��
        curHealth = maxHealth;

        // HP �� ����
        infoBarImage.fillAmount = 1f;
        characterBarImage.fillAmount = 1f;

        // ������ ��ġ ����
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1) && !isDead)
        {
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "NPC")
                {
                    // NPC Ŭ�� ��, ���� ȸ������ �ʾ��� ����
                    if (!hasHealed)
                    {
                        float distance = Vector3.Distance(transform.position, hit.point);
                        if (distance <= interactionRange)
                        {
                            HealPlayer();
                        }
                    }
                    spot.gameObject.SetActive(false); // spot ��Ȱ��ȭ (NPC Ŭ�� �� spot ǥ�� �� ��)
                }
                else
                {
                    spot.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                    spot.position = hit.point;
                    spot.gameObject.SetActive(true);
                    SetDestination(hit.point); // NPC�� �ƴ� ��� �����Ӱ� �̵�
                }
            }
        }
        LookMoveDirection();

        if (isDead && LowHp.gameObject.activeSelf)
        {
            LowHp.gameObject.SetActive(false);
        }
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
