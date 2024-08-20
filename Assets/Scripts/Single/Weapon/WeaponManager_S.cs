using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager_S : MonoBehaviour
{
    PlayerInputs _playerInputs;
    PlayerStatus_S _playerStatus;

    [Tooltip("무기 전환 시 지연 시간을 설정")]
    public float _switchDelay = 1f;

    [Header("현재 무기 관련")]
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
        // 시체면 가만히 있게 하기
        if (_playerStatus.Role == Define.Role.None) return;

        UseSelectedWeapon();

        if (!_playerInputs.aim && !_playerInputs.reload) // 조준하지 않고, 장전하지 않을 때 무기 교체 가능
            WeaponSwitching(); // 무기 교체

        if (Input.GetKeyDown(KeyCode.Q) && _selectedWeapon.name != "Rifle" && _selectedWeapon.name != "Knife")
        {
            DropWeapon();
        }
    }

    /// <summary>
    /// 역할에 따른 무기 초기화
    /// </summary>
    public void InitRoleWeapon()
    {
        if (_playerStatus.Role == Define.Role.Houseowner) // 집주인
        {
            _selectedWeaponIdx = 1;
        }
        SelectWeapon(_selectedWeaponIdx);

        Debug.Log("역할에 따른 무기 초기화 완료");
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

        if (Input.GetAxis("Mouse ScrollWheel") > 0f || Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (_selectedWeapon == _recentMelee)
                _selectedWeaponIdx = 1;
            else if (_selectedWeapon.tag == "Gun")
                _selectedWeaponIdx = _recentMelee.transform.GetSiblingIndex();
        }

        if (previousSelectedWeapon != _selectedWeaponIdx) // ???콺 ??? ???? ?ε??? ???? ???
        {
            if (_playerStatus.Role == Define.Role.Robber) _selectedWeaponIdx = 0;

            SelectWeapon(_selectedWeaponIdx);
        }
    }

    /// <summary>
    /// 무기 선택
    /// </summary>
    void SelectWeapon(int weaponIndex)
    {
        Debug.LogWarning($"_selectedWeaponIdx({transform.root.GetChild(2).GetComponent<PlayerStatus_S>().name}) :" + weaponIndex);
        _selectedWeaponIdx = weaponIndex;

        int idx = 0;
        foreach (Transform weapon in transform)
        {
            if (idx == weaponIndex)
            {
                weapon.gameObject.SetActive(true);
                _selectedWeapon = weapon.gameObject; // 현재 고른 무기 참조
                IsHoldGun();
                _playerStatus.ChangeIsHoldGun(_isHoldGun);

                if (_playerStatus == null) Debug.LogError($"playerStatus가 널");
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
    public void PickUpWeapon(string meleeName)
    {
        Transform newMelee = transform.Find(meleeName);
        _selectedWeaponIdx = newMelee.GetSiblingIndex(); // 교체할 무기가 몇 번째 자식인지

        if (_isHoldGun)
        {

        }        
        else
        {
            _selectedWeapon.SetActive(false);
            _selectedWeapon = newMelee.gameObject;
            newMelee.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 무기 버리기
    /// </summary>
    void DropWeapon()
    {
        GameObject droppedSelectedWeapon = Instantiate(_selectedWeapon, _selectedWeapon.transform.position, _selectedWeapon.transform.rotation); // instatntiation.
        Destroy(droppedSelectedWeapon.GetComponent<Melee_S>()); // Melee_S script delete for error prevention. 

        droppedSelectedWeapon.transform.localScale = droppedSelectedWeapon.transform.localScale * 1.7f; // size up.

        StartCoroutine(DropAndBounce(droppedSelectedWeapon));

        _selectedWeapon.SetActive(false);
        _selectedWeapon = transform.Find("Knife").gameObject;
        _selectedWeapon.SetActive(true);
        WeaponData weapon = GameManager_S._instance.GetWeaponStatusByName("Knife");
        Melee_S _currentWeapon = _selectedWeapon.GetComponent<Melee_S>();
        _currentWeapon.Attack = weapon.Attack;
        _currentWeapon.Rate = weapon.Rate;
        _currentWeapon.Range = weapon.Range;
    }

    IEnumerator DropAndBounce(GameObject droppedSelectedWeapon)
    {
        float floorY = transform.root.GetChild(2).position.y + 0.3f; // floorY is Player object's position.y + 0.3f.

        Vector3 velocity = new Vector3(0, -1f, 0); // first velocity.
        float gravity = -9.8f;
        float bounceDamping = 0.6f;
        float horizontalDamping = 0.98f;

        while (true)
        {
            droppedSelectedWeapon.transform.position += velocity * Time.deltaTime;

            if (droppedSelectedWeapon.transform.position.y <= floorY)
            {
                //bouncing.
                droppedSelectedWeapon.transform.position = new Vector3(droppedSelectedWeapon.transform.position.x, floorY, droppedSelectedWeapon.transform.position.z);
                velocity.y = -velocity.y * bounceDamping;

                velocity.x *= horizontalDamping;
                velocity.z *= horizontalDamping;

                if (Mathf.Abs(velocity.y) < 0.1f)
                {
                    velocity.y = 0;
                    break;
                }
            }
            else
            {
                // gravity.
                velocity.y += gravity * Time.deltaTime;
            }

            yield return null; // wait for next frame.
        }

        yield return new WaitForSeconds(1f);
        Destroy(droppedSelectedWeapon);
    }
}