using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 강도 컨트롤러
/// </summary>
public class RobberController : MonoBehaviour
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