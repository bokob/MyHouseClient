using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController_S : MonoBehaviour
{
    [Header("Cinemachine")]
    GameObject _mainCamera;                     // 메인 카메라
    GameObject _quaterFollowCamera;
    GameObject _thirdFollowCamera;
    GameObject _aimCamera;
    public GameObject _cinemachineCameraTarget; // 카메라가 바라볼 목표물
    public float _topClamp = 70.0f;             // 카메라 위 제한 각도
    public float _bottomClamp = -30.0f;         // 카메라 아래 제한 각도
    public float _cameraAngleOverride = 0.0f;   // 카메라 회전 각도 미세 조정에 사용
    public bool _lockCameraPosition = false;    // 카메라 잠금
    float _cinemachineTargetYaw;                // 카메라 Y축 회전 제어 사용
    float _cinemachineTargetPitch;

    public Define.View View = Define.View.None; // 카메라 시점

    // player
#if ENABLE_INPUT_SYSTEM
    PlayerInput _playerInput;
#endif

    public PlayerInputs _input;

    const float _threshold = 0.01f;

    public float _sensitivity = 1f;

    // Start is called before the first frame update
    void Awake()
    {
        CameraInit();
    }

    void LateUpdate()
    {
        CameraRotation();
    }


    void CameraInit() // 카메라 초기 세팅
    {
        // 카메라 오브젝트 세팅
        _mainCamera = gameObject;
        _quaterFollowCamera = _mainCamera.transform.parent.GetChild(1).gameObject;
        _thirdFollowCamera = _mainCamera.transform.parent.GetChild(2).gameObject;
        _aimCamera = _mainCamera.transform.parent.GetChild(3).gameObject;
    }

    public void SetHouseownerView() // 집주인 시점 설정
    {
        // 집주인에 맞는 카메라 설정
        _quaterFollowCamera.SetActive(false);
        _thirdFollowCamera.SetActive(true);
        _aimCamera.SetActive(true);
    }

    /// <summary>
    /// 카메라 각도 제한
    /// </summary>
    /// <param name="lfAngle"></param>
    /// <param name="lfMin"></param>
    /// <param name="lfMax"></param>
    /// <returns></returns>
    static public float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    /// <summary>
    /// 카메라 회전
    /// </summary>
    void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (_input.look.sqrMagnitude >= _threshold && !_lockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = 1.0f;

            // 정조준 할 때 천천히 돌아가야 하니까 Sensitivity를 넣어준다.
            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier * _sensitivity;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier * _sensitivity;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _bottomClamp, _topClamp);

        // 시네마신 카메라가 목표를 따라감
        _cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + _cameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }

    public void SetSensitivity(float newSensitivity)
    {
        _sensitivity = newSensitivity;
    }
}
