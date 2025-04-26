using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Kino;

public class TutorialManager : MonoBehaviour
{
    public enum ImagePosition { G, Q, W, E, Cool, Heal }
    [System.Serializable]
    public class ExplanationData
    {
        public Sprite image;
        public string text;
        public ImagePosition imagePosition;
    }

    [Header("Dialogue Settings")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public AudioClip typingSound;
    public float typingSpeed = 0.05f;

    [Header("Explanation Settings")]
    public GameObject imageExplanationPanel;
    public Image imageG, imageQ, imageW, imageE, imageCool, imageHeal;
    public TextMeshProUGUI explanationText_G, explanationText_Q, explanationText_W,
                            explanationText_E, explanationText_Cool, explanationText_Heal;
    public AudioClip skillAudio;
    public List<ExplanationData> explanations = new List<ExplanationData>();
    public RectTransform explanationTextBox;

    [Header("Input Blocker")]
    public GameObject tutorialInputBlocker;

    [Header("Linked Character")]
    public Tutorial_Knight_Move knightMove;

    [Header("Glitch")]
    public AudioClip startSound;
    public DigitalGlitch glitchEffect;
    public AnalogGlitch analogglitch;
    private AudioSource audioSource;

    private bool isTyping = false;
    private bool isTypingExplanation = false;
    private bool isExplaining = false;
    public int dialogueIndex = 0;
    private int currentExplanationIndex = 0;
    private bool isInputBlocked = false;
    private bool waitingForHeal = false;
    private bool healed = false;
    private bool isRespawning = false;

    private string[] dialogues = {
        "�ȳ��ϼ��� �����̿�!",
        "�̰��� ���� ������ ���̴� ����. �̽�ƽ �Ʒ����Դϴ�!",
        "���ݺ��� ������ ���� �⺻ ������ �ȳ��ϰڽ��ϴ�.",
        "�켱 ��ų �����Դϴ�.",
        "�ǰ� ���� ���� �����Ͽ�\n������Ʈ�� Ŭ���Ͽ� ü���� ȸ���غ�����!",
        "���ƿ�! ü���� ȸ���Ǿ����ϴ�!",
        "������ �����ϼ���. �� ȸ���� �� ����� �� �����Դϴ�.",
        "��, ���� ���� �������� ���� ���� óġ�غ��ô�!",
        "�̷�! �� ���� ������ ���̱���!",
        "�� ���� ������ ���� �Ǹ� �װԵ˴ϴ�! �����ϼ���!",
        "���� ��� �ȳ��� �������� ���ư� �ð��Դϴ�.",
        "����� ���ڽ��ϴ�!"
    };
    void Start()
    {
        Debug.Log(isTypingExplanation);
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        BlockInput(true);
        dialoguePanel.SetActive(true);
        StartCoroutine(TypeDialogue());

        if (explanations.Count >= 6)
        {
            explanations[0].text = "G�� �����̵�(�뽬) ��ų�Դϴ�.\n��� �� ���� �Ÿ��� ������ �̵��� �� ������\n��� �߿��� �������� ���� �ʽ��ϴ�.";
            explanations[1].text = "Q�� ���� ��ų�Դϴ�.\nĳ���ͺ� Ư���� ���� �پ��� ȿ���� ���ϴ�.";
            explanations[2].text = "W�� �þ� ��ų�Դϴ�.\nĳ���� �ֺ��� �þ߸� �����ݴϴ�.";
            explanations[3].text = "E�� ���� ��ų�Դϴ�.\nĳ���Ϳ��� �̷ο� ȿ���� �ο��մϴ�.";
            explanations[4].text = "��ų ��Ÿ���� ������ �Ǹ�\n��ų �̹��� �ֺ��� ������ �˴ϴ�.";
            explanations[5].text = "���� �� �ǰ� ��� �Ǿ��� ��\nü���� ȸ���ϴ� �����Դϴ�.\n��� �� �� ���̹Ƿ� �����ϰ� Ȱ���ϼž� �մϴ�!";
        }
    }
    void Update()
    {
        // ���� ���� �� Ŭ�� ����
        if (isExplaining && Input.GetMouseButtonDown(0))
        {
            currentExplanationIndex++;
            if (currentExplanationIndex < explanations.Count)
            {
                StartCoroutine(ShowExplanationSequence());
            }
            else
            {
                isExplaining = false;
                imageExplanationPanel.SetActive(false);
                dialoguePanel.SetActive(true);

                // ���� ���� �� �ڵ� ��� ���
                dialogueIndex = 4;
                StartCoroutine(ShowHealInstruction());
            }
        }

        // �Ϲ� ��� Ŭ�� ó��
        if (dialoguePanel.activeSelf && !isTyping && Input.GetMouseButtonDown(0))
        {
            dialogueIndex++;
            if (dialogueIndex == 4)
            {
                // ��ų ���� ����
                dialoguePanel.SetActive(false);
                imageExplanationPanel.SetActive(true);
                isExplaining = true;
                currentExplanationIndex = 0;
                StartCoroutine(ShowExplanationSequence());
            }
            else if (dialogueIndex < dialogues.Length)
            {
                StartCoroutine(TypeDialogue());
            }
            else
            {
                // ��� ��ȭ�� �������� �� �̵�
                LoadNextScene();
            }
        }
        // �ε��� 7�� ��簡 ��������
        if (dialogueIndex == 7 && !isTyping && dialoguePanel.activeSelf)
        {
            dialoguePanel.SetActive(false); // ��ȭ �г� �����
            BlockInput(false); // �Է� ��� (������ �� �ְ�)
        }

        // ������ ���̰�, ��ȭ �г��� ���� �ִٸ� ���� ��� ����
        if (isRespawning && !dialoguePanel.activeSelf)
        {
            BlockInput(true); // �Է� �ٽ� ����
            dialoguePanel.SetActive(true);
            dialogueIndex = 8; // ���� ��� �ε���
            StartCoroutine(TypeDialogue());
            isRespawning = false; // ������ ���� �ʱ�ȭ
        }
    }

    IEnumerator ShowHealInstruction()
    {
        yield return StartCoroutine(TypeDialogue());

        waitingForHeal = true;
        BlockInput(false); // �Է� ��� (������ �� �ְ�)

        knightMove.EnableMovement();

        // ����ڰ� Ŭ���� ������ ���
        while (!Input.GetMouseButtonDown(0))
        {
            yield return null;
        }

        dialoguePanel.SetActive(false);
        knightMove.SetHealthToLow();
    }

    IEnumerator TypeDialogue()
    {
        isTyping = true;
        dialogueText.text = "";
        int charCount = 0;

        foreach (char letter in dialogues[dialogueIndex].ToCharArray())
        {
            dialogueText.text += letter;
            charCount++;

            if (typingSound != null && audioSource != null && charCount % 4 == 0)
                audioSource.PlayOneShot(typingSound);

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        // Ÿ���� ������ ���� ���� ����
        if (audioSource.isPlaying)
            audioSource.Stop();
    }

    IEnumerator ShowExplanationSequence()
    {
        isTypingExplanation = true;

        var explanation = explanations[currentExplanationIndex];

        imageG.gameObject.SetActive(false);
        imageQ.gameObject.SetActive(false);
        imageW.gameObject.SetActive(false);
        imageE.gameObject.SetActive(false);
        imageCool.gameObject.SetActive(false);
        imageHeal.gameObject.SetActive(false);

        explanationText_G.gameObject.SetActive(false);
        explanationText_Q.gameObject.SetActive(false);
        explanationText_W.gameObject.SetActive(false);
        explanationText_E.gameObject.SetActive(false);
        explanationText_Cool.gameObject.SetActive(false);
        explanationText_Heal.gameObject.SetActive(false);

        Image targetImage = null;
        TextMeshProUGUI targetText = null;

        switch (explanation.imagePosition)
        {
            case ImagePosition.G: targetImage = imageG; targetText = explanationText_G; break;
            case ImagePosition.Q: targetImage = imageQ; targetText = explanationText_Q; break;
            case ImagePosition.W: targetImage = imageW; targetText = explanationText_W; break;
            case ImagePosition.E: targetImage = imageE; targetText = explanationText_E; break;
            case ImagePosition.Cool: targetImage = imageCool; targetText = explanationText_Cool; break;
            case ImagePosition.Heal: targetImage = imageHeal; targetText = explanationText_Heal; break;
        }

        if (targetImage != null)
        {
            targetImage.sprite = explanation.image;
            targetImage.color = Color.white;
            targetImage.gameObject.SetActive(true);
        }

        if (targetText != null)
        {
            targetText.gameObject.SetActive(true);
            targetText.text = explanation.text;
        }

        audioSource?.PlayOneShot(skillAudio);

        isTypingExplanation = false;
        yield return null;
    }

    public void BlockInput(bool block)
    {
        isInputBlocked = block;
        if (tutorialInputBlocker != null)
            tutorialInputBlocker.SetActive(block);
    }

    public bool IsInputBlocked()
    {
        return isInputBlocked;
    }
    public bool IsWaitingForHeal()
    {
        return waitingForHeal && !healed;
    }

    public void OnHealed()
    {
        if (healed) return;

        healed = true;
        waitingForHeal = false;

        BlockInput(true);
        dialoguePanel.SetActive(true);
        dialogueIndex = 5; // "���ƿ�! ü���� ȸ���Ǿ����ϴ�!"
        StartCoroutine(TypeDialogue());
    }
    public void OnCharacterDeath()
    {
        waitingForHeal = false; // �� ��� ���� ����
    }
    public void OnCharacterRespawn()
    {
        isRespawning = true;
    }
    private void LoadNextScene()
    {
        glitchEffect.intensity = 0f;
        analogglitch.scanLineJitter = 0f;
        analogglitch.verticalJump = 0f;
        analogglitch.horizontalShake = 0f;
        analogglitch.colorDrift = 0f;

        StartCoroutine(ToLobbyGlitchAndSound());
    }
    private IEnumerator ToLobbyGlitchAndSound()
    {
        yield return new WaitForSeconds(1f); // 1�� ���

        // �۸�ġ �ѱ�
        if (glitchEffect != null && analogglitch != null)
        {
            glitchEffect.enabled = true;
            analogglitch.enabled = true;
            StartCoroutine(IncreaseGlitch());
        }

        // ȿ���� ���
        if (startSound != null)
        {
            audioSource.PlayOneShot(startSound);
            yield return new WaitForSeconds(startSound.length);
        }

        SceneManager.LoadScene("Main_Lobby");
    }
    private IEnumerator IncreaseGlitch()
    {
        float duration = startSound != null ? startSound.length : 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // Analog �۸�ġ ���� ����
            analogglitch.scanLineJitter = Mathf.Lerp(0.1f, 1.0f, t);
            analogglitch.verticalJump = Mathf.Lerp(0.05f, 0.5f, t);
            analogglitch.horizontalShake = Mathf.Lerp(0.1f, 0.8f, t);
            analogglitch.colorDrift = Mathf.Lerp(0.1f, 1.0f, t);

            // Digital �۸�ġ ���� ����
            glitchEffect.intensity = Mathf.Lerp(0.0f, 1.0f, t);

            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}