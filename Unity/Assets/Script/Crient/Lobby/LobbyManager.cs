using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    // "���� ����" ��ư�� ������ Character_Choice ������ �̵�
    public void StartGame()
    {
        SceneManager.LoadScene("Character_Choice");
    }
}