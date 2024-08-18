using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus_S : MonoBehaviour
{
    #region ???? ?? ????
    [field: SerializeField] public Define.Role Role = Define.Role.None;
    [field: SerializeField] public float Hp { get; set; } = 100;    // ???
    [field: SerializeField] public float Sp { get; set; } = 100;    // ??????
    [field: SerializeField] public float MaxHp { get; private set; } = 100; // ??? ???
    [field: SerializeField] public float MaxSp { get; private set; } = 100; // ??? ??????
    [field: SerializeField] public float Defence { get; private set; } = 1; // ????
    #endregion

    #region ??????? ?? ????
    Animator _animator;
    List<Renderer> _renderers;
    #endregion

    bool _isDead = false;
    public bool _isPickUp = false;

    WeaponManager_S _weaponManager_S;
    public GameObject nearMeleeObject;
    private string meleeItemName;

    void Awake()
    {
        _animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
        InitRole();

        _weaponManager_S = transform.root.GetComponentInChildren<WeaponManager_S>();

        // ???? ????????
        _renderers = new List<Renderer>();
        Transform[] underTransforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < underTransforms.Length; i++)
        {
            Renderer renderer = underTransforms[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                _renderers.Add(renderer);
                // if (renderer.material.color == null) Debug.Log("?? ???? ???");
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && nearMeleeObject != null && _weaponManager_S._selectedWeapon.tag != "Gun")
        {
            _isPickUp = true;
            GetMeleeItem();
        }
    }

    /// <summary>
    /// ???? ????
    /// </summary>
    public void InitRole()
    {
        /*
         TODO
        ??????, Houseowner???? ???, ????????? Robber

        ????? ?????θ?
         */
        Role = Define.Role.Houseowner;
    }



    /// <summary>
    /// ?????? ???
    /// </summary>
    /// <param name="attack"> ???? ????? </param>
    public void TakedDamage(int attack)
    {
        if (Role == Define.Role.None) return; // 시체일 경우 종료

        // ???????? ??????????? ???복되??? ????????? ????????????? ????????? 값을 0??????????? ???게끔 ??????
        float damage = Mathf.Max(0, attack);
        Hp -= damage;
        if (Hp > 0)
        {
            HitChangeMaterials();
            Debug.Log(gameObject.name + "(???)?? " + damage + " 만큼 ???????? ?????????!");
            Debug.Log("?????? 체력: " + Hp);
        }
        else
        {
            Dead();
        }
    }

    /// <summary>
    /// ??? ????? 0.2??? ???
    /// </summary>
    public void Heal()
    {
        // ???? ????? ??? ??º??? ???? ???? ??? ????
        if (Hp < MaxHp)
        {
            // ?????
            float healAmount = MaxHp * 0.2f;

            // ??????? ???? ??°??? ???? ??? ????? ???? ????? ????
            float healedAmount = Mathf.Clamp(Hp + healAmount, 0, MaxHp) - Hp;

            Debug.Log("???? ???" + Hp);
            // ??? ???
            Hp += healedAmount;
            Debug.Log("????? " + healedAmount + "??? ???!");
            Debug.Log("???? ???: " + Hp);
        }
        else
        {
            Debug.Log("체력 회복 X.");
        }
    }

    /// <summary>
    /// ??? ?????????? ???? ???
    /// </summary>
    public void SpUp()
    {
        // ???? ???????? ??? ?????????? ???? ???? ??? ????
        if (Sp < MaxSp)
        {
            // ??????? ???? ?????????? ???? ??? ???????? ???? ????? ????
            float healedAmount = Mathf.Clamp(Sp + MaxSp, 0, MaxHp) - Sp;

            Debug.Log("???? ??????" + Sp);
            // ?????? ???
            Sp += healedAmount;
            Debug.Log("???? ???! ???? Sp: " + Sp);
        }
        else
        {
            Debug.Log("??? Sp. ????? ??? ????.");
        }
    }

    /// <summary>
    /// ?????? ????????
    /// </summary>
    public void ChargeSp()
    {
        Sp += Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    /// <summary>
    /// ?????? ?????
    /// </summary>
    public void DischargeSp()
    {
        Sp -= Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    /// <summary>
    /// ??????, ?????? ????
    /// </summary>
    public void JumpSpDown()
    {
        Sp -= 3;
    }

    /// <summary>
    /// ???? ????
    /// </summary>
    public void DefenceUp()
    {

    }

    /// <summary>
    /// ????
    /// </summary>
    public void Dead()
    {
        if (Role != Define.Role.None && Hp <= 0)
        {
            _isDead = true;
            Role = Define.Role.None; // 시체
            _animator.SetTrigger("setDie");
            StartCoroutine(DeadSinkCoroutine());
        }
    }

    /// <summary>
    /// ???? ??????
    /// </summary>
    /// <returns></returns>
    IEnumerator DeadSinkCoroutine()
    {
        yield return new WaitForSeconds(5f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// ???? ?????? Material ??? ???
    /// </summary>
    public void HitChangeMaterials()
    {
        for (int i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].material.color = Color.red;
            Debug.Log("???????.");
            //Debug.Log(_renderers[i].material.name);
        }
        StartCoroutine(ResetMaterialAfterDelay(1.7f));
    }

    /// <summary>
    /// ???? ??? Material ???????? ????
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
        //// ??? ?????? ???? ???? ????
        if (other.transform.root.name == gameObject.name) return;

        // if (other.tag == "Monster")
        //     HitChangeMaterials();
    }

    public void GetMeleeItem()
    {
        meleeItemName = nearMeleeObject.name;
        _weaponManager_S.PickUpWeapon(meleeItemName);
        //Destroy(nearMeleeObject);
    }

    public void SetRoleAnimator(RuntimeAnimatorController animController, Avatar avatar)
    {
        _animator.runtimeAnimatorController = animController;
        _animator.avatar = avatar;

        // ???????? ??? ?????? ????? ??? ??????
        _animator.enabled = false;
        _animator.enabled = true;
    }

    public void ChangeIsHoldGun(bool isHoldGun)
    {
        if (Role != Define.Role.Houseowner) return;
        _animator.SetBool("isHoldGun", isHoldGun);
    }
}