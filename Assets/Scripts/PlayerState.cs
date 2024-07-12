using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public Status _status;

    [Header("무기 관련")]
    [SerializeField] WeaponManager _weaponManager;
    [SerializeField] public Define.Role PlayerRole { get; set; } = Define.Role.None;

    public PlayerInputs _input;

    [Header("공격 관련")]
    bool _isSwingReady;  // 공격 준비
    float _swingDelay;   // 공격 딜레이
    bool _isStabReady;  // 공격 준비
    float _stabDelay;   // 공격 딜레이

    Animator _animator;
    List<Renderer> _renderers;
    public PlayerMove _playerMove;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Dead();
    }

    // 사망
    public void Dead()
    {
        if (PlayerRole != Define.Role.None && _status.Hp <= 0)
        {
            _animator.SetTrigger("setDie");
            PlayerRole = Define.Role.None; // 시체
            StartCoroutine(DeadSinkCoroutine());
        }
    }

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
    /// 근접 공격: 좌클릭(휘두르기), 우클릭(찌르기)
    /// </summary>
    public void MeleeAttack()
    {
        // 무기 오브젝트가 없거나, 무기가 비활성화 되어 있거나, 무기가 없으면 공격 취소
        if (_weaponManager._melee == null || _weaponManager._melee.activeSelf == false || _weaponManager._meleeWeapon == null)
            return;

        _swingDelay += Time.deltaTime;
        _stabDelay += Time.deltaTime;
        _isSwingReady = _weaponManager._meleeWeapon.Rate < _swingDelay; // 공격속도가 공격 딜레이보다 작으면 공격준비 완료
        _isStabReady = _weaponManager._meleeWeapon.Rate < _stabDelay;

        if (_input.swing && _isSwingReady && _playerMove._grounded) // 휘두르기
        {
            Debug.Log("휘두르기");
            _weaponManager._meleeWeapon.Use();
            _animator.SetTrigger("setSwing");
            _swingDelay = 0;
        }
        else if (_input.stap && _isStabReady && _playerMove._grounded) // 찌르기
        {
            Debug.Log("찌르기");
            _weaponManager._meleeWeapon.Use();
            _animator.SetTrigger("setStab");
            _stabDelay = 0;
            
        }
        _input.swing = false;
        _input.stap = false;
    }

    public void ChangeIsHoldGun(bool newIsHoldGun)
    {
        _animator.SetBool("isHoldGun", newIsHoldGun);
    }
    void OnTriggerEnter(Collider other)
    {
        //// 자기 자신에게 닿은 경우 무시
        //if (other.transform.root.name == gameObject.name) return;
        if (other.tag == "Melee" || other.tag == "Gun" || other.tag == "Monster")
            HitChangeMaterials();
    }

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
        Debug.Log("공격받은 측의 체력:" + _status.Hp);
    }

    IEnumerator ResetMaterialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        for(int i=0; i<_renderers.Count; i++)
            _renderers[i].material.color = Color.white;
    }
}
