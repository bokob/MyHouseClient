using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 한 손 근접 무기
/// </summary>
public class Melee : Weapon
{
    PlayerMove _playerMove;
    PlayerInputs _playerInputs;
    NewWeaponManager _weaponManager;

    BoxCollider _meleeArea;       // 근접 공격 범위
    TrailRenderer _trailEffet;    // 휘두를 때 효과
    Animator _animator;

    [Header("공격 관련")]
    bool _isSwingReady;  // 공격 준비
    float _swingDelay;   // 공격 딜레이
    bool _isStabReady;  // 공격 준비
    float _stabDelay;   // 공격 딜레이

    #region 절단 효과
    public LayerMask _sliceMask; // 자를 대상인 레이어 마스크
    public float _cutForce = 250f; // 자를 때 가해지는 힘

    Vector3 _entryPoint; // 오브젝트에 들어간 지점
    Vector3 _exitPoint; // 오브젝트를 뚫고 나간 지점
    bool _hasExited = false; // 오브젝트를 뚫고 나갔는지 여부를 저장하는 변수
    #endregion

    void Start()
    {
        base.Type = Define.Type.Melee;

        _playerMove = transform.root.GetChild(2).GetComponent<PlayerMove>();
        _playerInputs = transform.root.GetChild(2).GetComponent<PlayerInputs>();
        _animator = base.Master.gameObject.GetComponent<Animator>();

        _meleeArea = gameObject.GetComponent<BoxCollider>();
        _trailEffet = gameObject.GetComponentInChildren<TrailRenderer>();

        // TODO
        /*
         * 무기 능력치를 엑셀이나 json을 이용해 관리 예정
         * 따로 읽어와서 그 값들을 세팅해줘야 함
         * 현재 임시로 테스트를 위해 하드코딩 함
        */
        if (gameObject.tag == "Melee")
            base.Attack = 50;

    }

