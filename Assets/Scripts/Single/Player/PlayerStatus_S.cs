using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerStatus_S : MonoBehaviour, IStatus
{
    #region 상태 및 능력치
    [field: SerializeField] public Define.Role Role { get; set; } = Define.Role.None;
    [field: SerializeField] public float Hp { get; set; } = 100;    // ???
    [field: SerializeField] public float Sp { get; set; } = 100;    // ??????
    [field: SerializeField] public float MaxHp { get; set; } = 100; // ??? ???
    [field: SerializeField] public float MaxSp { get; set; } = 100; // ??? ??????
    [field: SerializeField] public float Defence { get; set; } = 1; // ????
    #endregion

    #region
    Animator _animator;
    List<Renderer> _renderers;
    #endregion

    bool _isDead = false;
    public bool _isPickUp = false;

    public WeaponManager_S _weaponManager_S;
    public GameObject nearMeleeObject;
    private string meleeItemName;

    void Awake()
    {
        _animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
        InitRole();

        _weaponManager_S = transform.root.GetComponentInChildren<WeaponManager_S>();

        // 매터리얼 얻기
        _renderers = new List<Renderer>();
        Transform[] underTransforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < underTransforms.Length; i++)
        {
            Renderer renderer = underTransforms[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                _renderers.Add(renderer);
            }
        }
    }

    public void InitRole()
    {
        Role = Define.Role.Houseowner;
    }

    // 데미지 입기
    public void TakedDamage(int attack)
    {
        if (Role == Define.Role.None) return; // 시체일 경우 종료

        float damage = Mathf.Max(0, attack);
        Hp -= damage;
        if (Hp > 0)
        {
            HitChangeMaterials();
            Debug.Log(gameObject.name + "가" + damage + " 만큼 피해입음");
            Debug.Log("데미지 입고 난 후 체력: " + Hp);
        }
        else
        {
            Dead();
        }
    }

    public void Heal()
    {
        if (Hp < MaxHp)
        {
            // ?????
            float healAmount = MaxHp * 0.2f;
            float healedAmount = Mathf.Clamp(Hp + healAmount, 0, MaxHp) - Hp;
            Hp += healedAmount;
        }
        else
        {
            Debug.Log("체력 회복 X.");
        }
    }

    // Sp 회복
    public void SpUp()
    {
        if (Sp < MaxSp)
        {
            Debug.Log("SP 회복");
            float healedAmount = Mathf.Clamp(Sp + MaxSp, 0, MaxSp) - Sp;
            Sp += healedAmount;
        }
        else
        {
            Debug.Log("Sp 회복 X");
        }
    }

    // SP 자연 회복
    public void ChargeSp()
    {
        Sp += Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    // 뛸 때 Sp 감소
    public void DischargeSp()
    {
        Sp -= Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    // 점프 Sp 감소
    public void JumpSpDown()
    {
        Sp -= 3;
    }

    // 방어력 증가
    public void DefenceUp()
    {

    }

    public void Dead() // 사망
    {
        if (Role != Define.Role.None && Hp <= 0)
        {
            _isDead = true;
            Role = Define.Role.None; // 시체
            _animator.SetTrigger("setDie");
            GetComponent<PlayerInput>().enabled = false;
            StartCoroutine(DeadSinkCoroutine());
        }
    }

    // 시체 처리
    IEnumerator DeadSinkCoroutine()
    {
        yield return new WaitForSeconds(5f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 피해 입었을 때 색 변함
    public void HitChangeMaterials()
    {
        for (int i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].material.color = Color.red;
            Debug.Log("색 변함");
            //Debug.Log(_renderers[i].material.name);
        }
        StartCoroutine(ResetMaterialAfterDelay(1.7f));
    }

    // 매터리얼 색 복구
    IEnumerator ResetMaterialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        for (int i = 0; i < _renderers.Count; i++)
            _renderers[i].material.color = Color.white;
    }

    public void GetMeleeItem()
    {
        meleeItemName = nearMeleeObject.name;
        _weaponManager_S.PickUpWeapon(meleeItemName);
        //Destroy(nearMeleeObject);
    }

    public void ChangeIsHoldGun(bool isHoldGun)
    {
        if (Role != Define.Role.Houseowner) return;
        _animator.SetBool("isHoldGun", isHoldGun);
    }
}