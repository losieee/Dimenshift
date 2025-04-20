using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Linq;

public class PhotonTest : MonoBehaviourPunCallbacks
{
    public TMP_InputField m_InputField;                             // �г��� �Է�â(���� ����)
    public TextMeshProUGUI[] nameTexts = new TextMeshProUGUI[4];    // �÷��̾� �迭

    void Start()
    {
        Screen.SetResolution(960, 600, false);                      // �ػ�
        StartCoroutine(InitializeUI());
    }

    IEnumerator InitializeUI()
    {
        yield return new WaitForEndOfFrame();                       // �������� ���������� ��ٸ��� UI �ʱ�ȭ

        GameObject inputObj = GameObject.Find("Canvas/InputField");
        if (inputObj != null)                                       // inputObj �� �����Ѵٸ�
        {
            m_InputField = inputObj.GetComponent<TMP_InputField>();
        }
        else
        {
            Debug.LogError("[UI] InputField ������Ʈ�� ã�� �� �����ϴ�.");
        }

        for (int i = 0; i < 4; i++)                                 // Text1 ~ Text4 �ʱ�ȭ
        {
            string objectName = $"Canvas/Text{i + 1}";
            GameObject go = GameObject.Find(objectName);
            if (go != null)
            {
                nameTexts[i] = go.GetComponent<TextMeshProUGUI>();
                nameTexts[i].text = "";
            }
            else
            {
                Debug.LogError($"[UI] {objectName} ������Ʈ�� ã�� �� �����ϴ�.");
            }
        }
    }

    public void Connect()                                           // ��ư ����
    {
        if (m_InputField == null)
        {
            Debug.LogError("[Connect] m_InputField�� null�Դϴ�.");
            return;
        }

        if (string.IsNullOrEmpty(m_InputField.text))                // m_InputField�� Text�� ��� ������
        {
            // ����ڰ� �г����� ������ ������ Player1000~9999���� �ڵ����� �̸��� �����ش�.
            m_InputField.text = "Player" + Random.Range(1000, 9999);
            Debug.Log($"player : {m_InputField.text}");
        }

        PhotonNetwork.ConnectUsingSettings();                       // ���� ����
    }

    public override void OnConnectedToMaster()                      // ������ ���� ���� �� ȣ��
    {
        if (m_InputField == null)
        {
            Debug.LogError("[Photon] OnConnectedToMaster���� m_InputField�� null�Դϴ�.");
            return;
        }

        PhotonNetwork.LocalPlayer.NickName = m_InputField.text;

        Debug.Log("[Photon] ������ ���� ���� �Ϸ� �� ���� �� ���� �õ�");
        PhotonNetwork.JoinRandomRoom();                             // �� ���� ������ ����, ������ ���� �ݹ� ȣ��
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[Room] ���� �� ���� ����: {message} �� ���ο� �� ���� �õ�");

        // ������ �� �̸��� ���� ���� ����
        string newRoomName = "Room" + Random.Range(1000, 9999);
        RoomOptions options = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.CreateRoom(newRoomName, options, null);
    }

    public override void OnJoinedRoom()                              // �� ���� ���� ��
    {
        Debug.Log($"[Room] �� ���� ����: {PhotonNetwork.CurrentRoom.Name}");

        // �ٷ� ȣ������ ���� ��¦ �����ؼ� UI �ʱ�ȭ Ÿ�̹� ���߱�
        StartCoroutine(DelayedUpdatePlayerListUI());
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)       // ���ο� �÷��̾� ���� ��
    {
        UpdatePlayerListUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)        // �÷��̾� ���� ��
    {
        UpdatePlayerListUI();
    }

    IEnumerator DelayedUpdatePlayerListUI()
    {
        yield return new WaitForSeconds(0.1f);                        // ���� ª�� ����
        UpdatePlayerListUI();
    }

    void UpdatePlayerListUI()
    {
        if (nameTexts == null || nameTexts.Length == 0)
        {
            Debug.LogWarning("[UI] nameTexts �迭�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return;
        }

        // ��� �÷��̾� ���� (UID ����, �ϰ��� �ְ�)
        Player[] sortedPlayers = PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber).ToArray();

        // ��� �ؽ�Ʈ �ʱ�ȭ
        for (int i = 0; i < nameTexts.Length; i++)
        {
            if (nameTexts[i] != null)
                nameTexts[i].text = "";
        }

        for (int i = 0; i < sortedPlayers.Length; i++)
        {
            string nickname = sortedPlayers[i].NickName;

            if (sortedPlayers[i] == PhotonNetwork.LocalPlayer)
            {
                // ������ �׻� Text1
                nameTexts[0].text = nickname;
            }
            else
            {
                // �ٸ� ������ Text2~Text4
                int otherIndex = GetOtherPlayerIndex(sortedPlayers[i]);
                if (otherIndex >= 1 && otherIndex < nameTexts.Length)
                {
                    nameTexts[otherIndex].text = nickname;
                }
            }
        }
    }

    int GetOtherPlayerIndex(Player player)
    {
        // ���ĵ� �������� ���� �÷��̾ �������� �ٸ� ������ ������ ����
        Player[] sortedPlayers = PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber).ToArray();
        int index = System.Array.IndexOf(sortedPlayers, player);
        int myIndex = System.Array.IndexOf(sortedPlayers, PhotonNetwork.LocalPlayer);

        // ������ ���� �����ϸ� �״��, ������ �ڸ� -1 �ؼ� ���ڸ��� ����
        if (index < myIndex)
            return index + 1;
        else if (index > myIndex)
            return index;

        return 0; // ����
    }
}