    void Update()
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
        _swingDelay += Time.deltaTime;
        _stabDelay += Time.deltaTime;
        _isSwingReady = base.Rate < _swingDelay; // 공격속도가 공격 딜레이보다 작으면 공격준비 완료
        _isStabReady = base.Rate < _stabDelay;
        if (_playerInputs.swing && _isSwingReady && _playerMove._grounded || _playerInputs.stap && _isStabReady && _playerMove._grounded)
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
                _animator.SetTrigger("setSwing");
                _swingDelay = 0;
            }
            else if (_playerInputs.stap && _playerMove._grounded) // 찌르기
            {
                Debug.Log("찌르기");
                // _weaponManager._selectedWeapon.GetComponent<Melee>().Use();
                _animator.SetTrigger("setStab");
                _stabDelay = 0;

            }

            _playerInputs.swing = false;
            _playerInputs.stap = false;

            StartCoroutine("MeleeAttackEffect");
        }
        else
        {
            // 시작하자마자 휘두르는 문제 방지(유니티 Play 누를 때 클릭 때문에 그런 듯 하다)
            _playerInputs.swing = false;
            _playerInputs.stap = false;
        }
    }

    /// <summary>
    /// 코루틴으로 Collider, TrailRenderer 특정 시간 동안만 활성화
    /// </summary>
    IEnumerator MeleeAttackEffect()
    {
        yield return new WaitForSeconds(0.5f);
        SetMeleeAreaServerRpc(true);
        SetTrailEffectServerRpc(true);

        yield return new WaitForSeconds(0.5f);
        SetMeleeAreaServerRpc(false);

        yield return new WaitForSeconds(0.5f);
        SetTrailEffectServerRpc(false);
    }

    [ServerRpc]
    void SetMeleeAreaServerRpc(bool state)
    {
        SetMeleeAreaClientRpc(state); // 서버에서 다른 클라이언트들에게 바꾸라고 명령
    }

    // punchCollider 상태를 모든 클라이언트에서 설정하는 ClientRpc 메서드
    [ClientRpc]
    void SetMeleeAreaClientRpc(bool state)
    {
        _meleeArea.enabled = state;
    }

    [ServerRpc]
    void SetTrailEffectServerRpc(bool state)
    {
        SetTrailEffectClientRpc(state); // 서버에서 다른 클라이언트들에게 바꾸라고 명령
    }

    // _trailEffect 상태를 모든 클라이언트에서 설정하는 ClientRpc 메서드
    [ClientRpc]
    void SetTrailEffectClientRpc(bool state)
    {
        _trailEffet.enabled = state;
    }



    // 칼이 트리거 안에 있을 때
    // _hasExited를 false로 설정
    void OnTriggerEnter(Collider other)
    {
        _hasExited = false;
        _entryPoint = other.ClosestPoint(transform.position);

        // 데미지 적용

        // 자기 자신에게 닿은 경우 무시
        if (other.transform.root.name == gameObject.name) return;

        if (other.GetComponent<PlayerStatus>() != null)
        {
            other.GetComponent<PlayerStatus>().TakedDamage(Attack);

            if (other.GetComponent<PlayerStatus>() != null)
            {
                other.GetComponent<PlayerStatus>().HitChangeMaterials();
            }
            if (other.GetComponent<Person>() != null)
            {
                other.GetComponent<Person>().HitChangeMaterials();
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        Debug.Log("관통");
    }

    // 관통 다 되면 레이어에 따라 절단
    void OnTriggerExit(Collider other)
    {
        // 충돌 지점의 방향을 자르는 방향으로 설정
        _exitPoint = other.ClosestPoint(transform.position);

        Vector3 cutDirection = _exitPoint - _entryPoint;
        Vector3 cutInPlane = (_entryPoint + _exitPoint) / 2;

        //Vector3 cutPlaneNormal = Vector3.Cross((entryPoint - exitPoint), (entryPoint - transform.position)).normalized;
        Vector3 cutPlaneNormal = Vector3.Cross((_entryPoint - _exitPoint), (_entryPoint - transform.position)).normalized;
        Debug.Log(cutPlaneNormal.x + ", " + cutPlaneNormal.y + ", " + cutPlaneNormal.z);

        if (cutPlaneNormal.x == 0 && cutPlaneNormal.y == 0 && cutPlaneNormal.z == 0)
        {
            // 원래 자르던 방향을 normalize 해서 넣어줘야 됨
            cutPlaneNormal = (_entryPoint - _exitPoint).normalized;
            Debug.Log("대체: " + cutPlaneNormal.x + " " + cutPlaneNormal.y + " " + cutPlaneNormal.z);

            bool isHorizontalCut = Mathf.Abs(cutDirection.x) > Mathf.Abs(cutDirection.y);

            // 가로로 자르는 경우
            if (isHorizontalCut)
            {
                // x 축 방향으로 자르기 때문에 cutPlaneNormal을 x 축 방향 벡터로 설정
                cutPlaneNormal = Vector3.up;
            }
            else // 세로로 자르는 경우
            {
                // y 축 방향으로 자르기 때문에 cutPlaneNormal을 y 축 방향 벡터로 설정
                cutPlaneNormal = Vector3.right;
            }
        }

        LayerMask cutableMask = LayerMask.GetMask(LayerMask.LayerToName(other.gameObject.layer));
        //Debug.Log("잘릴 레이어: " + LayerMask.LayerToName(other.gameObject.layer));
        if (_sliceMask.value == cutableMask)
        {
            Debug.LogWarning("자를 수 있는 오브젝트");
            // 오브젝트를 자르기
            Cutter.Cut(other.gameObject, cutInPlane, cutPlaneNormal);

            // 자를 때 가해지는 힘을 적용하여 오브젝트를 밀어냄
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(-cutPlaneNormal * _cutForce); // cutDirection 대신에 cutPlaneNormal을 사용
            }

            _hasExited = true;
        }
        else
        {
            //Debug.Log("sliceMask: " + sliceMask.value);
            //Debug.Log("자를 레이어: " + other.gameObject.layer);
            Debug.LogWarning("왜 안돼?");
        }
    }
}