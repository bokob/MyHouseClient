using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager_S : MonoBehaviour
{
    PlayerInputs _playerInputs;
    PlayerStatus_S _playerStatus;

    [Tooltip("???? ??? ?? ???? ?©£??? ????")]
    public float _switchDelay = 1f;

    [Header("???? ????")]
    [SerializeField] public GameObject _leftItemHand;           // ???? ??? ?????? (???: ??)
    [SerializeField] public GameObject _rightItemHand;          // ??????? ??? ?????? (???: ????)

    [Header("???? ???? ????")]
    public int _selectedWeaponIdx = 0;
    public GameObject _selectedWeapon;
    public GameObject _recentMelee; // the most recent Melee
    public bool _isHoldGun;

    void Awake()
    {
        _playerInputs = transform.root.GetChild(2).GetComponent<PlayerInputs>();
        _playerStatus = transform.root.GetChild(2).GetComponent<PlayerStatus_S>();
    }

    void Start()
    {
        InitRoleWeapon();
        _recentMelee = transform.Find("Knife").gameObject; // _recentMelee init
    }

    void Update()
    {
        if(!_playerInputs.aim && !_playerInputs.reload) // ???????? ???, ???????? ???? ?? ???? ??? ????
            WeaponSwitching(); // ???? ???

        if (Input.GetKeyDown(KeyCode.Q) && _selectedWeapon.name != "Rifle" && _selectedWeapon.name != "Knife")
        {
            Drop();
        }
    }

    /// <summary>
    /// ????? ???? ???? ????
    /// </summary>
    public void InitRoleWeapon()
    {
        // ????? ???? ? ???? ????
        if (_playerStatus.Role == Define.Role.Robber) // ????
        {
            _selectedWeaponIdx = 0;
        }
        else if (_playerStatus.Role == Define.Role.Houseowner) // ??????
        {
            _selectedWeaponIdx = 1;
        }
        SelectWeapon();

        Debug.Log("????? ???? ???? ???? ???");
    }


    /// <summary>
    /// ???? ???
    /// </summary>
    void WeaponSwitching()
    {
        int previousSelectedWeapon = _selectedWeaponIdx;

        if (_selectedWeapon.tag == "Melee") // if now pick weapon tag is Melee
        {
            _recentMelee = _selectedWeapon;
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (_selectedWeapon == _recentMelee)
                _selectedWeaponIdx = 1;
            else if (_selectedWeapon.tag == "Gun")
                _selectedWeaponIdx = _recentMelee.transform.GetSiblingIndex();
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (_selectedWeapon == _recentMelee)
                _selectedWeaponIdx = 1;
            else if (_selectedWeapon.tag == "Gun")
                _selectedWeaponIdx = _recentMelee.transform.GetSiblingIndex();
        }


        if (previousSelectedWeapon != _selectedWeaponIdx) // ???²J ??? ???? ?¥å??? ???? ???
        {
            SelectWeapon();
        }
    }

    /// <summary>
    /// ???? ????
    /// </summary>
    void SelectWeapon()
    {
        int idx = 0;
        foreach(Transform weapon in transform)
        {
            if (idx == _selectedWeaponIdx)
            {
                weapon.gameObject.SetActive(true);
                _selectedWeapon = weapon.gameObject; // ???? ???? ???? ????
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
    /// ????? ???? ???
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

    // ?? ??? ????? ????, ?? ??????? ??????? ????
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
    /// ???? ???
    /// </summary>
    public void PickUp(string meleeName)
    {
        Transform newMelee = transform.Find(meleeName);
        if (!_isHoldGun)
        {
            _selectedWeapon.SetActive(false);
            _selectedWeapon = newMelee.gameObject;
            newMelee.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// ???? ??????
    /// </summary>
    void Drop()
    {
        _selectedWeapon.SetActive(false);
        _selectedWeapon = transform.Find("Knife").gameObject;
        _selectedWeapon.SetActive(true);
    }
}
