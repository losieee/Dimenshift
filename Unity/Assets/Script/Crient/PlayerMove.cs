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
using Unity.VisualScripting;

public class PlayerMove : MonoBehaviour
{
    public Transform spot;
    public Transform player;
    public Transform miniPlayer;
    public Image death;
    public Image LowHp;
    public TextMeshProUGUI respawnCountdownText;   //ī��Ʈ�ٿ� UI �ؽ�Ʈ
    public Transform deathMark;
    public Image skillQCoolTime;    //��ų ��Ÿ��
    public Image skillWCoolTime;
    public Image skillECoolTime;
    public Image skillGCoolTime;

    [SerializeField]
    private Image infoBarImage; //ȭ�� ���� �Ʒ� HP_Bar
    [SerializeField]
    private Image characterBarImage;  //�÷��̾� �� HP_Bar
    [SerializeField]
    private FieldOfView fieldOfView;
    [SerializeField]
    private SmallFieldOfView smallFieldOfView;

    private float curHealth = 100; //���� ü��
    private float maxHealth = 100; //�ִ� ü��
    private float healAmount; //ȸ���� ����
    private float interactionRange = 3f; //�� ȸ�� ������ �Ÿ� ����
    private bool isMove = false;    //������ ����
    private bool hasHealed = false; //ü�� ȸ�� ���� ����
    private float respawnTime = 5f;       //������ �ð� 5��
    private float skillCoolTime;        // ��ų ��Ÿ��
    private float dashDistance = 3f;  // ���� �Ÿ�
    private float dashSpeed = 10f;    // ���� �ӵ�
    private LayerMask obstacleMask;   // ��ֹ� ����

    private new Camera camera;
    private CapsuleCollider capsule;
    private Light spotLight;
    private NavMeshAgent agent;
    private Vector3 destination;

    public Transform respawnPoint; // ������ ��ġ
    private bool isDead = false; // ��� ���� Ȯ��
    private bool isDashing = false; // ���� ������ Ȯ��
    private bool isGSkillCoolTime = false; // G ��ų ��� ���� ����
    private bool isInvincible = false; // ���� ���� ����


    public void SetHp(float amount) // Hp����
    {
        maxHealth = amount;
        curHealth = maxHealth;
    }

    private void Awake()
    {
        camera = Camera.main;
        capsule = GetComponentInChildren<CapsuleCollider>();
        spotLight = GetComponentInChildren<Light>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;   // NavMeshAgent�� ȸ���� ��Ȱ��ȭ
        curHealth = 100;
        death.gameObject.SetActive(false);
        LowHp.gameObject.SetActive(false);
        deathMark.gameObject.SetActive(false);
        skillQCoolTime.gameObject.SetActive(false);
        skillWCoolTime.gameObject.SetActive(false);
        skillECoolTime.gameObject.SetActive(false);
        skillGCoolTime.gameObject.SetActive(false);
        healAmount = maxHealth / 2;
    }

    private void ChangeHealthBarAmount(float amount) //* HP ������ ���� 
    {
        infoBarImage.fillAmount = amount;
        characterBarImage.fillAmount = amount;
    }

