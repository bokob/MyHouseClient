using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class WeaponManager : MonoBehaviour
{
    PlayerInputs _playerInputs;
    PlayerStatus _playerStatus;

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
        _playerStatus = transform.root.GetChild(2).GetComponent<PlayerStatus>();
        InitRoleWeapon();
    }

    void Start()
    {
        //InitRoleWeapon();
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
            _playerStatus._weaponHolder = _playerStatus._weaponHolders[0];
            _selectedWeapon = transform.GetChild(0).gameObject;
        }
        else if (_playerStatus.Role == Define.Role.Houseowner) // ������
        {
            _selectedWeaponIdx = 1;
            _playerStatus._weaponHolder = _playerStatus._weaponHolders[1];
            _selectedWeapon = transform.GetChild(1).gameObject;
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
            if (_playerStatus.Role == Define.Role.Robber) 
                _selectedWeaponIdx = 0;
            
            SelectWeapon();
        }
    }

    /// <summary>
    /// ���� ����
    /// </summary>
    void SelectWeapon()
    {
        Debug.LogWarning($"_selectedWeaponIdx({transform.root.GetChild(2).GetComponent<PlayerStatus>()._nickName}) :" + _selectedWeaponIdx);
        _playerStatus.gameObject.GetComponent<PhotonView>().RPC("SetWeapon", RpcTarget.AllBuffered, _selectedWeaponIdx);
        
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
        if(_selectedWeapon.tag == "Melee" && (Input.GetMouseButton(0) || Input.GetMouseButton(1)))
        {
            _selectedWeapon.GetComponent<Melee>().Use();
        }
        else if(_selectedWeapon.tag == "Gun")
        {
            _selectedWeapon.GetComponent<Gun>().Use();
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
