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
using System;

public class PlayerMove : MonoBehaviour
{
    public Transform spot;
    public Transform player;
    public Transform miniPlayer;
    public Image death;
    public Image lowHp;
    public Image heal;
    public TextMeshProUGUI respawnCountdownText;   //ī��Ʈ�ٿ� UI �ؽ�Ʈ
    public TextMeshProUGUI skillQTimeText;
    public TextMeshProUGUI skillWTimeText;
    public TextMeshProUGUI skillETimeText;
    public TextMeshProUGUI skillGTimeText;
    public Transform deathMark;
    public Image skillQCoolTime;    //��ų ��Ÿ��
    public Image skillWCoolTime;
    public Image skillECoolTime;
    public Image skillGCoolTime;
    public GameObject glowQ;        // ��Ÿ�� ���� �� ��¦��
    public GameObject glowW;
    public GameObject glowE;
    public GameObject glowG;
    public ParticleSystem healEffect;   // �� ��ƼŬ


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

    private bool isQCoolTime = false;   // ��Ÿ�� ����
    private bool isWCoolTime = false;
    private bool isECoolTime = false;

    private new Camera camera;
    private CapsuleCollider capsule;
    private Light spotLight;
    private NavMeshAgent agent;
    private Vector3 destination;
    private CanvasGroup healCanvasGroup;

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
        Debug.Log(isDashing);  // ���� ������ ��� ǥ��
        camera = Camera.main;
        capsule = GetComponentInChildren<CapsuleCollider>();
        spotLight = GetComponentInChildren<Light>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;   // NavMeshAgent�� ȸ���� ��Ȱ��ȭ
        curHealth = 100;
        death.gameObject.SetActive(false);
        lowHp.gameObject.SetActive(false);
        heal.gameObject.SetActive(false);
        deathMark.gameObject.SetActive(false);
        skillQCoolTime.gameObject.SetActive(false);
        skillWCoolTime.gameObject.SetActive(false);
        skillECoolTime.gameObject.SetActive(false);
        skillGCoolTime.gameObject.SetActive(false);
        healAmount = maxHealth / 2;
    }

    private void Start()
    {
        healCanvasGroup = heal.GetComponent<CanvasGroup>();
        if (healCanvasGroup == null)
        {
            healCanvasGroup = heal.gameObject.AddComponent<CanvasGroup>();
        }
        healCanvasGroup.alpha = 0f;
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
    private IEnumerator ShowHealEffectSmooth()  // ���� ������ �׵θ��� ������ ��Ÿ���� ������
    {
        heal.gameObject.SetActive(true);

        // ���̵� ��
        for (float t = 0; t < 1f; t += Time.deltaTime / 0.3f)
        {
            healCanvasGroup.alpha = t;
            yield return null;
        }
        healCanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(0.1f); // ���� ���̴� �ð�

        // ���̵� �ƿ�
        for (float t = 1f; t > 0f; t -= Time.deltaTime / 0.3f)
        {
            healCanvasGroup.alpha = t;
            yield return null;
        }
        healCanvasGroup.alpha = 0f;
        heal.gameObject.SetActive(false);
    }

    private void HealPlayer()
    {
        if (curHealth >= maxHealth)     // �̹� �ִ� ü���� ��� �� ���� �� ��
            return;
            
        curHealth += healAmount;
        StartCoroutine(ShowHealEffectSmooth());

        if (curHealth > maxHealth)
        {
            curHealth = maxHealth;
        }

        UpdateHealthUI();
        hasHealed = true;

        if (healEffect != null)
        {
            healEffect.Play(); // Heal ��ƼŬ ���
        }
    }
    private void UpdateHealthUI()
    {
        // ü�� ���� ���
        float healthRatio = curHealth / maxHealth;

        // HP �� ������Ʈ (������ ����)
        infoBarImage.fillAmount = healthRatio;
        characterBarImage.fillAmount = healthRatio;

        // ü�� ������ ���� ���� ���� �� LowHp Ȱ��ȭ/��Ȱ��ȭ
        if (healthRatio <= 0.1f) // 10% ����
        {
            infoBarImage.color = Color.red;
            lowHp.gameObject.SetActive(true);
        }
        else if (healthRatio <= 0.5f) // 10% �ʰ� 50% ����
        {
            infoBarImage.color = new Color(1f, 0.5f, 0f);
            lowHp.gameObject.SetActive(false);
        }
        else // 50% �ʰ�
        {
            infoBarImage.color = Color.white; // ���� ���� (���)
            lowHp.gameObject.SetActive(false);
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
        lowHp.gameObject.SetActive(false); // ��Ȳ�� ���� ��Ȱ��ȭ
        spot.gameObject.SetActive(false); // spot ��Ȱ��ȭ 
        agent.enabled = false; // �׺���̼� ��Ȱ��ȭ


        // �÷��̾ �׾��� �� viewAngle�� 0���� ���� (��������)
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
        healEffect.Play(); // Heal ��ƼŬ ���

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
                Transform capsuleChild = null; // "capsule" �̸��� ���� �ڽ� ������Ʈ�� ã�� ���� ����
                foreach (Transform child in transform)
                {
                    if (child.name == "Capsule")
                    {
                        capsuleChild = child;
                        break; // "capsule"�� ã������ ���� ����
                    }
                }
                // �÷��̾� �ڽ� �Ǵ� �ڽ� ������Ʈ�� Ŭ���ߴ��� Ȯ��
                if (hit.collider.gameObject == gameObject || (capsuleChild != null && hit.collider.gameObject == capsuleChild.gameObject))
                {
                    return; // �÷��̾� �ڽ� �Ǵ� �ڽ� ������Ʈ�� Ŭ���ߴٸ� �ƹ��͵� ���� �ʰ� �Լ� ����
                }

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

        if (isDead && lowHp.gameObject.activeSelf)
        {
            lowHp.gameObject.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Q) && !isQCoolTime)
        {
            isQCoolTime = true;
            skillCoolTime = 5f;
            StartCoroutine(CooldownRoutine(skillQCoolTime, skillCoolTime, () => isQCoolTime = false, skillQTimeText, glowQ));
        }
        else if (Input.GetKeyDown(KeyCode.W) && !isWCoolTime)
        {
            isWCoolTime = true;
            skillCoolTime = 5f;
            StartCoroutine(CooldownRoutine(skillWCoolTime, skillCoolTime, () => isWCoolTime = false, skillWTimeText, glowW));
        }
        else if (Input.GetKeyDown(KeyCode.E) && !isECoolTime)
        {
            isECoolTime = true;
            skillCoolTime = 5f;
            StartCoroutine(CooldownRoutine(skillECoolTime, skillCoolTime, () => isECoolTime = false, skillETimeText, glowE));
        }
        else if (Input.GetKeyDown(KeyCode.G) && !isGSkillCoolTime)
        {
            isGSkillCoolTime = true;
            skillCoolTime = 8f;
            StartCoroutine(CooldownRoutine(skillGCoolTime, skillCoolTime, () => isGSkillCoolTime = false, skillGTimeText, glowG));
            StartCoroutine(DashForward());
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

    IEnumerator CooldownRoutine(Image skillImage, float cooldownTime, System.Action onCooldownEnd, TextMeshProUGUI cooldownText = null, GameObject glowObject = null)
    {
        skillImage.gameObject.SetActive(true);
        if (cooldownText != null)
            cooldownText.gameObject.SetActive(true);

        float elapsedTime = 0f;

        while (elapsedTime < cooldownTime)
        {
            elapsedTime += Time.deltaTime;
            float remaining = cooldownTime - elapsedTime;
            skillImage.fillAmount = 1 - (elapsedTime / cooldownTime);

            if (cooldownText != null)
                cooldownText.text = Mathf.CeilToInt(remaining).ToString();

            yield return null;
        }

        skillImage.gameObject.SetActive(false);
        if (cooldownText != null)
            cooldownText.gameObject.SetActive(false);

        onCooldownEnd?.Invoke();

        if (glowObject != null)
            StartCoroutine(ShowGlowBriefly(glowObject));  // ��¦�� ����
    }

    private IEnumerator ShowGlowBriefly(GameObject glowObject)
{
    glowObject.SetActive(true);
    yield return new WaitForSeconds(0.2f);
    glowObject.SetActive(false);
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