    public void Damage(float damage) // ������ �޴� �Լ�
    {
        if (maxHealth == 0 || curHealth <= 0 || isDead || isInvincible) // ���� ������ �� ������ ����
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
        death.gameObject.SetActive(true);
        deathMark.gameObject.SetActive(true); // ������ũ Ȱ��ȭ
        agent.isStopped = true; // �̵� ����
        capsule.enabled = false; // capsule collider ��Ȱ��ȭ (+�״� �̸�� �߰�)
        spotLight.enabled = false;  // light ��Ȱ��ȭ
        LowHp.gameObject.SetActive(false); // ��Ȳ�� ���� ��Ȱ��ȭ
        spot.gameObject.SetActive(false); // spot ��Ȱ��ȭ 
        agent.enabled = false; // �׺���̼� ��Ȱ��ȭ
        




        // �÷��̾ �׾��� �� viewAngle�� 0���� ����
        if (fieldOfView != null)
        {
            fieldOfView.viewAngle = 0;
            smallFieldOfView.viewAngle = 0;
        }
        respawnTime = 5f; // ī��Ʈ�ٿ� �ð�
        while (respawnTime > 0)
        {
            respawnCountdownText.text = Mathf.Round(respawnTime).ToString(); // ī��Ʈ �ٿ� UI�� ǥ��
            yield return new WaitForSeconds(1f);
            respawnTime--; // 1�ʾ� ����
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
        capsule.enabled = true; // capsule collider �ٽ� Ȱ��ȭ
        spotLight.enabled = true; // light �ٽ� Ȱ��ȭ
        agent.enabled = true; // �׺���̼� �ٽ� Ȱ��ȭ
        agent.isStopped = false; // �ٽ� ������
        isDead = false;

        UpdateHealthUI(); // HP �� ���� �� LowHp ���� �ʱ�ȭ

        hasHealed = false; // ������ �� ȸ�� ���� ���·� �ʱ�ȭ

        death.gameObject.SetActive(false); // ������ ���� ��Ȱ��ȭ
        deathMark.gameObject.SetActive(false); // ������ũ ��Ȱ��ȭ

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

        if (Input.GetKeyDown(KeyCode.Q))
        {
            skillCoolTime = 5f;
            StartCoroutine(CooldownRoutine(skillQCoolTime, skillCoolTime));
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            skillCoolTime = 5f;
            StartCoroutine(CooldownRoutine(skillWCoolTime, skillCoolTime));
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            skillCoolTime = 5f;
            StartCoroutine(CooldownRoutine(skillECoolTime, skillCoolTime));
        }
        else if (Input.GetKeyDown(KeyCode.G) && !isGSkillCoolTime) // ��Ÿ�� ���� �� ���� X
        {
            isGSkillCoolTime = true; // G ��ų ��� ������ ����
            skillCoolTime = 8f;
            StartCoroutine(CooldownRoutine(skillGCoolTime, skillCoolTime)); // ��Ÿ�� UI ������Ʈ
            StartCoroutine(DashForward()); // ���� ����
        }
    }
    private IEnumerator DashForward() // �뽬 �Լ�
    {
        isDashing = true; // ���� ���� Ȱ��ȭ
        isInvincible = true; // ���� ���� Ȱ��ȭ

        float dashTime = dashDistance / dashSpeed; // ���� ���� �ð� ���
        Vector3 dashDirection = player.forward; // player�� �ٶ󺸴� �������� �̵�

        // ��ֹ� üũ (Raycast)
        if (Physics.Raycast(transform.position, dashDirection, out RaycastHit hit, dashDistance, obstacleMask))
        {
            dashDistance = hit.distance - 0.5f; // ��ֹ��� �浹���� �ʵ��� ����
        }

        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + dashDirection * dashDistance;

        // �θ� ������Ʈ�� �̵�
        while (elapsedTime < dashTime)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / dashTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition; // ���� ��ġ ����
        isDashing = false; // ���� ���� ����
        isInvincible = false; // ���� ���� ����
    }

    IEnumerator CooldownRoutine(Image skillImage, float cooldownTime)   // ��ų ��Ÿ�� UI ǥ��
    {
        skillImage.gameObject.SetActive(true);
        float elapsedTime = 0f;

        while (elapsedTime < cooldownTime)
        {
            elapsedTime += Time.deltaTime;
            skillImage.fillAmount = 1 - (elapsedTime / cooldownTime);
            yield return null;
        }

        skillImage.gameObject.SetActive(false);

        if (skillImage == skillGCoolTime) // G ��ų�̸� ��Ÿ�� ����
        {
            isGSkillCoolTime = false;
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
