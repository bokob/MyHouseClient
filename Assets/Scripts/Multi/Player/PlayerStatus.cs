using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;

public class PlayerStatus : MonoBehaviourPunCallbacks
{
    public PlayerMove _playerMove;
    public CameraController _cameraController;
    public InGameUI _inGameUI;

    #region ���� �� �ɷ�ġ, �̸� ��
    public bool _isLocalPlayer;
    public string _nickName;
    [field: SerializeField] public Define.Role Role = Define.Role.None;
    [field: SerializeField] public float Hp { get; set; } = 100;    // ü��
    [field: SerializeField] public float Sp { get; set; } = 100;    // ���׹̳�
    [field: SerializeField] public float MaxHp { get; private set; } = 100; // �ִ� ü��
    [field: SerializeField] public float MaxSp { get; private set; } = 100; // �ִ� ���׹̳�
    [field: SerializeField] public float Defence { get; private set; } = 1; // ����
    #endregion

    #region �ִϸ��̼� �� ����
    public Animator _animator;
    List<Renderer> _renderers;
    #endregion

    public Transform _weaponHolder;
    public Transform[] _weaponHolders;
    public WeaponManager _weaponManager;
    public GameObject nearMeleeObject;
    private string meleeItemName;
    public bool _isPickUp = false;

    public GameObject _smokeEffect;

    //public string nickname;

    //public TextMeshPro nicknameText;

    //public Transform TPWeaponHolder;

    public void IsLocalPlayer()
    {
        //TPWeaponHolder.gameObject.SetActive(false);

        _isLocalPlayer = true;
        _playerMove.enabled = true;         // PlayerMove Ȱ��ȭ
        _cameraController.gameObject.transform.parent.gameObject.SetActive(true);
        _inGameUI.gameObject.SetActive(true);
        transform.GetChild(0).gameObject.GetComponent<FadeObjectBlockingObject>().enabled = true; // ���� ���� ����ȭ Ȱ��ȭ
    }

    [PunRPC]
    public void SetNickname(string _name)
    {
        _nickName = _name;
    }

    [PunRPC]
    public void SetRole(Define.Role role) // ���� ����
    {
        // ������ ���� �Ҵ�
        Debug.Log($"�� ����({_nickName}): " + Role);

        Role = role;

        if (Role == Define.Role.Robber)
            _animator = transform.GetChild(0).GetComponent<Animator>();
        else if (Role == Define.Role.Houseowner)
            _animator = transform.GetChild(1).GetComponent<Animator>();
    }

    /// <summary>
    /// ���
    /// </summary>
    [PunRPC]
    public void Dead()
    {
        if (Role != Define.Role.None && Hp <= 0)
        {
            _animator.SetTrigger("setDie");
            Role = Define.Role.None; // ��ü
            // StartCoroutine(DeadSinkCoroutine());
            StartCoroutine(TombCoroutine());
        }
    }

