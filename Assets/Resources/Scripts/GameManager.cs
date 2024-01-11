using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab = null;
    private List<GameObject> playerGoList = new List<GameObject>();
    private void Start()
    {
        CreatePlayer();

    } // end of Start()
    private void CreatePlayer()
    {
        if (playerPrefab != null)
        {
            // PhotonNetwork의 Instantiate를 쓰면 모든 client에게 생성된 프리팹이 보여지게 된다.
            // 생성할 때 내꺼만 만드는 데, 이전에 생성된 것을 기억해놓고 새로운 애 들어오면 기존에 있던것 들 전부 그대로 생성해준다는 것
            // client 1번 기준으로 player1은 본인
            // client 2번 기준으로 palyer1은 본인 x. Player2가 본인. player1을 기억해놓고 현재 따로 instantiate안해도 만들어 줌.
            // ㄴ 내가 실제로 조작하는 것은 client1 일경우 player1인데,
            // ㄴ 내가 관리하는 게 player1이라는 것을 알 방법은 없어서 로직 추가해 줘야 한다.
            // player script하나로 내가 조작하는 애 vs 안하는 애 동시에 조작해야 한다.
            // ㄴ (PhotonNetwork.Instantiate로 만드는 것은 결국 똑같은 Player이기 때문에 구분해 줘야한다.)

            GameObject go = PhotonNetwork.Instantiate(
                playerPrefab.name, // prefab의 이름을 던져준다. "P_player"라고 적어도 된다.
                new Vector3( // 모든 클라이언트들이 같은 위치에 만들어 진다.
                    Random.Range(-10.0f, 10.0f),
                    0.0f,
                    Random.Range(-10.0f, 10.0f)),
                Quaternion.identity,
                0);
            PlayerCtrl goPlayerCtrl = go.GetComponent<PlayerCtrl>();
            goPlayerCtrl.SetMaterial(PhotonNetwork.CurrentRoom.PlayerCount); // 생성된 player의 color를 배열 순서대로 부여
            //playerColors[PhotonNetwork.CurrentRoom.PlayerCount - 1] = PhotonNetwork.CurrentRoom.PlayerCount;
        }
    }
    // PhotonNetwork.LeaveRoom 함수가 호출되면 호출
    public override void OnLeftRoom()
    {
        Debug.Log("Left Room");
        SceneManager.LoadScene("Launcher");
    }

    // 어떤 플레이어가 입장할 때 호출되는 함수 
    public override void OnPlayerEnteredRoom(Player otherPlayer) // 들어온 player의 정보이지, 객체 정보가 아님
    {
        Debug.LogFormat("Player Entered Room: {0}",
                        otherPlayer.NickName);
        // # RPC : Remote Procedure Call  - 원격으로 함수를 호출
        // 누가 들어왔는지 모든 클라이언트가 통지받음
     //   photonView.RPC("RPCApplyPlayerList", RpcTarget.All); // 목록 갱신 함수 "ApplyPlayerList" 호출
        // ㄴ # photonView - 포톤 상태를 관찰하고 있는 컴포넌트. getComponent하지 않아도 자동으로 들어온다.
    }
    // 플레이어가 나갈 때 호출되는 함수
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogFormat("Player Left Room: {0}",
                        otherPlayer.NickName);

    }
    [PunRPC] // 이걸 달아야 RPC로 원격 호출 가능해 진다.
    public void RPCApplyPlayerList()
    {
        int playerCnt = PhotonNetwork.CurrentRoom.PlayerCount;
        // 플레이어 리스트가 최신이라면 건너뜀
        if (playerCnt == playerGoList.Count) return;

        // 현재 방에 접속해 있는 플레이어의 수
        Debug.LogError("CurrentRoom PlayerCount : " + playerCnt);

        // 현재 생성되어 있는 모든 포톤뷰 가져오기
        PhotonView[] photonViews = FindObjectsOfType<PhotonView>();

        // 매번 재정렬을 하는게 좋으므로 플레이어 게임오브젝트 리스트를 초기화
        playerGoList.Clear(); // 새로 들어올때마다 목록을 다시 만들어 주기 때문에 매번 Clear하고 사용
        //System.Array.Clear(playerGoList, 0, playerGoList.Length); // 배열 리셋

        // 현재 생성되어 있는 포톤뷰 전체와
        // 접속중인 플레이어들의 액터넘버를 비교해,
        // 플레이어 게임오브젝트 리스트에 추가
        for (int i = 0; i < playerCnt; ++i)
        {
            // 키는 0이 아닌 1부터 시작
            // 포톤에서 뭘 만들면 딕셔너리 구조로 키값이 할당되는데, 그 키 값은 1부터 시작되므로 여기서도 key를 1로 초기화 했다.
            int key = i + 1;
            for (int j = 0; j < photonViews.Length; ++j)
            {
                // 만약 PhotonNetwork.Instantiate를 통해서 생성된 포톤뷰가 아니라면 넘김
                if (photonViews[j].isRuntimeInstantiated == false) continue;
                // 만약 현재 키 값이 딕셔너리 내에 존재하지 않는다면 넘김
                if (PhotonNetwork.CurrentRoom.Players.ContainsKey(key) == false) continue;


                // 포톤뷰의 액터넘버
                int viewNum = photonViews[j].Owner.ActorNumber;// 포톤뷰 가진애들은 actorNumber 가진다.
                // 접속중인 플레이어의 액터넘버 // 서버가 가지고 있음. OnPlayerEntertedRoom 에서 전달받은 otherPlayer에 정보 댐김
                int playerNum = PhotonNetwork.CurrentRoom.Players[key].ActorNumber;

                // 액터넘버가 같은 오브젝트가 있다면,
                if (viewNum == playerNum)
                {
                    // 게임오브젝트 이름도 알아보기 쉽게 변경
                    photonViews[j].gameObject.name = "Player_" + photonViews[j].Owner.NickName;
                    // 실제 게임오브젝트를 리스트에 추가
                    playerGoList.Add(photonViews[j].gameObject);
                }
            }
        }
        // 디버그용
        PrintPlayerList();  
    } // end of ApplyPlayerList()

    private void PrintPlayerList()
    {
        foreach (GameObject go in playerGoList)
        {
            if (go != null)
            {
                // 빌드에서 실행하면 에러로그로 띄우기 위해서
                Debug.LogError(go.name);
            }
        }
    }

} // end of class
