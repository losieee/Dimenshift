using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public AudioClip startSound; // Start ��ư ȿ����
    private AudioSource audioSource;

    private bool isClicked = false; // �ߺ� Ŭ�� ����

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    // "���� ����" ��ư�� ������ ȿ���� ��� �� Character_Choice ������ �̵�
    public void StartGame()
    {
        if (isClicked) return; // �ߺ� Ŭ�� ����
        isClicked = true;

        if (startSound != null)
            audioSource.PlayOneShot(startSound);

        // ȿ���� ���̸�ŭ ��ٷȴٰ� �� ��ȯ
        float delay = startSound != null ? startSound.length : 0f;
        Invoke(nameof(LoadNextScene), delay);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene("Character_Choice");
    }
}