    void Awake()
    {
        // ���� ��������
        _renderers = new List<Renderer>();
        Transform[] underTransforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < underTransforms.Length; i++)
        {
            Renderer renderer = underTransforms[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                _renderers.Add(renderer);
                // if (renderer.material.color == null) Debug.Log("�� ���� ��?");
            }
        }
    }

    void Update()
    {
        //if (!IsLocalPlayer) return;
        //Dead();

        if (Input.GetKeyDown(KeyCode.E) && nearMeleeObject != null && _weaponManager._selectedWeapon.tag != "Gun")
        {
            _isPickUp = true;
            GetMeleeItem();
        if(Input.GetKeyDown(KeyCode.Y))
        {
            _animator.SetTrigger("setDie");
            Role = Define.Role.None; // ��ü
            // StartCoroutine(DeadSinkCoroutine());
            StartCoroutine(TombCoroutine());
        }
    }

    /// <summary>
    /// ������ �Ա�
    /// </summary>
    /// <param name="attack"> ���� ���ݷ� </param>
    [PunRPC]
    public void TakedDamage(int attack, PhotonMessageInfo info)
    {
        // ���ذ� ������� ȸ���Ǵ� ������ �Ͼ�Ƿ� ������ ���� 0�̻����� �ǰԲ� ����
        float damage = Mathf.Max(0, attack - Defence);
        Hp -= damage;
        Debug.Log(gameObject.name + "(��)�� " + damage + " ��ŭ ���ظ� �Ծ���!");
        Debug.Log("���� ü��: " + Hp);

        if(Hp <= 0)
        {
            Player killer = info.Sender;

            GameManager._instance.OnPlayerKilled(photonView.Owner, killer);

            GetComponent<PhotonView>().RPC("Dead", RpcTarget.AllBuffered);
        }

    }


    #region ü�� �� ���׹̳� ����
    /// <summary>
    /// �ִ� ü���� 0.2��ŭ ȸ��
    /// </summary>
    public void Heal()
    {
        // ���� ü���� �ִ� ü�º��� ���� ���� ȸ�� ����
        if (Hp < MaxHp)
        {
            // ȸ����
            float healAmount = MaxHp * 0.2f;

            // ȸ������ ���� ü�°��� ���� �ִ� ü���� ���� �ʵ��� ����
            float healedAmount = Mathf.Clamp(Hp + healAmount, 0, MaxHp) - Hp;

            Debug.Log("���� ü��" + Hp);
            // ü�� ȸ��
            Hp += healedAmount;
            Debug.Log("ü���� " + healedAmount + "��ŭ ȸ��!");
            Debug.Log("���� ü��: " + Hp);
        }
        else
        {
            Debug.Log("�ִ� ü��. ȸ���� �ʿ� ����.");
        }
    }

    /// <summary>
    /// �ִ� ���׹̳����� ���� ȸ��
    /// </summary>
    public void SpUp()
    {
        // ���� ���׹̳��� �ִ� ���׹̳����� ���� ���� ȸ�� ����
        if (Sp < MaxSp)
        {
            // ȸ������ ���� ���׹̳����� ���� �ִ� ���׹̳��� ���� �ʵ��� ����
            float healedAmount = Mathf.Clamp(Sp + MaxSp, 0, MaxHp) - Sp;

            Debug.Log("���� ���׹̳�" + Sp);
            // ���׹̳� ȸ��
            Sp += healedAmount;
            Debug.Log("���� ȸ��! ���� Sp: " + Sp);
        }
        else
        {
            Debug.Log("�ִ� Sp. ȸ���� �ʿ� ����.");
        }
    }

    /// <summary>
    /// ���׹̳� ��������
    /// </summary>
    public void ChargeSp()
    {
        Sp += Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    /// <summary>
    /// ���׹̳� ���̱�
    /// </summary>
    public void DischargeSp()
    {
        Sp -= Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    /// <summary>
    /// ������, ���׹̳� ����
    /// </summary>
    public void JumpSpDown() => Sp -= 3;

    /// <summary>
    /// ���� ����
    /// </summary>
    public void DefenceUp()
    {

    }
    #endregion


    /// <summary>
    /// ���������� ����
    /// </summary>
    [PunRPC]
    public void TransformIntoRobber()
    {
        transform.GetChild(0).gameObject.SetActive(true); // ���� Ȱ��ȭ
        transform.GetChild(1).gameObject.SetActive(false);  // ������ ��Ȱ��ȭ

        Debug.Log("���� ����: " + Role);

        _cameraController.gameObject.GetComponent<CameraController>().SetRobberView(); // ���� �������� ����

        Debug.Log(gameObject.GetComponent<PlayerStatus>()._nickName + "(��)�� ������ ���� �Ϸ�");
    }


    /// <summary>
    /// ���������� ����
    /// </summary>
    [PunRPC]
    public void TransformIntoHouseowner()
    {
        transform.GetChild(0).gameObject.SetActive(false); // ���� ��Ȱ��ȭ
        transform.GetChild(1).gameObject.SetActive(true);  // ������ Ȱ��ȭ

        _inGameUI.gameObject.transform.GetChild(4).gameObject.SetActive(true); // ������ Ȱ��ȭ

        transform.GetChild(0).gameObject.GetComponent<FadeObjectBlockingObject>().enabled = false; // ���� ���� ����ȭ ��Ȱ��ȭ

        Debug.Log($"���� ����({transform.root.GetChild(2).GetComponent<PlayerStatus>()._nickName}): " + Role);

        _cameraController.gameObject.GetComponent<CameraController>().SetHouseownerView(); // ������ �������� ����

        Debug.Log(gameObject.GetComponent<PlayerStatus>()._nickName + "(��)�� ���������� ���� �Ϸ�");
    }

    /// <summary>
    /// ��ü �ٴ����� ����ɱ�
    /// </summary>
    /// <returns></returns>
    // IEnumerator DeadSinkCoroutine()
    // {
    //     GetComponent<CharacterController>().enabled = false;
    //     GetComponent<PlayerInput>().enabled = false;
    //     yield return new WaitForSeconds(3f);
    //     while (transform.position.y > -5f)
    //     {
    //         transform.Translate(Vector3.down * 0.1f * Time.deltaTime);
    //         yield return null;
    //     }
    //     // Destroy(gameObject);

    //     if(GetComponent<PhotonView>().IsMine)
    //     {
    //         Application.Quit();
    //     }

    //     //Application.Quit(); // ���� ����
    //     Debug.Log("���� ���� ����");
    // }

    IEnumerator TombCoroutine()
    {
        GetComponent<CharacterController>().enabled = false;
        GetComponent<PlayerInput>().enabled = false;
        yield return new WaitForSeconds(2f);
        transform.GetChild(0).gameObject.SetActive(false); // ���� ��Ȱ��ȭ
        transform.GetChild(1).gameObject.SetActive(false);  // ������ ��Ȱ��ȭ
        transform.GetChild(3).gameObject.SetActive(true); // Tombstone Active
        yield return new WaitForSeconds(3f);
        if(GetComponent<PhotonView>().IsMine)
        {
            Application.Quit();
        }
        Debug.Log("���� ���� ����");
    }

    /// <summary>
    /// ���� ������ Material �Ӱ� ��ȭ
    /// </summary>
    [PunRPC]
    public void HitChangeMaterials()
    {
        // �±װ� ���� �Ǵ� ����

        for (int i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].material.color = Color.red;
            Debug.Log("�����Ѵ�.");
            Debug.Log(_renderers[i].material.name);
        }

        StartCoroutine(ResetMaterialAfterDelay(1.7f));

        //Debug.Log($"�÷��̾ {other.transform.root.name}���� ���� ����!");
        Debug.Log("���ݹ��� ���� ü��:" + Hp);
    }

    /// <summary>
    /// ���� �ް� Material ������� ����
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator ResetMaterialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        for (int i = 0; i < _renderers.Count; i++)
            _renderers[i].material.color = Color.white;
    }

    public void GetMeleeItem()
    {
        meleeItemName = nearMeleeObject.name;
        _weaponManager.PickUpWeapon(meleeItemName);
        //Destroy(nearMeleeObject);
    }


    public void ChangeIsHoldGun(bool isHoldGun)
    {
        if (Role != Define.Role.Houseowner) return;
        _animator.SetBool("isHoldGun", isHoldGun);
    }

    [PunRPC]
    void SmokeEffect(Vector3 position)
    {
        PhotonNetwork.Instantiate("SmokeParticlePrefab", position, Quaternion.identity);
    }
}