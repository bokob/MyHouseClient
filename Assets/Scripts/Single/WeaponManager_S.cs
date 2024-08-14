using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;


public class WeaponData
{
    public string Name;
    public Define.Type Type;
    public int Attack;
    public float Rate;
    public float Range;
}

public class WeaponManager_S : MonoBehaviour
{
    PlayerInputs _playerInputs;
    PlayerStatus_S _playerStatus;

    [Tooltip("무기 전환 시 지연 시간을 설정")]
    public float _switchDelay = 1f;

    [Header("무기 관련")]
    [SerializeField] public GameObject _leftItemHand;           // 왼손에 있는 아이템 (자식: 탄창)
    [SerializeField] public GameObject _rightItemHand;          // 오른손에 있는 아이템 (자식: 무기)

    [Header("현재 무기 관련")]
    public int _selectedWeaponIdx = 0;
    public GameObject _selectedWeapon;
    public GameObject _recentMelee; // the most recent Melee
    public bool _isHoldGun;

    [Header("?? ??")]
    public List<WeaponData> weapons;

    void Awake()
    {
        _playerInputs = transform.root.GetChild(2).GetComponent<PlayerInputs>();
        _playerStatus = transform.root.GetChild(2).GetComponent<PlayerStatus_S>();
    }

    void Start()
    {
        InitRoleWeapon();

        _recentMelee = transform.Find("Knife").gameObject; // _recentMelee init

        LoadWeaponData();
    }

    void Update()
    {
        if (!_playerInputs.aim && !_playerInputs.reload) // 조준하지 않고, 장전하지 않을 때 무기 교체 가능
            WeaponSwitching(); // 무기 교체

        if (Input.GetKeyDown(KeyCode.Q) && _selectedWeapon.name != "Rifle" && _selectedWeapon.name != "Knife")
        {
            Drop();
        }
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
        }
        else if (_playerStatus.Role == Define.Role.Houseowner) // 집주인
        {
            _selectedWeaponIdx = 1;
        }
        SelectWeapon();

        Debug.Log("역할에 따른 무기 초기화 완료");
    }

    void LoadWeaponData()
    {
        string filePath = Path.Combine(Application.dataPath, "Scripts/Weapon/weapondata.json");

        if (File.Exists(filePath))
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                weapons = JsonConvert.DeserializeObject<List<WeaponData>>(jsonContent);

                if (weapons != null)
                {
                    Debug.Log("Weapon data loaded successfully.");

                    // ???? ????? ??
                    if (weapons.Count == 0)
                    {
                        Debug.LogWarning("Weapon list is empty.");
                    }
                    else
                    {
                        foreach (var weapon in weapons)
                        {
                            Debug.Log($"Weapon: {weapon.Name}, Attack: {weapon.Attack}, Rate: {weapon.Rate}");
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                Debug.LogError($"JSON parsing error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load weapon data: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("Weapon data file not found.");
        }

    }
    public WeaponData GetWeaponByName(string weaponName)
    {
        return weapons.Find(weapon => weapon.Name == weaponName);
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


        if (previousSelectedWeapon != _selectedWeaponIdx) // ???콺 ??? ???? ?ε??? ???? ???
        {
            if (_playerStatus.Role == Define.Role.Robber) _selectedWeaponIdx = 0;

            SelectWeapon();
        }
    }

    /// <summary>
    /// 무기 선택
    /// </summary>
    void SelectWeapon()
    {
        int idx = 0;
        foreach (Transform weapon in transform)
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
    /// 선택된 무기 사용
    /// </summary>
    public void UseSelectedWeapon()
    {
        if (_selectedWeapon.tag == "Melee")
        {
            _selectedWeapon.GetComponent<Melee_S>().Use();
        }
        else if (_selectedWeapon.tag == "Gun")
        {
            _selectedWeapon.GetComponent<Gun_S>().Use();
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
    public void PickUp(string meleeName)
    {
        Transform newMelee = transform.Find(meleeName);
        if (!_isHoldGun)
        {
            _selectedWeapon.SetActive(false);
            _selectedWeapon = newMelee.gameObject;
            newMelee.gameObject.SetActive(true);
        }
        WeaponData weapon = GetWeaponByName(meleeName);
        if (weapon != null)
        {
            Debug.Log($"Picked up {weapon.Name}. Attack: {weapon.Attack}, Rate: {weapon.Rate}");
            Melee_S _currentWeapon = _selectedWeapon.GetComponent<Melee_S>(); 
            _currentWeapon.Attack = weapon.Attack;
            _currentWeapon.Rate = weapon.Rate;
            _currentWeapon.Range = weapon.Range;
        }
        else
        {
            Debug.LogWarning("Weapon not found!");
        }
    }

    /// <summary>
    /// 무기 버리기
    /// </summary>
    void Drop()
    {
        _selectedWeapon.SetActive(false);
        _selectedWeapon = transform.Find("Knife").gameObject;
        _selectedWeapon.SetActive(true);
    }
}