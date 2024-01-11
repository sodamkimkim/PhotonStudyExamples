using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class PhotonLauncher : MonoBehaviourPunCallbacks
{ // ���� ���� & �����ϱ� ���ؼ� ¥���� class PhotonLauncher
    [SerializeField] private string gameVersion = "0.0.1";
    [SerializeField] private byte maxPlayerPerRoom = 4;
    [SerializeField] private string nickName = string.Empty;
    [SerializeField] private Button connectButton = null;
    /*
    # ���� ��Ʈ��ũ���� ���� �κ� �� ���� ����
     �� ���� ���� -> �κ� �� �ִٰ� �뿡�� �׽�����.
     �� ���� �� ������ �κ� ������ �� ��
     */

    private void Awake()
    {
        // �� �������� �����Ͱ� �Ǵµ�, �����Ͱ� �� �θ��� �÷��̾� ���ÿ� �� �����
        // �� ����ȭ �����ַ��� �̰��� ������ �Ѵ�.
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        connectButton.interactable = true;
    }

    // ConnectButton�� �������� ȣ��
    public void Connect()
    {
        if (string.IsNullOrEmpty(nickName))
        {
            Debug.Log("NickName is empty");
            return;
        }

        if (PhotonNetwork.IsConnected) // ���� ������ ���ӵ� �������� üũ
        {
            PhotonNetwork.JoinRandomRoom(); // ���� �� �� �ִ� ���� �������� �ϳ� ã�´�.
            //PhotonNetwork.JoinRoom()
            //PhotonNetwork.LeaveLobby()
        }
        else
        { // ó������ ������ �ȵǾ� �����ϱ� ������ ����� ���´�.
            Debug.LogFormat("Connect : {0}", gameVersion);
            PhotonNetwork.GameVersion = gameVersion; // PhotonNetwork ���� ���ƾ� ���� ���ӿ� ������ �� �ִ�.
            //PhotonNetwork.GameVersion = Application.version; // ���� ���ӹ��� ���� ���ҰŸ� ����Ƽ ������Ʈ ���� �־� ����ϸ� �ȴ�.

            // ���� Ŭ���忡 ������ �����ϴ� ����
            // ���ӿ� �����ϸ� OnConnectedToMaster �޼��� ȣ��(callback)
            PhotonNetwork.ConnectUsingSettings();// �����Ʈ��ũ�� ������ �⺻ ������ ������ �ϰڴ�.
            //PhotonNetwork.ConnectUsingSettings(AppSettings appSettings, [bool startInOfflineMode = false])
            // �� ������ �־�� �������̵� �Ἥ Ŀ�����ص� ��.
        }
    }
    // InputField_NickName�� ������ �г����� ������
    public void OnValueChangedNickName(string _nickName)
    {
        nickName = _nickName;
        // ���� �̸� ����
        PhotonNetwork.NickName = nickName;
    }
    // PhotonNetwork.ConnectUsingSettings()�� �ݹ��Լ�
    public override void OnConnectedToMaster()
    {
        Debug.LogFormat("Connected to Master: {0}", nickName);
        // ���ӵ����ϱ� �����ư false
        connectButton.interactable = false;
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("Disconnected: {0}", cause);
        connectButton.interactable = true;

        // ���� �����ϸ� OnJoinedRoom ȣ��
        Debug.Log("Create Room");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayerPerRoom });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
        // �����Ͱ� PhotonNetwork.LoadLevel()�� ȣ���ϸ�,
        // ��� �÷��̾ ������ ������ �ڵ����� �ε�
        // PhotonNetwork.LoadLevel("Room"); // PhotonNetwork.LoadLevel()�� ������ Ŭ���̾�Ʈ������ ȣ��Ǿ�� �ϹǷ� isMasterclient�� �̿��� üũ�Ѵ�.
        // ���⼭�� �����Ͱ� ���ÿ� ������ �����ϰ� �ϴ� ������ �ƴϱ� ������ ���� ���� �θ��� ��.
        SceneManager.LoadScene("Room"); // Scenes in build�� ��ϵ� ���� Load����
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    { // �������� ���԰�, ó���� �� �����ϱ� ����� ���´�.
        Debug.LogErrorFormat("JoinRandomFailed({0}): {1}", returnCode, message);
        Debug.Log("Create Room");
        // random�̶� roomname null
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayerPerRoom }); // createRoom �Ǹ� -> joinRoom 
    }
} // end of class
