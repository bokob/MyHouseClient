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
}

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
        LoadWeaponData();
    }

    void Update()
    {
        if(!_playerInputs.aim && !_playerInputs.reload) // ???????? ???, ???????? ???? ?? ???? ??? ????
            WeaponSwitching(); // ???? ???
        
        if (Input.GetKeyDown(KeyCode.E)) // ???? ?? ?? ?? ??
        {
            if (GameManager_S._instance != null)
            {
                GameManager_S._instance.PickUpWeapon("Knife");
            }
            else
            {
                Debug.LogError("GameManager_S instance is not available.");
            }
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

        // if(Input.GetKeyDown(KeyCode.Alpha1)) // ???? ????


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
    void PickUp()
    {

    }

    /// <summary>
    /// ???? ??????
    /// </summary>
    void Drop()
    {

    }
}


[System.Serializable]
public class WeaponDataWrapper
{
    public WeaponData[] items;
}