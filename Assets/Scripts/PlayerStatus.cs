using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    #region 상태 및 능력치
    [field: SerializeField] public Define.Role Role = Define.Role.None;
    [field: SerializeField] public float Hp { get; set; } = 100;    // 체력
    [field: SerializeField] public float Sp { get; set; } = 100;    // 스테미나
    [field: SerializeField] public float MaxHp { get; private set; } = 100; // 최대 체력
    [field: SerializeField] public float MaxSp { get; private set; } = 100; // 최대 스테미나
    [field: SerializeField] public float Defence { get; private set; } = 1; // 방어력
    #endregion

    #region 애니메이션 및 피해
    Animator _animator;
    List<Renderer> _renderers;
    #endregion

    [Header("무기 관련")]
    [SerializeField] NewWeaponManager _houseOwnerWeaponManager;


    // 시점 변환
    

    void Start()
    {
        InitRole();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        Dead();
        TransformIntoHouseowner();
    }

    /// <summary>
    /// 역할 초기화
    /// </summary>
    public void InitRole()
    {
        /*
         TODO
        호스트면, Houseowner으로 하고, 클라이언트면 Robber

        일단 편의를 위해 강도롤 시작
         */
        Role = Define.Role.Robber;
    }



    /// <summary>
    /// 데미지 입기
    /// </summary>
    /// <param name="attack"> 가할 공격력 </param>
    public void TakedDamage(int attack)
    {
        // 피해가 음수라면 회복되는 현상이 일어나므로 피해의 값을 0이상으로 되게끔 설정
        float damage = Mathf.Max(0, attack - Defence);
        Hp -= damage;

        Debug.Log(gameObject.name + "(이)가 " + damage + " 만큼 피해를 입었음!");
        Debug.Log("남은 체력: " + Hp);
    }

    /// <summary>
    /// 최대 체력의 0.2만큼 회복
    /// </summary>
    public void Heal()
    {
        // 현재 체력이 최대 체력보다 작을 때만 회복 적용
        if (Hp < MaxHp)
        {
            // 회복량
            float healAmount = MaxHp * 0.2f;

            // 회복량과 현재 체력과의 합이 최대 체력을 넘지 않도록 조절
            float healedAmount = Mathf.Clamp(Hp + healAmount, 0, MaxHp) - Hp;

            Debug.Log("이전 체력" + Hp);
            // 체력 회복
            Hp += healedAmount;
            Debug.Log("체력을 " + healedAmount + "만큼 회복!");
            Debug.Log("현재 체력: " + Hp);
        }
        else
        {
            Debug.Log("최대 체력. 회복할 필요 없음.");
        }
    }

    /// <summary>
    /// 최대 스테미나까지 전부 회복
    /// </summary>
    public void SpUp()
    {
        // 현재 스테미나가 최대 스테미나보다 작을 때만 회복 적용
        if (Sp < MaxSp)
        {
            // 회복량과 현재 스테미나와의 합이 최대 스테미나를 넘지 않도록 조절
            float healedAmount = Mathf.Clamp(Sp + MaxSp, 0, MaxHp) - Sp;

            Debug.Log("이전 스테미나" + Sp);
            // 스테미나 회복
            Sp += healedAmount;
            Debug.Log("전부 회복! 현재 Sp: " + Sp);
        }
        else
        {
            Debug.Log("최대 Sp. 회복할 필요 없음.");
        }
    }

    /// <summary>
    /// 스테미나 차오르기
    /// </summary>
    public void ChargeSp()
    {
        Sp += Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    /// <summary>
    /// 스테미나 깎이기
    /// </summary>
    public void DischargeSp()
    {
        Sp -= Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    /// <summary>
    /// 점프시, 스테미나 감소
    /// </summary>
    public void JumpSpDown()
    {
        Sp -= 3;
    }

    /// <summary>
    /// 방어력 증가
    /// </summary>
    public void DefenceUp()
    {

    }

    /// <summary>
    /// 집주인으로 변신
    /// </summary>
    public void TransformIntoHouseowner()
    {
        if (!Input.GetKeyDown(KeyCode.T)) return;

        transform.GetChild(0).gameObject.SetActive(false); // 강도 비활성화
        transform.GetChild(1).gameObject.SetActive(true);  // 집주인 활성화
        Role = Define.Role.Houseowner;

        Debug.Log("현재 상태: " + Role);

        Camera.main.gameObject.GetComponent<CameraController>().SetHouseownerView(); // 집주인 시점으로 설정

        Debug.Log("집주인 시점을 변환");

        // 집주인 무기 세팅
        _houseOwnerWeaponManager.InitRoleWeapon();
        Debug.Log("집주인으로 변신 완료");
    }

    
    /// <summary>
    /// 사망
    /// </summary>
    public void Dead()
    {
        if (Role != Define.Role.None && Hp <= 0)
        {
            _animator.SetTrigger("setDie");
            Role = Define.Role.None; // 시체
            StartCoroutine(DeadSinkCoroutine());
        }
    }

    /// <summary>
    /// 시체 바닥으로 가라앉기
    /// </summary>
    /// <returns></returns>
    IEnumerator DeadSinkCoroutine()
    {
        yield return new WaitForSeconds(3f);
        while (transform.position.y > -1.5f)
        {
            transform.Translate(Vector3.down * 0.1f * Time.deltaTime);
            yield return null;
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// 피해 받으면 Material 붉게 변화
    /// </summary>
    public void HitChangeMaterials()
    {
        // 태그가 무기 또는 몬스터

        for (int i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].material.color = Color.red;
            Debug.Log("색변한다.");
            //Debug.Log(_renderers[i].material.name);
        }

        StartCoroutine(ResetMaterialAfterDelay(1.7f));

        //Debug.Log($"플레이어가 {other.transform.root.name}에게 공격 받음!");
        Debug.Log("공격받은 측의 체력:" + Hp);
    }

    /// <summary>
    /// 피해 받고 Material 원래대로 복구
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator ResetMaterialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        for (int i = 0; i < _renderers.Count; i++)
            _renderers[i].material.color = Color.white;
    }

    void OnTriggerEnter(Collider other)
    {
        //// 자기 자신에게 닿은 경우 무시
        //if (other.transform.root.name == gameObject.name) return;
        if (other.tag == "Melee" || other.tag == "Gun" || other.tag == "Monster")
            HitChangeMaterials();
    }

    public void SetRoleAnimator(RuntimeAnimatorController animController, Avatar avatar)
    {
        _animator.runtimeAnimatorController = animController;
        _animator.avatar = avatar;

        // 애니메이터 속성 교체하고 껐다가 켜야 동작함
        _animator.enabled = false;
        _animator.enabled = true;
    }

    //public PlayerMove _playerMove;
    //public PlayerInputs _input;
    //[Header("공격 관련")]
    //bool _isSwingReady;  // 공격 준비
    //float _swingDelay;   // 공격 딜레이
    //bool _isStabReady;  // 공격 준비
    //float _stabDelay;   // 공격 딜레이

    //public void ChangeIsHoldGun(bool newIsHoldGun)
    //{
    //    _animator.SetBool("isHoldGun", newIsHoldGun);
    //}

    ///// <summary>
    ///// 근접 공격: 좌클릭(휘두르기), 우클릭(찌르기)
    ///// </summary>
    //public void MeleeAttack()
    //{
    //    // 무기 오브젝트가 없거나, 무기가 비활성화 되어 있거나, 무기가 없으면 공격 취소
    //    if (_weaponManager._melee == null || _weaponManager._melee.activeSelf == false || _weaponManager._meleeWeapon == null)
    //        return;

    //    _swingDelay += Time.deltaTime;
    //    _stabDelay += Time.deltaTime;
    //    _isSwingReady = _weaponManager._meleeWeapon.Rate < _swingDelay; // 공격속도가 공격 딜레이보다 작으면 공격준비 완료
    //    _isStabReady = _weaponManager._meleeWeapon.Rate < _stabDelay;

    //    if (_input.swing && _isSwingReady && _playerMove._grounded) // 휘두르기
    //    {
    //        Debug.Log("휘두르기");
    //        _weaponManager._meleeWeapon.Use();
    //        _animator.SetTrigger("setSwing");
    //        _swingDelay = 0;
    //    }
    //    else if (_input.stap && _isStabReady && _playerMove._grounded) // 찌르기
    //    {
    //        Debug.Log("찌르기");
    //        _weaponManager._meleeWeapon.Use();
    //        _animator.SetTrigger("setStab");
    //        _stabDelay = 0;

    //    }
    //    _input.swing = false;
    //    _input.stap = false;
    //}
}
