using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEditor.TerrainTools;

public class Fight_Demo : MonoBehaviour
{
    [Header("Skill Handlers")]
    [SerializeField] private SkillHandler qSkillHandler;
    [SerializeField] private SkillHandler eSkillHandler;
    [SerializeField] private SkillHandler shiftSkillHandler;

    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashSpeed = 15f;

    public Animator animator;

    private bool canMove = true;
    private bool isMove = false;
    private bool isDashing = false;
    private bool isInvincible = false;

    // 콤보
    private int comboStep = 0;
    private bool isAttacking = false;
    private bool comboQueued = false;
    private bool canQueueNextCombo = false;

    private CapsuleCollider capsule;
    private NavMeshAgent agent;
    private Camera mainCamera;
    private Vector3 destination;
    private Coroutine comboResetCoroutine;

    private Quaternion idleAttackRotationOffset;    //애니메이션 기울이기용

    private void Awake()
    {
        capsule = GetComponentInChildren<CapsuleCollider>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        mainCamera = Camera.main;

        // 오른쪽으로 30도 회전 오프셋 (Y축)
        idleAttackRotationOffset = Quaternion.Euler(0f, 30f, 0f);

        agent.updateRotation = false;
        agent.enabled = true;
        agent.acceleration = 1000f;
        agent.angularSpeed = 10000f;
        agent.stoppingDistance = 0.4f;
        agent.autoBraking = true;

        shiftSkillHandler.onSkillUsed.AddListener(() => StartCoroutine(DashForward()));
    }

    private void Update()
    {
        HandleComboInput();

        if (canMove && !isAttacking)
        {
            HandleKeyboardMoveInput();

            if (Input.GetKeyDown(KeyCode.Q))
                qSkillHandler.TryUseSkill();

            if (Input.GetKeyDown(KeyCode.E))
                eSkillHandler.TryUseSkill();

            if(Input.GetKeyDown(KeyCode.LeftShift))
                shiftSkillHandler.TryUseSkill();
        }

        LookMoveDirection();
    }

    private void HandleKeyboardMoveInput()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        input = input.normalized;

        if (input != Vector3.zero)
        {
            Vector3 moveDir = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0) * input;
            agent.SetDestination(transform.position + moveDir);
            isMove = true;
        }
    }

    private void LookMoveDirection()
    {
        if (!isMove)
        {
            animator.SetBool("isRunning", false);
            return;
        }

        Vector3 moveDirection = agent.desiredVelocity.normalized;

        if (moveDirection != Vector3.zero)
        {
            moveDirection.y = 0;
            capsule.transform.rotation = Quaternion.LookRotation(moveDirection);
        }

        if (agent.enabled && agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            isMove = false;
            agent.ResetPath();
            animator.SetBool("isRunning", false);
        }
        else
        {
            animator.SetBool("isRunning", true);
        }
    }

    private void HandleComboInput()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (canQueueNextCombo && comboStep < 3)
        {
            comboQueued = true;
            return;
        }

        if (isAttacking) return;

        DontMove();

        // 마우스 위치 기준 회전
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Field")))
        {
            Vector3 dir = hit.point - transform.position;
            dir.y = 0;
            if (dir != Vector3.zero)
            {
                capsule.transform.rotation = Quaternion.LookRotation(dir) * idleAttackRotationOffset;
            }
        }

        comboStep = 1;
        isAttacking = true;
        animator.SetInteger("ComboCount", comboStep);
        animator.SetBool("isAttacking", true);
    }

    public void DontMove()
    {
        agent.ResetPath();
        isMove = false;
        canMove = false;
    }

    public void EnableNextCombo()
    {
        canQueueNextCombo = true;
    }

    public void EndCombo()
    {
        if (comboResetCoroutine != null)
            StopCoroutine(comboResetCoroutine);

        comboResetCoroutine = StartCoroutine(DelayedComboReset());
    }

    private IEnumerator DelayedComboReset()
    {
        yield return new WaitForEndOfFrame();

        if (comboQueued && comboStep < 3)
        {
            comboStep++;
            animator.SetInteger("ComboCount", comboStep);
            comboQueued = false;
            canQueueNextCombo = false;
        }
        else
        {
            animator.SetInteger("ComboCount", 0);
            comboStep = 0;
            isAttacking = false;
            comboQueued = false;
            canQueueNextCombo = false;

            canMove = false;
            // 콤보 종료 후 1초 대기
            yield return new WaitForSeconds(0.4f);
            canMove = true;
            animator.SetBool("isAttacking", false);
        }
    }

    public IEnumerator DashForward()
    {
        if (isAttacking)
        {
            // 대시 중에는 공격 상태를 해제
            isAttacking = false;
            comboQueued = false;
            canQueueNextCombo = false;
            comboStep = 0;
            animator.SetInteger("ComboCount", 0);
        }

        isDashing = true;
        isInvincible = true;
        animator.SetTrigger("isDashing");

        float dashTime = dashDistance / dashSpeed;
        Vector3 dashDirection = capsule.transform.forward;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, dashDirection, out hit, dashDistance))
        {
            dashDistance = hit.distance - 0.3f;
        }

        float elapsed = 0f;
        Vector3 start = transform.position;
        Vector3 end = start + dashDirection * dashDistance;

        while (elapsed < dashTime)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / dashTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
        isDashing = false;
        isInvincible = false;

        // 대시 후에는 움직임 가능
        canMove = true;
    }
    public void StopAgentImmediately()
    {
        if (TryGetComponent(out NavMeshAgent agent))
        {
            agent.ResetPath(); // 현재 경로 즉시 정지
            agent.velocity = Vector3.zero;
        }

        // 애니메이션 이동 상태 해제 (선택)
        if (TryGetComponent(out Animator anim))
        {
            anim.SetBool("isRunning", false);
            anim.SetBool("isRunning", false);
        }

        // 상태 플래그도 변경 (선택)
        canMove = false;
        isMove = false;
    }
    public void ResumeMovement()
    {
        canMove = true;
    }
    public bool IsAttacking()
    {
        return isAttacking;
    }
}
