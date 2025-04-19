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
    public TMP_InputField m_InputField;
    public TextMeshProUGUI[] nameTexts = new TextMeshProUGUI[4];

    void Start()
    {
        Screen.SetResolution(960, 600, false);
        StartCoroutine(InitializeUI());
    }

    IEnumerator InitializeUI()
    {
        yield return new WaitForEndOfFrame();

        GameObject inputObj = GameObject.Find("Canvas/InputField");
        if (inputObj != null)
        {
            m_InputField = inputObj.GetComponent<TMP_InputField>();
        }
        else
        {
            Debug.LogError("[UI] InputField ������Ʈ�� ã�� �� �����ϴ�.");
        }

        for (int i = 0; i < 4; i++)
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

    public void Connect()
    {
        if (m_InputField == null)
        {
            Debug.LogError("[Connect] m_InputField�� null�Դϴ�.");
            return;
        }

        if (string.IsNullOrEmpty(m_InputField.text))
        {
            m_InputField.text = "Player" + Random.Range(1000, 9999);
        }

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        if (m_InputField == null)
        {
            Debug.LogError("[Photon] OnConnectedToMaster���� m_InputField�� null�Դϴ�.");
            return;
        }

        PhotonNetwork.LocalPlayer.NickName = m_InputField.text;

        RoomOptions options = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.JoinOrCreateRoom("Room1", options, null);
    }

    public override void OnJoinedRoom()
    {
        UpdatePlayerListUI();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerListUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerListUI();
    }

    void UpdatePlayerListUI()
    {
        // ��� �÷��̾� ���� (UID ����, �ϰ��� �ְ�)
        Player[] sortedPlayers = PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber).ToArray();

        for (int i = 0; i < nameTexts.Length; i++)
        {
            if (i < sortedPlayers.Length)
            {
                string nickname = sortedPlayers[i].NickName;

                // ������ �׻� Text1
                if (sortedPlayers[i] == PhotonNetwork.LocalPlayer)
                {
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
            else
            {
                // �� ������ �����
                nameTexts[i].text = "";
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
