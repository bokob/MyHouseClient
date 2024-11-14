using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

/// <summary>
/// 한 손 근접 무기
/// </summary>
public class Melee : Weapon
{
    PlayerMove _playerMove;
    PlayerInputs _playerInputs;
    WeaponManager _weaponManager;
    PlayerStatus _playerStatus;

    BoxCollider _meleeArea;       // 근접 공격 범위
    TrailRenderer _trailEffet;    // 휘두를 때 효과
    public Animator _animator;

    [Header("공격 관련")]
    bool _isSwingReady;  // 공격 준비
    float _swingDelay;   // 공격 딜레이
    bool _isStabReady;  // 공격 준비
    float _stabDelay;   // 공격 딜레이

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
            Debug.LogWarning("무기 애니메이터가 왜 널이지");
        else if (_playerStatus.Role == Define.Role.None)
            Debug.Log("왜 None이야?");

        PV = GetComponent<PhotonView>();
    }

    void InitWeapon()
    {
        base.Type = Define.Type.Melee;

        _meleeArea = gameObject.GetComponent<BoxCollider>();
        _trailEffet = gameObject.GetComponentInChildren<TrailRenderer>();

        // 무기 스탯 초기화
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
    /// 근접 공격: 좌클릭(휘두르기), 우클릭(찌르기)
    /// 공격 효과 코루틴 같이 실행된다.
    /// </summary>
    public override void Use()
    {
        _isSwingReady = base.Rate < _swingDelay; // 공격속도가 공격 딜레이보다 작으면 공격준비 완료
        _isStabReady = base.Rate < _stabDelay;

        if (_playerInputs == null) Debug.Log("널");
        if (_playerMove == null) Debug.Log("널");

        if (_playerInputs.swing && _isSwingReady && _playerMove._grounded || _playerInputs.stab && _isStabReady && _playerMove._grounded)
        {
            StopCoroutine("MeleeAttackEffect");

            //// 근접 무기가 아니거나 무기가 활성화 되어 있지 않으면 종료
            //if (_weaponManager._selectedWeapon.tag != "Melee" || !_weaponManager._selectedWeapon.activeSelf) return;

            // 공격속도가 공격 딜레이보다 작으면 공격준비 완료
            //_isSwingReady = _weaponManager._selectedWeapon.GetComponent<Melee>().Rate < _swingDelay;
            //_isStabReady = _weaponManager._selectedWeapon.GetComponent<Melee>().Rate < _stabDelay;
            if (_playerInputs.swing && _playerMove._grounded) // 휘두르기
            {
                Debug.Log("휘두르기");
                // _weaponManager._selectedWeapon.GetComponent<Melee>().Use();
                //_animator.SetTrigger("setSwing");
                _animator.SetBool("isSwing", true);
                _swingDelay = 0;
            }
            else if (_playerInputs.stab && _playerMove._grounded) // 찌르기
            {
                Debug.Log("찌르기");
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
            // 시작하자마자 휘두르는 문제 방지(유니티 Play 누를 때 클릭 때문에 그런 듯 하다)
            _playerInputs.swing = false;
            _playerInputs.stab = false;
        }
    }

    /// <summary>
    /// 코루틴으로 Collider, TrailRenderer 특정 시간 동안만 활성화
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

    // punchCollider 상태를 모든 클라이언트에서 설정하는 ClientRpc 메서드
    [PunRPC]
    void SetMeleeArea(bool state)
    {
        _animator.SetBool("isSwing", false);
        _animator.SetBool("isStab", false);
        _meleeArea.enabled = state;
    }

    // _trailEffect 상태를 모든 클라이언트에서 설정하는 ClientRpc 메서드
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

        // 자기 자신에게 닿은 경우 무시
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