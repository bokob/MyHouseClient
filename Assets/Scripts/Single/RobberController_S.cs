using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobberController_S : MonoBehaviour
{
    PlayerStatus_S _playerStatus;
    [SerializeField] WeaponManager_S _weaponManager;

    void Awake()
    {
        _playerStatus = transform.parent.GetComponent<PlayerStatus_S>();
    }

    void Start()
    {
        _weaponManager.InitRoleWeapon();
    }
    void Update()
    {
        // 시체면 가만히 있게 하기
        if (_playerStatus.Role == Define.Role.None) return;
        
        _weaponManager.UseSelectedWeapon();

    }
}
