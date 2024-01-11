using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class PlayerCtrl : MonoBehaviourPun
{
    private Rigidbody rb = null;
    [SerializeField] private GameObject bulletPrefab = null;
    [SerializeField] private Color[] colors = null;
    [SerializeField] private float speed = 3.0f;

    private int hp = 3;
    private bool isDead = false;
    private int playerNum = 0;


    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    private void Start()
    {
        isDead = false;
    }

    private void Update()
    {
        // 모든 Player가 PlayerCtrl.cs 가지고 있기 때문에 내 플레이어라는 걸 구분하려면 photonView.IsMine써야한다.
        // 내 플레이어가 아니면 키 컨트롤 안되게끔
        if (!photonView.IsMine) return;
        if (isDead) return;

        if (Input.GetKey(KeyCode.W))
            rb.AddForce(Vector3.forward * speed);
        if (Input.GetKey(KeyCode.S))
            rb.AddForce(Vector3.back * speed);
        if (Input.GetKey(KeyCode.A))
            rb.AddForce(Vector3.left * speed);
        if (Input.GetKey(KeyCode.D))
            rb.AddForce(Vector3.right * speed);

        if (Input.GetMouseButtonDown(0)) ShootBullet();

        // 플레이어가 마우스 커서 쫓아서 바라보도록
        LookAtMouseCursor();
        photonView.RPC("SayThisIsMyColor", RpcTarget.All, playerNum);
    }

    public void SetMaterial(int _playerNum)
    {
        Debug.LogError(_playerNum + " : " + colors.Length);
        if (_playerNum > colors.Length) return;

        photonView.RPC("SayThisIsMyColor", RpcTarget.All, _playerNum);
        playerNum = _playerNum;
    }
    [PunRPC]
    public void SayThisIsMyColor(int _playerNum)
    {
        this.GetComponent<MeshRenderer>().material.color = colors[_playerNum - 1];
    }
    private void ShootBullet()
    {
        if (bulletPrefab)
        {
            GameObject go = PhotonNetwork.Instantiate(
                bulletPrefab.name,
                this.transform.position,
                Quaternion.identity);
            go.GetComponent<Bullet>().Shoot(this.gameObject, this.transform.forward);
        }
    }

    public void LookAtMouseCursor()
    {
        // 마우스커서 포지션을 screen to world하면 Vector2 -> 3 => z는 카메라 찍고있는 젤 앞 위치
        // ㄴ 포지션 위치와  맞지 않기 때문에 playerPos를 screen으로 보내어 쓴다.
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerPos = Camera.main.WorldToScreenPoint(this.transform.position);
        Vector3 dir = mousePos - playerPos;
        // arcTan으로 x축 y축으로 각도 구함. 
        // 근데 return in radian이기 때문에 변환
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        // 현재 플레이어가 transform.up  == Vector3.up 상태이기 때문에 Vector3.up을 사용
        // 정면 z축과 angle을 보정해주기 위해서 90도 더해줌
        this.transform.rotation = Quaternion.AngleAxis(-angle + 90.0f, Vector3.up);
    }

    [PunRPC]
    public void ApplyHp(int _hp)
    {
        hp = _hp; // hp를 player각자 빼주는게 아니라 갱신하는 방식 이용
        Debug.LogErrorFormat("{0} Hp: {1}", PhotonNetwork.NickName, hp);

        if (hp <= 0)
        {
            Debug.LogErrorFormat("Destroy: {0}", PhotonNetwork.NickName);
            isDead = true;
            PhotonNetwork.Destroy(this.gameObject);
        }
    }

    [PunRPC]
    public void OnDamage(int _dmg)
    {
        hp -= _dmg;
        photonView.RPC("ApplyHp", RpcTarget.Others, hp);
    }
}
