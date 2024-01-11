using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class PhotonLauncher : MonoBehaviourPunCallbacks
{ // 포톤 접속 & 입장하기 위해서 짜여진 class PhotonLauncher
    [SerializeField] private string gameVersion = "0.0.1";
    [SerializeField] private byte maxPlayerPerRoom = 4;
    [SerializeField] private string nickName = string.Empty;
    [SerializeField] private Button connectButton = null;
    /*
    # 포톤 네트워크에는 서버 로비 룸 개념 존재
     ㄴ 서버 접속 -> 로비에 모여 있다가 룸에서 겜시작함.
     ㄴ 현재 이 게임은 로비 개념은 뺀 것
     */

    private void Awake()
    {
        // 방 만든사람이 마스터가 되는데, 마스터가 씬 부르면 플레이어 동시에 씬 띄워줌
        // 씬 동기화 시켜주려면 이것이 켜져야 한다.
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        connectButton.interactable = true;
    }

    // ConnectButton이 눌러지면 호출
    public void Connect()
    {
        if (string.IsNullOrEmpty(nickName))
        {
            Debug.Log("NickName is empty");
            return;
        }

        if (PhotonNetwork.IsConnected) // 내가 서버에 접속된 상태인지 체크
        {
            PhotonNetwork.JoinRandomRoom(); // 내가 들어갈 수 있는 방을 랜덤으로 하나 찾는다.
            //PhotonNetwork.JoinRoom()
            //PhotonNetwork.LeaveLobby()
        }
        else
        { // 처음에는 접속이 안되어 있으니까 무조건 여기로 들어온다.
            Debug.LogFormat("Connect : {0}", gameVersion);
            PhotonNetwork.GameVersion = gameVersion; // PhotonNetwork 버전 같아야 같은 게임에 접속할 수 있다.
            //PhotonNetwork.GameVersion = Application.version; // 따로 게임버전 관리 안할거면 유니티 프로젝트 버전 넣어 사용하면 된다.

            // 포톤 클라우드에 접속을 시작하는 지점
            // 접속에 성공하면 OnConnectedToMaster 메서드 호출(callback)
            PhotonNetwork.ConnectUsingSettings();// 포톤네트워크에 설정된 기본 값으로 접속을 하겠다.
            //PhotonNetwork.ConnectUsingSettings(AppSettings appSettings, [bool startInOfflineMode = false])
            // ㄴ 설정값 넣어쓰는 오버라이드 써서 커스텀해도 됨.
        }
    }
    // InputField_NickName과 연결해 닉네임을 가져옴
    public void OnValueChangedNickName(string _nickName)
    {
        nickName = _nickName;
        // 유저 이름 지정
        PhotonNetwork.NickName = nickName;
    }
    // PhotonNetwork.ConnectUsingSettings()의 콜백함수
    public override void OnConnectedToMaster()
    {
        Debug.LogFormat("Connected to Master: {0}", nickName);
        // 접속됐으니까 연결버튼 false
        connectButton.interactable = false;
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("Disconnected: {0}", cause);
        connectButton.interactable = true;

        // 방을 생성하면 OnJoinedRoom 호출
        Debug.Log("Create Room");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayerPerRoom });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
        // 마스터가 PhotonNetwork.LoadLevel()을 호출하면,
        // 모든 플레이어가 동일한 레벨을 자동으로 로드
        // PhotonNetwork.LoadLevel("Room"); // PhotonNetwork.LoadLevel()은 마스터 클라이언트에서만 호출되어야 하므로 isMasterclient를 이용해 체크한다.
        // 여기서는 마스터가 동시에 게임을 시작하게 하는 구조가 아니기 때문에 각자 씬을 부르면 됨.
        SceneManager.LoadScene("Room"); // Scenes in build에 등록된 씬만 Load가능
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    { // 서버에는 들어왔고, 처음엔 방 없으니까 여기로 들어온다.
        Debug.LogErrorFormat("JoinRandomFailed({0}): {1}", returnCode, message);
        Debug.Log("Create Room");
        // random이라서 roomname null
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayerPerRoom }); // createRoom 되면 -> joinRoom 
    }
} // end of class
