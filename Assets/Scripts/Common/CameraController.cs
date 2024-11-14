using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] GameObject _mainCamera;                     // 메인 카메라
    [SerializeField] GameObject _quaterFollowCamera;
    [SerializeField] GameObject _thirdFollowCamera;
    [SerializeField] GameObject _aimCamera;
    [SerializeField] GameObject _minimapCamera;
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

    void Start()
    {
    }

    void LateUpdate()
    {
        if (SceneManager.GetActiveScene().name != "TitleScene" && View == Define.View.Third)
        {
            CameraRotation();
        }
    }

    public void SetRobberView() // 강도 시점 설정
    {
        // 강도에 맞는 카메라 설정
        View = Define.View.Quater;
        _quaterFollowCamera.SetActive(true);
        _thirdFollowCamera.SetActive(false);
        _aimCamera.SetActive(false);
    }

    public void SetHouseownerView() // 집주인 시점 설정
    {
        // 집주인에 맞는 카메라 설정
        View = Define.View.Third;
        _quaterFollowCamera.SetActive(false);
        _thirdFollowCamera.SetActive(true);
        _aimCamera.SetActive(true);
        _minimapCamera.GetComponent<MinimapCameraPosition>().OnOffMinimapPanel(false);
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

        // 씬 전환 후에 오류 방지를 위한 구문, LateUpdate여서 게임 씬에서 파괴된 _cinemachineCameraTarget에 접근하려고 해서 문제되는 듯 하다
        if (_cinemachineCameraTarget == null) return;

        // 시네마신 카메라가 목표를 따라감
        _cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + _cameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }

    public void SetSensitivity(float newSensitivity)
    {
        _sensitivity = newSensitivity;
    }
}
