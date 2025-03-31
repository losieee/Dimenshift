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

    public Transform spot;
    public Transform player;
    public Transform miniPlayer;
    public Transform dead;
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
    private bool isMove = false;

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

        // ü�� ���� ���
        float healthRatio = curHealth / maxHealth;

        // HP �� ������Ʈ (������ ����)
        infoBarImage.fillAmount = healthRatio;
        characterBarImage.fillAmount = healthRatio;


        if (curHealth <= 0)
        {
            if (!isDead) // �ڷ�ƾ�� ���� ���� �ƴ� ���� ����
            {
                StartCoroutine(RespawnCoroutine()); // ������ �ڷ�ƾ ����
            }
        }
    }

    private IEnumerator RespawnCoroutine()
    {
        isDead = true; // ��� ���� ����
        capsule.enabled = false; // capsule collider ��Ȱ��ȭ (+�״� �̸�� �߰�)
        
        agent.isStopped = true; // �̵� ����
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

        dead.gameObject.SetActive(false);
        //countdownText.gameObject.SetActive(false); // ī��Ʈ�ٿ� UI �����

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
