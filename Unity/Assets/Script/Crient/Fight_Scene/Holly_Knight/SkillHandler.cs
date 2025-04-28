using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SkillHandler : MonoBehaviour
{
    public SkillData skillData;

    public Image cooldownImage;
    public TextMeshProUGUI cooldownText;
    public GameObject glowEffect;
    public UnityEngine.Events.UnityEvent onSkillUsed;

    public AudioClip skillSound; // ��ų ����
    public AudioClip gKeySound;
    public AudioClip qKeySound;
    public AudioClip wKeySound;
    public AudioClip eKeySound;

    private Animator animator;  // ��ų �ִϸ��̼�

    public Tutorial_Knight_Move knight_Move;

    private AudioSource audioSource;

    private bool isCooldown = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        animator = GetComponent<Animator>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        // GŰ�� ������ �� Ư�� ���� ��� (��Ÿ�� �ƴ� ����)
        if (Input.GetKeyDown(KeyCode.G) && gKeySound != null && !isCooldown)
        {
            audioSource.PlayOneShot(gKeySound);
        }

        if (Input.GetKeyDown(KeyCode.Q) && qKeySound != null && !isCooldown)
        {
            audioSource.PlayOneShot(qKeySound);
            knight_Move.animator.SetTrigger("Qskill");
        }

        if (Input.GetKeyDown(KeyCode.W) && wKeySound != null && !isCooldown)
        {
            audioSource.PlayOneShot(wKeySound);
        }

        if (Input.GetKeyDown(KeyCode.E) && eKeySound != null && !isCooldown)
        {
            audioSource.PlayOneShot(eKeySound);
        }

        TryUseSkill();
    }

    public void TryUseSkill()
    {
        if (!isCooldown && Input.GetKeyDown(skillData.activationKey))
        {
            if (skillSound != null)
                audioSource.PlayOneShot(skillSound); // �Ϲ� ��ų ȿ���� ���

            StartCoroutine(CooldownRoutine());
        }
    }

    private IEnumerator CooldownRoutine()
    {
        onSkillUsed?.Invoke(); // ��� �� ���� ����

        isCooldown = true;
        cooldownImage.gameObject.SetActive(true);
        cooldownText.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < skillData.cooldownTime)
        {
            elapsed += Time.deltaTime;
            float remaining = skillData.cooldownTime - elapsed;

            cooldownImage.fillAmount = 1 - (elapsed / skillData.cooldownTime);
            cooldownText.text = Mathf.CeilToInt(remaining).ToString();

            yield return null;
        }

        cooldownImage.gameObject.SetActive(false);
        cooldownText.gameObject.SetActive(false);
        isCooldown = false;

        if (glowEffect != null)
            StartCoroutine(ShowGlowBriefly());
    }

    private IEnumerator ShowGlowBriefly()
    {
        if (glowEffect == null) yield break;

        glowEffect.SetActive(true);

        CanvasGroup canvasGroup = glowEffect.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = glowEffect.AddComponent<CanvasGroup>();

        float fadeInTime = 0.1f;
        for (float t = 0; t < fadeInTime; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeInTime);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(0.1f);

        float fadeOutTime = 0.1f;
        for (float t = 0; t < fadeOutTime; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeOutTime);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        glowEffect.SetActive(false);
    }
}