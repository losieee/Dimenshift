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

    public GameObject selectedPrefab;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �� �Ѿ�� �ı� �� ��
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
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Lobby")
        {
            selectedButton = null;
            isTimeOver = false;
            timeLimit = 10f;
            UpdateTimerUI();
        }
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

        string targetScene = (selectedButton != null) ? "SampleScene" : "Lobby";

        // ���� �������� ButtonManager ����
        SceneManager.LoadScene(targetScene);
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