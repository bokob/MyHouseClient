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

    [Tooltip("무기 전환 시 지연 시간을 설정")]
    public float _switchDelay = 1f;

    [Header("무기 관련")]
    [SerializeField] public GameObject _leftItemHand;           // 왼손에 있는 아이템 (자식: 탄창)
    [SerializeField] public GameObject _rightItemHand;          // 오른손에 있는 아이템 (자식: 무기)

    [Header("현재 무기 관련")]
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
        if(transform.root.GetChild(2).gameObject.GetComponent<PhotonView>().IsMine&& !_playerInputs.aim && !_playerInputs.reload) // 조준하지 않고, 장전하지 않을 때 무기 교체 가능
            WeaponSwitching(); // 무기 교체
    }

    /// <summary>
    /// 역할에 따른 무기 초기화
    /// </summary>
    public void InitRoleWeapon()
    {
        // 역할에 따른 첫 무기 설정
        if (_playerStatus.Role == Define.Role.Robber) // 강도
        {
            _selectedWeaponIdx = 0;
            _playerStatus._weaponHolder = _playerStatus._weaponHolders[0];
            _selectedWeapon = transform.GetChild(0).gameObject;
        }
        else if (_playerStatus.Role == Define.Role.Houseowner) // 집주인
        {
            _selectedWeaponIdx = 1;
            _playerStatus._weaponHolder = _playerStatus._weaponHolders[1];
            _selectedWeapon = transform.GetChild(1).gameObject;
        }
        SelectWeapon();

        Debug.Log("역할에 따른 무기 초기화 완료");
    }


    /// <summary>0
    /// 무기 교체
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

        // if(Input.GetKeyDown(KeyCode.Alpha1)) // 근접 무기


        if (previousSelectedWeapon != _selectedWeaponIdx) // 마우스 휠로 무기 인덱스 바귀면 교체
        {
            if (_playerStatus.Role == Define.Role.Robber) 
                _selectedWeaponIdx = 0;
            
            SelectWeapon();
        }
    }

    /// <summary>
    /// 무기 선택
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
                _selectedWeapon = weapon.gameObject; // 현재 고른 무기 참조
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
    /// 선택된 무기 사용
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

    // Check Hold Gun, To apply for Gun animation
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
    /// 무기 줍기
    /// </summary>
    void PickUp()
    {

    }

    /// <summary>
    /// 무기 버리기
    /// </summary>
    void Drop()
    {

    }
}