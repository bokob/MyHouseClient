using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 강도 컨트롤러
/// </summary>
public class RobberController : MonoBehaviour
{
    PlayerStatus _playerStatus;
    [SerializeField] NewWeaponManager _weaponManager;

    void Awake()
    {
        _playerStatus = transform.parent.GetComponent<PlayerStatus>();
    }
    void Start()
    {
        RobberInit(); // 강도 세팅
    }

    void Update()
    {
        // 시체면 가만히 있게 하기
        if (_playerStatus.Role == Define.Role.None) return;

        if (Input.GetKeyUp(KeyCode.T)) // 'T' 누르면 집주인으로 변신
            _playerStatus.TransformIntoHouseowner();

        _weaponManager.UseSelectedWeapon();
    }

    void RobberInit()
    {
        _playerStatus.Role = Define.Role.Robber;
    }
}