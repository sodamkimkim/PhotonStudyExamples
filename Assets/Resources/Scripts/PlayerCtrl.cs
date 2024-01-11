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
        // ��� Player�� PlayerCtrl.cs ������ �ֱ� ������ �� �÷��̾��� �� �����Ϸ��� photonView.IsMine����Ѵ�.
        // �� �÷��̾ �ƴϸ� Ű ��Ʈ�� �ȵǰԲ�
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

        // �÷��̾ ���콺 Ŀ�� �ѾƼ� �ٶ󺸵���
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
        // ���콺Ŀ�� �������� screen to world�ϸ� Vector2 -> 3 => z�� ī�޶� ����ִ� �� �� ��ġ
        // �� ������ ��ġ��  ���� �ʱ� ������ playerPos�� screen���� ������ ����.
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerPos = Camera.main.WorldToScreenPoint(this.transform.position);
        Vector3 dir = mousePos - playerPos;
        // arcTan���� x�� y������ ���� ����. 
        // �ٵ� return in radian�̱� ������ ��ȯ
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        // ���� �÷��̾ transform.up  == Vector3.up �����̱� ������ Vector3.up�� ���
        // ���� z��� angle�� �������ֱ� ���ؼ� 90�� ������
        this.transform.rotation = Quaternion.AngleAxis(-angle + 90.0f, Vector3.up);
    }

    [PunRPC]
    public void ApplyHp(int _hp)
    {
        hp = _hp; // hp�� player���� ���ִ°� �ƴ϶� �����ϴ� ��� �̿�
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
