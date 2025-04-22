using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Kino;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    public AudioClip startSound;
    private AudioSource audioSource;
    
    public DigitalGlitch glitchEffect;
    public AnalogGlitch analogGlitch;

    private bool isClicked = false;

    private void Awake()
    {
        // �̱��� ���� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // �� ��ȯ �� ������Ʈ ����
        }
        else
        {
            Destroy(gameObject);  // �̹� �̱����� �����ϸ� �ڽ��� ����
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

        if (glitchEffect != null) glitchEffect.enabled = false;
        if (analogGlitch != null) analogGlitch.enabled = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Main_Lobby�� Character_Lobby �������� �����ϰ� ������ �������� ����
        if (scene.name == "Main_Lobby" || scene.name == "Character_Lobby")
        {
            DontDestroyOnLoad(gameObject);  // �ش� �������� ����
        }
        else
        {
            Destroy(gameObject);  // �ٸ� �������� ����
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        if (isClicked) return;
        isClicked = true;
        ResetGlitch();
        StartCoroutine(DelayedGlitchAndSound("Character_Choice"));
    }

    public void Tutorial()
    {
        if (isClicked) return;
        isClicked = true;
        ResetGlitch();
        StartCoroutine(DelayedGlitchAndSound("Tutorial"));
    }

    private void ResetGlitch()
    {
        if (glitchEffect != null) glitchEffect.intensity = 0f;
        if (analogGlitch != null)
        {
            analogGlitch.scanLineJitter = 0f;
            analogGlitch.verticalJump = 0f;
            analogGlitch.horizontalShake = 0f;
            analogGlitch.colorDrift = 0f;
        }
    }

    private IEnumerator DelayedGlitchAndSound(string sceneName)
    {
        yield return new WaitForSeconds(1f); // 1�� ���

        // �۸�ġ ȿ�� �ѱ�
        if (glitchEffect != null && analogGlitch != null)
        {
            glitchEffect.enabled = true;
            analogGlitch.enabled = true;
            StartCoroutine(IncreaseGlitch());
        }

        // ȿ���� ���
        if (startSound != null)
        {
            audioSource.PlayOneShot(startSound);
            yield return new WaitForSeconds(startSound.length);
        }

        // �� ��ȯ
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator IncreaseGlitch()
    {
        float duration = startSound != null ? startSound.length : 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // Analog �۸�ġ ���� ����
            if (analogGlitch != null)
            {
                analogGlitch.scanLineJitter = Mathf.Lerp(0.1f, 1.0f, t);
                analogGlitch.verticalJump = Mathf.Lerp(0.05f, 0.5f, t);
                analogGlitch.horizontalShake = Mathf.Lerp(0.1f, 0.8f, t);
                analogGlitch.colorDrift = Mathf.Lerp(0.1f, 1.0f, t);
            }

            // Digital �۸�ġ ���� ����
            if (glitchEffect != null)
            {
                glitchEffect.intensity = Mathf.Lerp(0.0f, 1.0f, t);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}