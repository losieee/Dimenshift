using System.Collections;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class PhotonTest : MonoBehaviourPunCallbacks
{
    public TMP_InputField nicknameInput;
    public TMP_Text[] nameTexts;

    private static PhotonTest instance;

    private void Awake()
    {
        // �� ��ȯ �Ŀ��� �����ǵ��� ����
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        Screen.SetResolution(960, 600, false);

        nicknameInput = GameObject.Find("Canvas/InputField").GetComponent<TMP_InputField>();

        // �ڵ� �� ����ȭ ����
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void Connect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = nicknameInput.text;

        // �ִ� 4�� �� / ���� ���� �� �� ����
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 4;
        PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: options);
    }

    public override void OnJoinedRoom()
    {
        UpdatePlayerList();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }

    void UpdatePlayerList()
    {
        for (int i = 0; i < nameTexts.Length; i++)
        {
            nameTexts[i].text = "";
        }

        for (int i = 0; i < PhotonNetwork.PlayerList.Length && i < nameTexts.Length; i++)
        {
            nameTexts[i].text = PhotonNetwork.PlayerList[i].NickName;
        }
    }

    // ���常 �� ��ȯ ��ư�� ���� �� ����
    public void StartGameButton()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Character_Choice");
        }
    }
}
