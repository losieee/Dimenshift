using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameView gameView;
    private PlayerModel playerModel;
    private GameAPI gameAPI;

    // Start is called before the first frame update
    void Start()
    {
        gameAPI = gameObject.AddComponent<GameAPI>();
        gameView.SetRegisterButtonListener(OnRegisterButtonClicked);
        gameView.SetLoginButtonListeener(OnLoginButtonClicked);
        gameView.SetCollectbuttonListener(OnCollectButtonClicked);
    }

    public void OnRegisterButtonClicked()
    {
        string playerName = gameView.playerNameInput.text;
        StartCoroutine(gameAPI.RegisterPlayer(playerName, "1234"));             // �÷��̾� ��� ��û ������
    }

    public void OnLoginButtonClicked()
    {
        string playerName = gameView.playerNameText.text;
        StartCoroutine(LoginPlayerCoroutine(playerName, "1234"));             // �÷��̾� �α��� ��û ������
    }

    public void OnCollectButtonClicked()
    {
        if (playerModel == null)
        {
            Debug.LogWarning("�÷��̾ �α��ε��� �ʾҽ��ϴ�.");
            return;
        }

        Debug.Log($"Collecting resources for : {playerModel.playerName}");
        StartCoroutine(CollectCoroutine(playerModel.playerName));               // PlayerModel.name ���
    }

    private IEnumerator CollectCoroutine(string playerName)
    {
        yield return gameAPI.CollectResources(playerName, player =>
        {
            if (player != null)
            {
                playerModel.player_count = player.player_count;
                UpdateResourcesDisplay();
            }
            else
            {
                Debug.LogError("���ҽ� ���� ����: �÷��̾� �����Ͱ� null�Դϴ�.");
            }
        });
    }

    private IEnumerator LoginPlayerCoroutine(string playerName, string password)
    {
        yield return gameAPI.LoginPlayer(playerName, password, player =>
        {
            if (player != null)
            {
                playerModel = player;       // �α��� ���� �� PlayerModel ��ü ����
                UpdateResourcesDisplay();   // �α��� ���� �� UI ������Ʈ
            }
            else
            {
                Debug.LogError("�α��� ����: �÷��̾� �����Ͱ� null�Դϴ�.");
            }
        });
    }

    private void UpdateResourcesDisplay()
    {
        if(playerModel != null) // playermodel�� null�� �ƴ� ���� UI ������Ʈ
        {
            gameView.UpdateResource(playerModel.player_count);
        }
    }
}


