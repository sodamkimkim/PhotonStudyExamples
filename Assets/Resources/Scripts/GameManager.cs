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
            // PhotonNetwork�� Instantiate�� ���� ��� client���� ������ �������� �������� �ȴ�.
            // ������ �� ������ ����� ��, ������ ������ ���� ����س��� ���ο� �� ������ ������ �ִ��� �� ���� �״�� �������شٴ� ��
            // client 1�� �������� player1�� ����
            // client 2�� �������� palyer1�� ���� x. Player2�� ����. player1�� ����س��� ���� ���� instantiate���ص� ����� ��.
            // �� ���� ������ �����ϴ� ���� client1 �ϰ�� player1�ε�,
            // �� ���� �����ϴ� �� player1�̶�� ���� �� ����� ��� ���� �߰��� ��� �Ѵ�.
            // player script�ϳ��� ���� �����ϴ� �� vs ���ϴ� �� ���ÿ� �����ؾ� �Ѵ�.
            // �� (PhotonNetwork.Instantiate�� ����� ���� �ᱹ �Ȱ��� Player�̱� ������ ������ ����Ѵ�.)

            GameObject go = PhotonNetwork.Instantiate(
                playerPrefab.name, // prefab�� �̸��� �����ش�. "P_player"��� ��� �ȴ�.
                new Vector3( // ��� Ŭ���̾�Ʈ���� ���� ��ġ�� ����� ����.
                    Random.Range(-10.0f, 10.0f),
                    0.0f,
                    Random.Range(-10.0f, 10.0f)),
                Quaternion.identity,
                0);
            PlayerCtrl goPlayerCtrl = go.GetComponent<PlayerCtrl>();
            goPlayerCtrl.SetMaterial(PhotonNetwork.CurrentRoom.PlayerCount); // ������ player�� color�� �迭 ������� �ο�
            //playerColors[PhotonNetwork.CurrentRoom.PlayerCount - 1] = PhotonNetwork.CurrentRoom.PlayerCount;
        }
    }
    // PhotonNetwork.LeaveRoom �Լ��� ȣ��Ǹ� ȣ��
    public override void OnLeftRoom()
    {
        Debug.Log("Left Room");
        SceneManager.LoadScene("Launcher");
    }

    // � �÷��̾ ������ �� ȣ��Ǵ� �Լ� 
    public override void OnPlayerEnteredRoom(Player otherPlayer) // ���� player�� ��������, ��ü ������ �ƴ�
    {
        Debug.LogFormat("Player Entered Room: {0}",
                        otherPlayer.NickName);
        // # RPC : Remote Procedure Call  - �������� �Լ��� ȣ��
        // ���� ���Դ��� ��� Ŭ���̾�Ʈ�� ��������
     //   photonView.RPC("RPCApplyPlayerList", RpcTarget.All); // ��� ���� �Լ� "ApplyPlayerList" ȣ��
        // �� # photonView - ���� ���¸� �����ϰ� �ִ� ������Ʈ. getComponent���� �ʾƵ� �ڵ����� ���´�.
    }
    // �÷��̾ ���� �� ȣ��Ǵ� �Լ�
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogFormat("Player Left Room: {0}",
                        otherPlayer.NickName);

    }
    [PunRPC] // �̰� �޾ƾ� RPC�� ���� ȣ�� ������ ����.
    public void RPCApplyPlayerList()
    {
        int playerCnt = PhotonNetwork.CurrentRoom.PlayerCount;
        // �÷��̾� ����Ʈ�� �ֽ��̶�� �ǳʶ�
        if (playerCnt == playerGoList.Count) return;

        // ���� �濡 ������ �ִ� �÷��̾��� ��
        Debug.LogError("CurrentRoom PlayerCount : " + playerCnt);

        // ���� �����Ǿ� �ִ� ��� ����� ��������
        PhotonView[] photonViews = FindObjectsOfType<PhotonView>();

        // �Ź� �������� �ϴ°� �����Ƿ� �÷��̾� ���ӿ�����Ʈ ����Ʈ�� �ʱ�ȭ
        playerGoList.Clear(); // ���� ���ö����� ����� �ٽ� ����� �ֱ� ������ �Ź� Clear�ϰ� ���
        //System.Array.Clear(playerGoList, 0, playerGoList.Length); // �迭 ����

        // ���� �����Ǿ� �ִ� ����� ��ü��
        // �������� �÷��̾���� ���ͳѹ��� ����,
        // �÷��̾� ���ӿ�����Ʈ ����Ʈ�� �߰�
        for (int i = 0; i < playerCnt; ++i)
        {
            // Ű�� 0�� �ƴ� 1���� ����
            // ���濡�� �� ����� ��ųʸ� ������ Ű���� �Ҵ�Ǵµ�, �� Ű ���� 1���� ���۵ǹǷ� ���⼭�� key�� 1�� �ʱ�ȭ �ߴ�.
            int key = i + 1;
            for (int j = 0; j < photonViews.Length; ++j)
            {
                // ���� PhotonNetwork.Instantiate�� ���ؼ� ������ ����䰡 �ƴ϶�� �ѱ�
                if (photonViews[j].isRuntimeInstantiated == false) continue;
                // ���� ���� Ű ���� ��ųʸ� ���� �������� �ʴ´ٸ� �ѱ�
                if (PhotonNetwork.CurrentRoom.Players.ContainsKey(key) == false) continue;


                // ������� ���ͳѹ�
                int viewNum = photonViews[j].Owner.ActorNumber;// ����� �����ֵ��� actorNumber ������.
                // �������� �÷��̾��� ���ͳѹ� // ������ ������ ����. OnPlayerEntertedRoom ���� ���޹��� otherPlayer�� ���� ���
                int playerNum = PhotonNetwork.CurrentRoom.Players[key].ActorNumber;

                // ���ͳѹ��� ���� ������Ʈ�� �ִٸ�,
                if (viewNum == playerNum)
                {
                    // ���ӿ�����Ʈ �̸��� �˾ƺ��� ���� ����
                    photonViews[j].gameObject.name = "Player_" + photonViews[j].Owner.NickName;
                    // ���� ���ӿ�����Ʈ�� ����Ʈ�� �߰�
                    playerGoList.Add(photonViews[j].gameObject);
                }
            }
        }
        // ����׿�
        PrintPlayerList();  
    } // end of ApplyPlayerList()

    private void PrintPlayerList()
    {
        foreach (GameObject go in playerGoList)
        {
            if (go != null)
            {
                // ���忡�� �����ϸ� �����α׷� ���� ���ؼ�
                Debug.LogError(go.name);
            }
        }
    }

} // end of class
