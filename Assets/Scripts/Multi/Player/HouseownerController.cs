using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 집주인 고유의 기능만 존재
/// </summary>
public class HouseownerController : MonoBehaviour
{
    PlayerStatus _playerStatus;
    [SerializeField] WeaponManager _weaponManager;

    void Awake()
    {
        _playerStatus = transform.parent.GetComponent<PlayerStatus>();
    }

    void Update()
    {
        // 시체면 가만히 있게 하기
        if (_playerStatus.Role == Define.Role.None) return;
        
        _weaponManager.UseSelectedWeapon();
    }
}