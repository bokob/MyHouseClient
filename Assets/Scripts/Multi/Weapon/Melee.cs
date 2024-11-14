using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

/// <summary>
/// �� �� ���� ����
/// </summary>
public class Melee : Weapon
{
    PlayerMove _playerMove;
    PlayerInputs _playerInputs;
    WeaponManager _weaponManager;
    PlayerStatus _playerStatus;

    BoxCollider _meleeArea;       // ���� ���� ����
    TrailRenderer _trailEffet;    // �ֵθ� �� ȿ��
    public Animator _animator;

    [Header("���� ����")]
    bool _isSwingReady;  // ���� �غ�
    float _swingDelay;   // ���� ������
    bool _isStabReady;  // ���� �غ�
    float _stabDelay;   // ���� ������

    PhotonView PV;

    private void Awake()
    {
        InitWeapon();

        _playerMove = transform.root.GetChild(2).GetComponent<PlayerMove>();
        _playerInputs = transform.root.GetChild(2).GetComponent<PlayerInputs>();
        _playerStatus = transform.root.GetChild(2).GetComponent<PlayerStatus>();

        if (_playerStatus.Role == Define.Role.Robber)
            _animator = transform.root.GetChild(2).GetChild(0).gameObject.GetComponent<Animator>();
        else if (_playerStatus.Role == Define.Role.Houseowner)
            _animator = transform.root.GetChild(2).GetChild(1).gameObject.GetComponent<Animator>();
        else if (_playerStatus == null)
            Debug.LogWarning("���� �ִϸ����Ͱ� �� ������");
        else if (_playerStatus.Role == Define.Role.None)
            Debug.Log("�� None�̾�?");

        PV = GetComponent<PhotonView>();
    }

    void InitWeapon()
    {
        base.Type = Define.Type.Melee;

        _meleeArea = gameObject.GetComponent<BoxCollider>();
        _trailEffet = gameObject.GetComponentInChildren<TrailRenderer>();

        // ���� ���� �ʱ�ȭ
        WeaponData weapon = GameManager._instance.GetWeaponStatusByName(transform.name);
        if (weapon != null)
        {
            Debug.Log($"Weapon Name: {weapon.Name}. Attack: {weapon.Attack}, Rate: {weapon.Rate}");
            Attack = weapon.Attack;
            Rate = weapon.Rate;
            Range = weapon.Range;
        }
        else
        {
            Debug.LogWarning("Weapon not found!");
        }
    }

    void Update()
    {
        AttackDelay();
        Use();

        _playerInputs.shoot = false;
        _playerInputs.aim = false;
        _playerInputs.reload = false;
    }

    void AttackDelay()
    {
        _swingDelay += Time.deltaTime;
        _stabDelay += Time.deltaTime;
    }

    /// <summary>
    /// ���� ����: ��Ŭ��(�ֵθ���), ��Ŭ��(���)
    /// ���� ȿ�� �ڷ�ƾ ���� ����ȴ�.
    /// </summary>
    public override void Use()
    {
        _isSwingReady = base.Rate < _swingDelay; // ���ݼӵ��� ���� �����̺��� ������ �����غ� �Ϸ�
        _isStabReady = base.Rate < _stabDelay;

        if (_playerInputs == null) Debug.Log("��");
        if (_playerMove == null) Debug.Log("��");

        if (_playerInputs.swing && _isSwingReady && _playerMove._grounded || _playerInputs.stab && _isStabReady && _playerMove._grounded)
        {
            StopCoroutine("MeleeAttackEffect");

            //// ���� ���Ⱑ �ƴϰų� ���Ⱑ Ȱ��ȭ �Ǿ� ���� ������ ����
            //if (_weaponManager._selectedWeapon.tag != "Melee" || !_weaponManager._selectedWeapon.activeSelf) return;

            // ���ݼӵ��� ���� �����̺��� ������ �����غ� �Ϸ�
            //_isSwingReady = _weaponManager._selectedWeapon.GetComponent<Melee>().Rate < _swingDelay;
            //_isStabReady = _weaponManager._selectedWeapon.GetComponent<Melee>().Rate < _stabDelay;
            if (_playerInputs.swing && _playerMove._grounded) // �ֵθ���
            {
                Debug.Log("�ֵθ���");
                // _weaponManager._selectedWeapon.GetComponent<Melee>().Use();
                //_animator.SetTrigger("setSwing");
                _animator.SetBool("isSwing", true);
                _swingDelay = 0;
            }
            else if (_playerInputs.stab && _playerMove._grounded) // ���
            {
                Debug.Log("���");
                // _weaponManager._selectedWeapon.GetComponent<Melee>().Use();
                //_animator.SetTrigger("setStab");
                _animator.SetBool("isStab", true);
                _stabDelay = 0;

            }
            _playerInputs.swing = false;
            _playerInputs.stab = false;
            StartCoroutine("MeleeAttackEffect");
        }
        else
        {
            // �������ڸ��� �ֵθ��� ���� ����(����Ƽ Play ���� �� Ŭ�� ������ �׷� �� �ϴ�)
            _playerInputs.swing = false;
            _playerInputs.stab = false;
        }
    }

    /// <summary>
    /// �ڷ�ƾ���� Collider, TrailRenderer Ư�� �ð� ���ȸ� Ȱ��ȭ
    /// </summary>
    IEnumerator MeleeAttackEffect()
    {
        yield return new WaitForSeconds(0.5f);
        PV.RPC("SetMeleeArea", RpcTarget.All, true);
        PV.RPC("SetTrailEffect", RpcTarget.All, true);
        PV.RPC("PlayAttackSound", RpcTarget.All);

        yield return new WaitForSeconds(0.5f);
        PV.RPC("SetMeleeArea", RpcTarget.All, false);

        yield return new WaitForSeconds(0.5f);
        PV.RPC("SetTrailEffect", RpcTarget.All, false);
    }

    // punchCollider ���¸� ��� Ŭ���̾�Ʈ���� �����ϴ� ClientRpc �޼���
    [PunRPC]
    void SetMeleeArea(bool state)
    {
        _animator.SetBool("isSwing", false);
        _animator.SetBool("isStab", false);
        _meleeArea.enabled = state;
    }

    // _trailEffect ���¸� ��� Ŭ���̾�Ʈ���� �����ϴ� ClientRpc �޼���
    [PunRPC]
    void SetTrailEffect(bool state)
    { 
        _trailEffet.enabled = state;
    }

    [PunRPC]
    void PlayAttackSound()
    {
        SoundManager._instance.PlayEffectAtPoint("MeleeAttack", transform);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        // �ڱ� �ڽſ��� ���� ��� ����
        if (other.transform.root.name.Contains("Player") && other.transform.root.GetChild(2).GetComponent<PlayerStatus>() == _playerStatus)
        {
            return;
        }

        PlayerStatus otherPlayerStatus = other.GetComponent<PlayerStatus>();
        if (otherPlayerStatus != null)
        {
            PhotonView otherPhotonView = otherPlayerStatus.gameObject.GetComponent<PhotonView>();

            otherPlayerStatus.gameObject.GetComponent<PhotonView>().RPC("TakedDamage", otherPhotonView.Owner, base.Attack);
            otherPlayerStatus.gameObject.GetComponent<PhotonView>().RPC("HitChangeMaterials", RpcTarget.All);
        }

        Monster monster = other.GetComponent<Monster>();
        if(monster != null)
        {
            monster.GetComponent<IStatus>().TakedDamage(Attack);
        }
    }
}