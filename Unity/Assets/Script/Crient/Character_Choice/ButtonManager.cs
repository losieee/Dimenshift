using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

public class ButtonManager : MonoBehaviour
{
    public static ButtonManager Instance { get; private set; }
    private ButtonEffect selectedButton = null;
    private float timeLimit = 10f; // ���ѽð� 10��
    private bool isTimeOver = false; // �ð� �ʰ� ����
    public TMP_Text timerText; // UI�� ǥ���� Ÿ�̸� �ؽ�Ʈ

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // ���ѽð� ī��Ʈ�ٿ� ����
        Invoke("TimeOver", timeLimit);
    }

    void Update()
    {
        if (!isTimeOver)
        {
            timeLimit -= Time.deltaTime; // ���� �ð� ����
            if (timeLimit < 0) timeLimit = 0; // 0�� ���Ϸ� �������� �ʵ��� ����
            UpdateTimerUI();
        }
    }

    // UI Ÿ�̸� ������Ʈ
    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = timeLimit.ToString("F0"); // ������ ǥ��
        }
    }

    // ��ư�� ������ ��� �� �̵����� �ʰ� ���ø� ����
    public bool CanSelect()
    {
        return selectedButton == null && !isTimeOver;
    }

    // ��ư�� ���õǾ��� �� ȣ��
    public void SetSelectedButton(ButtonEffect button)
    {
        selectedButton = button;
        DisableOtherButtons();
    }

    // �ð��� �� ������, ��ư�� ���ȴ��� ���ο� ���� �� �̵�
    private void TimeOver()
    {
        isTimeOver = true;
        if (selectedButton != null)
        {
            SceneManager.LoadScene("SampleScene"); // ��ư�� ���ȴٸ� SampleScene���� �̵�
        }
        else
        {
            SceneManager.LoadScene("Lobby"); // �ƹ� ��ư�� �� ���ȴٸ� Lobby�� �̵�
        }
    }

    // �ٸ� ��ư�� ��Ȱ��ȭ
    private void DisableOtherButtons()
    {
        ButtonEffect[] allButtons = FindObjectsOfType<ButtonEffect>();
        foreach (ButtonEffect button in allButtons)
        {
            if (button != selectedButton)
            {
                button.DisableButton();
            }
        }
    }
}