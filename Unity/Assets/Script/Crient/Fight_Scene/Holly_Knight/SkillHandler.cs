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

    public AudioClip skillSound; // ȿ���� Ŭ��
    private AudioSource audioSource; //����� �ҽ�
    
    public AudioClip eKeySound;   // E Ű ���� ����
    public AudioClip wKeySound;   // W Ű ���� ����
    public AudioClip gKeySound;   // G Ű ���� ����

    private bool isCooldown = false;


    void Start()
    {
        // AudioSource �ʱ�ȭ
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    void Update()
    {
        TryUseSkill();

        // EŰ ȿ���� ���
        if (Input.GetKeyDown(KeyCode.E) && eKeySound != null)
            audioSource.PlayOneShot(eKeySound);

        // WŰ ȿ���� ���
        if (Input.GetKeyDown(KeyCode.W) && wKeySound != null)
            audioSource.PlayOneShot(wKeySound);

        // GŰ ȿ���� ���
        if (Input.GetKeyDown(KeyCode.G) && gKeySound != null)
            audioSource.PlayOneShot(gKeySound);
    }
    public void TryUseSkill()
    {
        if (!isCooldown && Input.GetKeyDown(skillData.activationKey))
        {
            if (skillSound != null)
            audioSource.PlayOneShot(skillSound);
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

        // Ȥ�� �����ִٸ� �ѱ�
        glowEffect.SetActive(true);

        // CanvasGroup�� ������ ���, ������ �߰�
        CanvasGroup canvasGroup = glowEffect.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = glowEffect.AddComponent<CanvasGroup>();

        // Fade In
        float fadeInTime = 0.1f;
        for (float t = 0; t < fadeInTime; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeInTime);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // ��� ����
        yield return new WaitForSeconds(0.1f);

        // Fade Out
        float fadeOutTime = 0.1f;
        for (float t = 0; t < fadeOutTime; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeOutTime);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        // ������ ������������ ��Ȱ��ȭ
        glowEffect.SetActive(false);
    }
}
