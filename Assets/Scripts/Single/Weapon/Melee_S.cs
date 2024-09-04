using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee_S : Weapon
{
    PlayerMove_S _playerMove;
    PlayerInputs _playerInputs;

    BoxCollider _meleeArea;       // 근접 공격 범위
    TrailRenderer _trailEffet;    // 휘두를 때 효과
    Animator _animator;

    [Header("공격 관련")]
    bool _isSwingReady;  // 공격 준비
    float _swingDelay;   // 공격 딜레이
    bool _isStabReady;  // 공격 준비
    float _stabDelay;   // 공격 딜레이
    [SerializeField] bool isAttack = false;

    #region 절단 효과 변수
    public LayerMask _sliceMask; // 자를 대상인 레이어 마스크
    public float _cutForce = 250f; // 자를 때 가해지는 힘

    Vector3 _entryPoint; // 오브젝트에 들어간 지점
    Vector3 _exitPoint; // 오브젝트를 뚫고 나간 지점
    bool _hasExited = false; // 오브젝트를 뚫고 나갔는지 여부를 저장하는 변수 (디버깅 용)
    #endregion

    void Awake()
    {
        InitWeapon();
    }

    void InitWeapon()
    {
        base.Type = Define.Type.Melee;

        _meleeArea = gameObject.GetComponent<BoxCollider>();
        _trailEffet = gameObject.GetComponentInChildren<TrailRenderer>();

        // 무기 스탯 초기화
        WeaponData weapon = GameManager_S._instance.GetWeaponStatusByName(transform.name);
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


    void Start()
    {
        _playerMove = transform.root.GetChild(2).GetComponent<PlayerMove_S>();
        _playerInputs = transform.root.GetChild(2).GetComponent<PlayerInputs>();

        if (transform.root.childCount > 2) // 싱글 전용
        {
            _animator = transform.root.GetChild(2).GetChild(0).GetComponent<Animator>();
        }

        _meleeArea = gameObject.GetComponent<BoxCollider>();
        _trailEffet = gameObject.GetComponentInChildren<TrailRenderer>();
    }

    void Update()
    {
        _swingDelay += Time.deltaTime;
        _stabDelay += Time.deltaTime;

        _playerInputs.shoot = false;
        _playerInputs.aim = false;
    }

    public override void Use()
    {
        _isSwingReady = base.Rate < _swingDelay; // 공격속도가 공격 딜레이보다 작으면 공격준비 완료
        _isStabReady = base.Rate < _stabDelay;

        // 무기의 트랜스폼을 가져와서 무기 위치와 방향을 설정
        Transform weaponTransform = transform;  // 무기의 트랜스폼을 직접 사용하거나 _weaponTransform 변수를 사용 가능

        if (_playerInputs == null) _playerInputs = weaponTransform.GetComponentInParent<PlayerInputs>();
        if (_playerMove == null) _playerMove = weaponTransform.GetComponentInParent<PlayerMove_S>();

        if (_playerInputs.swing || _playerInputs.stab)
        {
            if(!_playerMove._grounded) // 지면이 아니면 공격 X
            {
                _playerInputs.swing = false;
                _playerInputs.stab = false;
                return;
            }

            StopCoroutine("MeleeAttackEffect");
            if (_playerInputs.swing && _isSwingReady) // 휘두르기
            {
                _animator.SetTrigger("setSwing");
                _swingDelay = 0;
            }
            else if (_playerInputs.stab && _isStabReady) // 찌르기
            {
                Debug.Log("찌르기");
                _animator.SetTrigger("setStab");
                _stabDelay = 0;
            }
            _playerInputs.swing = false;
            _playerInputs.stab = false;
            StartCoroutine("MeleeAttackEffect");
        }
        else
        {
            // 시작하자마자 휘두르는 문제 방지(유니티 Play 누를 때 클릭 때문에)
            _playerInputs.swing = false;
            _playerInputs.stab = false;
        }
    }

    /// <summary>
    /// 코루틴으로 isAttack, Collider, TrailRenderer 특정 시간 동안만 true
    /// </summary>
    IEnumerator MeleeAttackEffect()
    {
        isAttack = true;
        yield return new WaitForSeconds(0.5f);
        _meleeArea.enabled = true;
        _trailEffet.enabled = true;

        yield return new WaitForSeconds(0.5f);
        _meleeArea.enabled = false;

        yield return new WaitForSeconds(0.5f);
        _trailEffet.enabled = false;
        isAttack = false;
    }

    void OnTriggerEnter(Collider other)
    {
        _hasExited = true;
        
        _entryPoint = other.ClosestPoint(transform.position); // 자르기 시작하는 지점

        // 자기 자신에게 닿은 경우 무시
        if (other.CompareTag("Player") && transform.root.GetChild(2) == other.transform.root.GetChild(2)) 
            return;

        if (other.GetComponent<IStatus>() != null && isAttack)
        {
            other.GetComponent<IStatus>().TakedDamage(Attack);
        }
    }

    #region 절단 기능
    void OnTriggerStay(Collider other)
    {
        Debug.Log("관통");
    }

    // 관통 다 되면 레이어에 따라 절단
    void OnTriggerExit(Collider other)
    {
        _exitPoint = other.ClosestPoint(transform.position); // 자르기 종료된 지점

        Vector3 cutDirection = _exitPoint - _entryPoint; // 자르는 방향
        Vector3 cutInPlane = (_entryPoint + _exitPoint) / 2; // 중간 벡터 (평면의 중심)
        Vector3 cutPlaneNormal = Vector3.Cross((_entryPoint - _exitPoint), (_entryPoint - transform.position)).normalized; // 평면의 법선 벡터

        // 평행일 경우 사잇각이 0이기 때문에 외적 값이 0, 따라서 임의로 설정
        if (cutPlaneNormal.x == 0 && cutPlaneNormal.y == 0 && cutPlaneNormal.z == 0)
        {
            cutPlaneNormal = (_entryPoint - _exitPoint).normalized; // 원래 자르던 방향을 normalize 해서 넣어줘야 됨

            bool isHorizontalCut = Mathf.Abs(cutDirection.x) > Mathf.Abs(cutDirection.y); // 수평으로 잘랐는지
            if (isHorizontalCut) // 가로로 자르는 경우
                cutPlaneNormal = Vector3.up; // x 축 방향으로 자르기 때문에 평면 기준 윗 방향인 Vector3.up으로 설정
            else // 세로로 자르는 경우
                cutPlaneNormal = Vector3.right; // y 축 방향으로 자르기 때문에 평면 기준 윗 방향인 Vector3.right으로 설정
        }

        LayerMask cutableMask = 1 << other.gameObject.layer; // 현재 오브젝트의 레이어 마스크
        if ((_sliceMask.value & cutableMask) != 0)  // 자를 수 있는 레이어에 포함되어 있다면
        {
            Debug.LogWarning("자를 수 있는 오브젝트");
            Cutter.Cut(other.gameObject, cutInPlane, cutPlaneNormal); // 오브젝트를 자르기

            // 자를 때 가해지는 힘을 적용하여 오브젝트를 밀어냄
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddForce(-cutPlaneNormal * _cutForce); // cutDirection 대신에 cutPlaneNormal을 사용
            
            _hasExited = true;
        }
        else
        {
            Debug.LogWarning("잘리지 않음");
        }
    }
    #endregion
}