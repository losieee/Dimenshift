using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public enum ImagePosition
    {
        G, Q, W, E, Cool, Heal
    }

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
    public Image imageG;
    public Image imageQ;
    public Image imageW;
    public Image imageE;
    public Image imageCool;
    public Image imageHeal;

    public TextMeshProUGUI explanationText_G;
    public TextMeshProUGUI explanationText_Q;
    public TextMeshProUGUI explanationText_W;
    public TextMeshProUGUI explanationText_E;
    public TextMeshProUGUI explanationText_Cool;
    public TextMeshProUGUI explanationText_Heal;

    public AudioClip skillAudio;

    public List<ExplanationData> explanations = new List<ExplanationData>();
    public RectTransform explanationTextBox;

    [Header("Input Blocker")]
    public GameObject tutorialInputBlocker; // Inspector â���� �Ҵ�

    private AudioSource audioSource;
    private bool isTyping = false;
    private bool isTypingExplanation = false;
    private bool dialogueEnded = false;
    private bool isExplaining = false;
    private float inactivityTimer = 0f;
    private float waitTime = 3f;
    private int dialogueIndex = 0;
    private int currentExplanationIndex = 0;
    private bool isInputBlocked = false; // �Է� ���� ����

    private string[] dialogues = {
        "�ȳ��ϼ��� �����̿�!",
        "�̰��� ���� ������ ���̴� ����. �̽�ƽ �Ʒ����Դϴ�!",
        "���ݺ��� ������ ���� �⺻ ������ �ȳ��ϰڽ��ϴ�.",
        "�켱 ��ų �����Դϴ�."
    };

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // ���� �ؽ�Ʈ �ӽ� ����
        if (explanations.Count >= 6) // explanations ����Ʈ ũ�⸦ 6 �̻����� Ȯ��
        {
            explanations[0].text = "G�� �����̵�(�뽬) ��ų�Դϴ�.\n���� ���� �Ÿ��� ������ �̵��� �� ������\n����߿��� �������� ���� �ʽ��ϴ�.";
            explanations[1].text = "Q�� ���� ��ų�Դϴ�.\nĳ���ͺ� Ư���� ���� �پ��� ȿ���� ���ϴ�.";
            explanations[2].text = "W�� �þ� ��ų�Դϴ�.\nĳ���� �ֺ��� �þ߸� �����ݴϴ�.";
            explanations[3].text = "E�� ���� ��ų�Դϴ�.\nĳ���Ϳ��� �̷ο� ȿ���� �ο��մϴ�.";
            explanations[4].text = "��ų ��Ÿ���� ������ �Ǹ�\n��ų �̹��� �ֺ��� ������ �˴ϴ�.";
            explanations[5].text = "���� �� �ǰ� ��� �Ǿ��� �� ü���� ȸ���ϴ� �����Դϴ�.\n��� �� �ѹ��̹Ƿ� �����ϰ� Ȱ���ϼž� �մϴ�!";
        }

        dialoguePanel.SetActive(true);
        BlockInput(true); // ���̾�α� ���� �� �Է� ����
        StartCoroutine(TypeDialogue());
    }

    void Update()
    {
        // ���̾�α� �г��� Ȱ��ȭ�Ǿ� ���� �� Ŭ�� ����
        if (dialoguePanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = dialogues[dialogueIndex];
                isTyping = false;
            }
            else
            {
                dialogueIndex++;
                if (dialogueIndex < dialogues.Length)
                {
                    StartCoroutine(TypeDialogue());
                }
                else
                {
                    dialoguePanel.SetActive(false);
                    dialogueEnded = true;
                    inactivityTimer = 0f;

                    imageExplanationPanel.SetActive(true);
                    isExplaining = true;
                    currentExplanationIndex = 0;
                    BlockInput(true); // ���� ���� �� �Է� ����
                    StartCoroutine(ShowExplanationSequence());
                }
            }
        }

        // ���� �г��� Ȱ��ȭ�Ǿ� ���� �� Ŭ�� ����
        if (isExplaining && Input.GetMouseButtonDown(0))
        {
            if (isTypingExplanation)
            {
                StopAllCoroutines();
                ShowFullExplanationText();
                isTypingExplanation = false;
            }
            else
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
                    BlockInput(false); // ���� ���� �� �Է� ���
                }
            }
        }

        // ���̾�α׿� ������ ��� ������ ��
        if (dialogueEnded && !isExplaining)
        {
            if (Input.anyKeyDown)
            {
                inactivityTimer = 0f;
            }
            else
            {
                inactivityTimer += Time.deltaTime;
                if (inactivityTimer >= waitTime)
                {
                    dialogueEnded = false;
                }
            }
        }
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

            if (typingSound != null && audioSource != null && charCount % 3 == 0)
            {
                audioSource.PlayOneShot(typingSound);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    IEnumerator ShowExplanationSequence()
    {
        isTypingExplanation = true;

        var explanation = explanations[currentExplanationIndex];

        // ��� �̹���/�ؽ�Ʈ ��Ȱ��ȭ
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
            case ImagePosition.G:
                targetImage = imageG;
                targetText = explanationText_G;
                audioSource.PlayOneShot(skillAudio);
                break;
            case ImagePosition.Q:
                targetImage = imageQ;
                targetText = explanationText_Q;
                audioSource.PlayOneShot(skillAudio);
                break;
            case ImagePosition.W:
                targetImage = imageW;
                targetText = explanationText_W;
                audioSource.PlayOneShot(skillAudio);
                break;
            case ImagePosition.E:
                targetImage = imageE;
                targetText = explanationText_E;
                audioSource.PlayOneShot(skillAudio);
                break;
            case ImagePosition.Cool:
                targetImage = imageCool;
                targetText = explanationText_Cool;
                audioSource.PlayOneShot(skillAudio);
                break;
            case ImagePosition.Heal:
                targetImage = imageHeal;
                targetText = explanationText_Heal;
                audioSource.PlayOneShot(skillAudio);
                break;
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
            targetText.text = explanation.text;  // Ÿ���� ȿ�� ���� ��� ���
        }

        yield return null;

        isTypingExplanation = false;
    }

    void ShowFullExplanationText()
    {
        var explanation = explanations[currentExplanationIndex];
        switch (explanation.imagePosition)
        {
            case ImagePosition.G:
                explanationText_G.text = explanation.text;
                break;
            case ImagePosition.Q:
                explanationText_Q.text = explanation.text;
                break;
            case ImagePosition.W:
                explanationText_W.text = explanation.text;
                break;
            case ImagePosition.E:
                explanationText_E.text = explanation.text;
                break;
            case ImagePosition.Cool:
                explanationText_Cool.text = explanation.text;
                break;
        }
    }

    public void BlockInput(bool block)
    {
        isInputBlocked = block;
        if (tutorialInputBlocker != null)
        {
            tutorialInputBlocker.SetActive(block);
        }
    }
    public bool IsInputBlocked()
    {
        return isInputBlocked;
    }
}