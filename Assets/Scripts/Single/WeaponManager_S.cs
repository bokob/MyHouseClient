using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager_S : MonoBehaviour
{
    PlayerInputs _playerInputs;
    PlayerStatus_S _playerStatus;

    [Tooltip("���� ��ȯ �� ���� �ð��� ����")]
    public float _switchDelay = 1f;

    [Header("���� ����")]
    [SerializeField] public GameObject _leftItemHand;           // �޼տ� �ִ� ������ (�ڽ�: źâ)
    [SerializeField] public GameObject _rightItemHand;          // �����տ� �ִ� ������ (�ڽ�: ����)

    [Header("���� ���� ����")]
    public int _selectedWeaponIdx = 0;
    public GameObject _selectedWeapon;
    public bool _isHoldGun;

    void Awake()
    {
        _playerInputs = transform.root.GetChild(2).GetComponent<PlayerInputs>();
        _playerStatus = transform.root.GetChild(2).GetComponent<PlayerStatus_S>();
    }

    void Start()
    {
        InitRoleWeapon();
    }

    void Update()
    {
        if(!_playerInputs.aim && !_playerInputs.reload) // �������� �ʰ�, �������� ���� �� ���� ��ü ����
            WeaponSwitching(); // ���� ��ü
    }

    /// <summary>
    /// ���ҿ� ���� ���� �ʱ�ȭ
    /// </summary>
    public void InitRoleWeapon()
    {
        // ���ҿ� ���� ù ���� ����
        if (_playerStatus.Role == Define.Role.Robber) // ����
        {
            _selectedWeaponIdx = 0;
        }
        else if (_playerStatus.Role == Define.Role.Houseowner) // ������
        {
            _selectedWeaponIdx = 1;
        }
        SelectWeapon();

        Debug.Log("���ҿ� ���� ���� �ʱ�ȭ �Ϸ�");
    }


    /// <summary>
    /// ���� ��ü
    /// </summary>
    void WeaponSwitching()
    {
        int previousSelectedWeapon = _selectedWeaponIdx;

        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (_selectedWeaponIdx >= transform.childCount - 1)
                _selectedWeaponIdx = 0;
            else
                _selectedWeaponIdx++;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (_selectedWeaponIdx <= 0)
                _selectedWeaponIdx = transform.childCount - 1;
            else
                _selectedWeaponIdx--;
        }

        // if(Input.GetKeyDown(KeyCode.Alpha1)) // ���� ����


        if (previousSelectedWeapon != _selectedWeaponIdx) // ���콺 �ٷ� ���� �ε��� �ٱ͸� ��ü
        {
            SelectWeapon();
        }
    }

    /// <summary>
    /// ���� ����
    /// </summary>
    void SelectWeapon()
    {
        int idx = 0;
        foreach(Transform weapon in transform)
        {
            if (idx == _selectedWeaponIdx)
            {
                weapon.gameObject.SetActive(true);
                _selectedWeapon = weapon.gameObject; // ���� ���� ���� ����
                IsHoldGun();
                _playerStatus.ChangeIsHoldGun(_isHoldGun);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }
            idx++;
        }
    }

    /// <summary>
    /// ���õ� ���� ���
    /// </summary>
    public void UseSelectedWeapon()
    {
        if(_selectedWeapon.tag == "Melee")
        {
            _selectedWeapon.GetComponent<Melee_S>().Use();
        }
        else if(_selectedWeapon.tag == "Gun")
        {
            _selectedWeapon.GetComponent<Gun_S>().Use();
        }
        else
        {
            Debug.Log("This weapon has none tag");
        }
    }

    // �� ��� �ִ��� ����, �� �ִϸ��̼� �����ϱ� ����
    void IsHoldGun()
    {
        if (_selectedWeapon.tag == "Gun")
            _isHoldGun = true;
        else if (_selectedWeapon.tag == "Melee")
            _isHoldGun = false;
        else
        {
            Debug.Log("This weapon has none tag");
        }
    }


    /// <summary>
    /// ���� �ݱ�
    /// </summary>
    void PickUp()
    {

    }

    /// <summary>
    /// ���� ������
    /// </summary>
    void Drop()
    {

    }
}
