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
    PlayerController _playerController;
    PlayerStatus _playerStatus;
    WeaponManager _weaponManager;

    [Tooltip("카메라")]
    GameObject _cameras;
    GameObject _quaterFollowCamera;
    GameObject _thirdFollowCamera;
    GameObject _aimCamera;

    void Start()
    {
        // 강도 세팅
        RobberInit();
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.T)) // 'T' 누르면 집주인으로 변신
            _playerStatus.TransformIntoHouseowner();
    }

    void RobberInit()
    {
        _playerController = transform.parent.GetComponent<PlayerController>();
        _playerStatus = transform.parent.GetComponent<PlayerStatus>();
        _weaponManager = transform.parent.GetComponent<WeaponManager>();

        _playerController.PlayerRole = Define.Role.Robber;

        CameraInit();
        RobberWeaponInit();
    }

    void CameraInit() // 카메라 세팅
    {
        // 카메라 오브젝트 세팅
        _cameras = Camera.main.gameObject.transform.parent.gameObject;
        _quaterFollowCamera = _cameras.transform.GetChild(1).gameObject;
        _thirdFollowCamera = _cameras.transform.GetChild(2).gameObject;
        _aimCamera = _cameras.transform.GetChild(3).gameObject;

        // 강도에 맞는 카메라 설정
        _quaterFollowCamera.SetActive(true);
        _thirdFollowCamera.SetActive(false);
        _aimCamera.SetActive(false);
    }

    void RobberWeaponInit() // 강도 무기 세팅
    {
        //_weaponManager.InitializeWeapon();
    }
}