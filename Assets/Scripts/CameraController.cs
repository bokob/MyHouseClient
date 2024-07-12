using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    // Camera Controller 임시
    [Header("Cinemachine")]
    public GameObject _cinemachineCameraTarget; // 카메라가 바라볼 목표물
    public float _topClamp = 70.0f;             // 카메라 위 제한 각도
    public float _bottomClamp = -30.0f;         // 카메라 아래 제한 각도
    public float _cameraAngleOverride = 0.0f;   // 카메라 회전 각도 미세 조정에 사용
    public bool _lockCameraPosition = false;    // 카메라 잠금
    GameObject _mainCamera;                     // 메인 카메라
    float _cinemachineTargetYaw;                // 카메라 Y축 회전 제어 사용
    float _cinemachineTargetPitch;

    // player
#if ENABLE_INPUT_SYSTEM
    PlayerInput _playerInput;
#endif

    [SerializeField] public Define.Role PlayerRole { get; set; } = Define.Role.None;

    public PlayerInputs _input;

    const float _threshold = 0.01f;

    public float _sensitivity = 1f;    

    // 입력장치(키보드, 마우스 인식)
    bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return _playerInput.currentControlScheme == "KeyboardMouse";
#else
			return false;
#endif
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 카메라 각도 제한
    static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    // 카메라 회전
    void CameraRotation()
    {
        if (PlayerRole == Define.Role.Robber) return;

        // if there is an input and camera position is not fixed
        if (_input.look.sqrMagnitude >= _threshold && !_lockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

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
