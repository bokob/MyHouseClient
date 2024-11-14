using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerStatus_S : MonoBehaviour, IStatus
{
    #region ���� �� �ɷ�ġ
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

        // ���͸��� ���
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

    // ������ �Ա�
    public void TakedDamage(int attack)
    {
        if (Role == Define.Role.None) return; // ��ü�� ��� ����

        float damage = Mathf.Max(0, attack);
        Hp -= damage;
        if (Hp > 0)
        {
            HitChangeMaterials();
            Debug.Log(gameObject.name + "��" + damage + " ��ŭ ��������");
            Debug.Log("������ �԰� �� �� ü��: " + Hp);
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
            float healAmount = MaxHp * 0.35f;
            float healedAmount = Mathf.Clamp(Hp + healAmount, 0, MaxHp) - Hp;
            Hp += healedAmount;
        }
        else
        {
            Debug.Log("ü�� ȸ�� X.");
        }
    }

    // Sp ȸ��
    public void SpUp()
    {
        if (Sp < MaxSp)
        {
            Debug.Log("SP ȸ��");
            float healedAmount = Mathf.Clamp(Sp + MaxSp, 0, MaxSp) - Sp;
            Sp += healedAmount;
        }
        else
        {
            Debug.Log("Sp ȸ�� X");
        }
    }

    // SP �ڿ� ȸ��
    public void ChargeSp()
    {
        Sp += Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    // �� �� Sp ����
    public void DischargeSp()
    {
        Sp -= Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    // ���� Sp ����
    public void JumpSpDown()
    {
        Sp -= 3;
    }

    // ���� ����
    public void DefenceUp()
    {

    }

    public void Dead() // ���
    {
        if (Role != Define.Role.None && Hp <= 0)
        {
            _isDead = true;
            Role = Define.Role.None; // ��ü
            _animator.SetTrigger("setDie");
            GetComponent<PlayerInput>().enabled = false;
            StartCoroutine(DeadSinkCoroutine());
        }
    }

    // ��ü ó��
    IEnumerator DeadSinkCoroutine()
    {
        yield return new WaitForSeconds(5f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ���� �Ծ��� �� �� ����
    public void HitChangeMaterials()
    {
        for (int i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].material.color = Color.red;
            Debug.Log("�� ����");
            //Debug.Log(_renderers[i].material.name);
        }
        StartCoroutine(ResetMaterialAfterDelay(1.7f));
    }

    // ���͸��� �� ����
    IEnumerator ResetMaterialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        for (int i = 0; i < _renderers.Count; i++)
            _renderers[i].material.color = Color.white;
    }

    public void ChangeIsHoldGun(bool isHoldGun)
    {
        if (Role != Define.Role.Houseowner) return;
        _animator.SetBool("isHoldGun", isHoldGun);
    }
}