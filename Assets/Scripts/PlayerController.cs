using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player")]
    public bool _isDead = false;
    public float _moveSpeed = 2.0f;      // 움직임 속도
    public float _sprintSpeed = 5.335f;  // 달리기 속도
    float _speed;
    float _animationBlend;

    [Range(0.0f, 0.3f)]
    public float _rotationSmoothTime = 0.12f;   // 움직임 방향 전환
    public float _speedChangeRate = 10.0f;      // 속도 가속
    public float _sensitivity = 1f;             // 민감도

    float _targetRotation = 0.0f;
    float _rotationVelocity;
    float _verticalVelocity;
    float _terminalVelocity = 53.0f;
    bool _rotateOnMove = true;

    // timeout deltatime
    float _jumpTimeoutDelta;
    float _fallTimeoutDelta;

    // 지면, 점프 관련
    [Space(10)]
    public float _jumpHeight = 1.2f;        // 점프 높이
    public float _gravity = -15.0f;         // 유니티 엔진에서 기본 중력: -9.81f
    [Space(10)]
    public float _jumpTimeout = 0.50f;      // 점프 쿨타임
    public float _fallTimeout = 0.15f;      // 떨어지는 상태로 진입하는데 걸리는 시간
    public bool _grounded = true;           // 지면에 닿았는지 여부
    public float _groundedOffset = -0.14f;  // 지면 거칠기
    public float _groundedRadius = 0.28f;   // 캐릭터 컨트롤러에서 구체 형성해서 지면체크할 때, 구체 반지름
    public LayerMask _groundLayers;         // 땅에 해당하는 레이어 마스크

    [Header("Cinemachine")]
    public GameObject _cinemachineCameraTarget; // 카메라가 바라볼 목표물
    public float _topClamp = 70.0f;             // 카메라 위 제한 각도
    public float _bottomClamp = -30.0f;         // 카메라 아래 제한 각도
    public float _cameraAngleOverride = 0.0f;   // 카메라 회전 각도 미세 조정에 사용
    public bool _lockCameraPosition = false;    // 카메라 잠금
    GameObject _mainCamera;                     // 메인 카메라
    float _cinemachineTargetYaw;                // 카메라 Y축 회전 제어 사용
    float _cinemachineTargetPitch;              // 카메라 상하 회전 제어 사용

    // 애니메이션 관련
     Animator _animator;
    // animation IDs
    int _animIDSpeed;
    int _animIDGrounded;
    int _animIDJump;
    int _animIDFreeFall;
    int _animIDMotionSpeed;
    bool _hasAnimator;

    const float _threshold = 0.01f;

    public AudioClip _landingAudioClip;                     // 발소리
    [Range(0, 1)] public float _footstepAudioVolume = 0.5f; // 발소리 크기

    CharacterController _controller;
    public PlayerInputs _input;
    public Status _status;

    // player
#if ENABLE_INPUT_SYSTEM
    PlayerInput _playerInput;
#endif

    [Header("무기 관련")]
    [SerializeField] WeaponManager _weaponManager;

    [Header("공격 관련")]
    bool _isSwingReady;  // 공격 준비
    float _swingDelay;   // 공격 딜레이
    bool _isStabReady;  // 공격 준비
    float _stabDelay;   // 공격 딜레이

    List<Renderer> _renderers;

    [SerializeField] public Define.Role PlayerRole { get; set; } = Define.Role.None;

    void Awake()
    {
        PlayerInit();
        _weaponManager.PlayerWeaponInit(); // 플레이어 무기 세팅
    }

    void PlayerInit()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        // PlayerCameraRoot 설정
        _cinemachineCameraTarget = transform.GetChild(2).gameObject;
        
        _status = gameObject.GetComponent<Status>();
        _weaponManager = GetComponent<WeaponManager>();

        // 플레이어 하위의 모든 매터리얼 구하기
        _renderers = new List<Renderer>();
        Transform[] playerUnderTransforms = GetComponentsInChildren<Transform>(true);
        for(int i=0; i< playerUnderTransforms.Length; i++)
        {
            Renderer renderer = playerUnderTransforms[i].GetComponent<Renderer>();
            if(renderer!=null)
                _renderers.Add(renderer);
        }
    }

    void Update()
    {
        if (PlayerRole == Define.Role.None) return;

        JumpAndGravity();   // 점프
        GroundedCheck();    // 지면체크
        Move();             // 이동
        MeleeAttack();      // 근접 공격
        Dead();
    }

    void LateUpdate()
    {
        CameraRotation();
    }

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

    void Start()
    {
        _cinemachineTargetYaw = _cinemachineCameraTarget.transform.rotation.eulerAngles.y;

        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputs>();
#if ENABLE_INPUT_SYSTEM 
        _playerInput = GetComponent<PlayerInput>();
#else
		Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

        AssignAnimationIDs();

        // reset our timeouts on start
        _jumpTimeoutDelta = _jumpTimeout;
        _fallTimeoutDelta = _fallTimeout;
        
    }

    // 애니메이션 파라미터 해시로 관리
    void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    void Move()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = _input.sprint ? _sprintSpeed : _moveSpeed;

        // sp가 0이면 기본 이동속도
        if (_status.Sp == 0)
            targetSpeed = _moveSpeed;

        // 안달리면 스테미나 회복
        if(!_input.sprint)
            _status.ChargeSp();

        // 움직임 없으면 0 벡터로 처리
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * _speedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * _speedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // normalise input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        // if there is a move input rotate player when the player is moving
        if (_input.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                _rotationSmoothTime);

            // rotate to face input direction relative to camera position
            if (_rotateOnMove)
            {
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            // 달리고 있는 경우에 스테미나 감소
            if (_input.sprint)
                _status.DischargeSp();

        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // 플레이어 움직이게 하기
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }
    }// 이동

    // 지면 체크
    void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _groundedOffset, transform.position.z);
        _grounded = Physics.CheckSphere(spherePosition, _groundedRadius, _groundLayers, QueryTriggerInteraction.Ignore);

        // "Grounded" 애니메이션 파라미터 변경
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, _grounded);
        }
    }

    // 점프
    void JumpAndGravity()
    {
        // 땅에 닿고 스테미나가 0보다 커야 점프
        if (_grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = _fallTimeout;

            // update animator if using character
            if (_hasAnimator)
            {
                
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
                _verticalVelocity = -2f;

            // 스테미나 없으면 점프 못하게 막음
            if (_status.Sp <= 0) return;

            // Jump
            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.Play("JumpStart");
                    _animator.SetBool(_animIDJump, true);
                }

                _status.JumpSpDown();
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
                _jumpTimeoutDelta -= Time.deltaTime;
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = _jumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
                _fallTimeoutDelta -= Time.deltaTime;
            else
            {
                // update animator if using character
                if (_hasAnimator)
                    _animator.SetBool(_animIDFreeFall, true);
            }

            // if we are not grounded, do not jump
            _input.jump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
            _verticalVelocity += _gravity * Time.deltaTime;
    }

    // 바닥에 닿는 범위 확인을 위한 Gizmo
    void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (_grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - _groundedOffset, transform.position.z), _groundedRadius);
    }

    // 사망
    public void Dead()
    {
        if (PlayerRole != Define.Role.None && _status.Hp <= 0)
        {
            _animator.SetTrigger("setDie");
            PlayerRole = Define.Role.None; // 시체
            StartCoroutine(DeadSinkCoroutine());
        }
    }

    IEnumerator DeadSinkCoroutine()
    {
        yield return new WaitForSeconds(3f);
        while (transform.position.y > -1.5f)
        {
            transform.Translate(Vector3.down * 0.1f * Time.deltaTime);
            yield return null;
        }
        Destroy(gameObject);
    }


    /// <summary>
    /// 근접 공격: 좌클릭(휘두르기), 우클릭(찌르기)
    /// </summary>
    public void MeleeAttack()
    {
        // 무기 오브젝트가 없거나, 무기가 비활성화 되어 있거나, 무기가 없으면 공격 취소
        if (_weaponManager._melee == null || _weaponManager._melee.activeSelf == false || _weaponManager._meleeWeapon == null)
            return;

        _swingDelay += Time.deltaTime;
        _stabDelay += Time.deltaTime;
        _isSwingReady = _weaponManager._meleeWeapon.Rate < _swingDelay; // 공격속도가 공격 딜레이보다 작으면 공격준비 완료
        _isStabReady = _weaponManager._meleeWeapon.Rate < _stabDelay;

        if (_input.swing && _isSwingReady && _grounded) // 휘두르기
        {
            Debug.Log("휘두르기");
            _weaponManager._meleeWeapon.Use();
            _animator.SetTrigger("setSwing");
            _swingDelay = 0;
        }
        else if (_input.stap && _isStabReady && _grounded) // 찌르기
        {
            Debug.Log("찌르기");
            _weaponManager._meleeWeapon.Use();
            _animator.SetTrigger("setStab");
            _stabDelay = 0;
            
        }
        _input.swing = false;
        _input.stap = false;
    }

    // 땅에 닿을 때 착지 소리 나게 하는 애니메이션 이벤트
    void OnLand(AnimationEvent animationEvent)
    {
        if (_controller == null || _landingAudioClip == null)
            return;

        if (animationEvent.animatorClipInfo.weight > 0.5f)
            AudioSource.PlayClipAtPoint(_landingAudioClip, transform.TransformPoint(_controller.center), _footstepAudioVolume);
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

    public void SetRotateOnMove(bool newRotateOnMove)
    {
        _rotateOnMove = newRotateOnMove;
    }

    public void SetRoleAnimator(RuntimeAnimatorController animController, Avatar avatar)
    {
        _animator.runtimeAnimatorController = animController;
        _animator.avatar = avatar;

        // 애니메이터 속성 교체하고 껐다가 켜야 동작함
        _animator.enabled = false;
        _animator.enabled = true;
    }

    public void ChangeIsHoldGun(bool newIsHoldGun)
    {
        _animator.SetBool("isHoldGun", newIsHoldGun);
    }

    void OnTriggerEnter(Collider other)
    {
        //// 자기 자신에게 닿은 경우 무시
        //if (other.transform.root.name == gameObject.name) return;
        if (other.tag == "Melee" || other.tag == "Gun" || other.tag == "Monster")
            HitChangeMaterials();
    }

    public void HitChangeMaterials()
    {
        // 태그가 무기 또는 몬스터
        
        for (int i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].material.color = Color.red;
            Debug.Log("색변한다.");
            //Debug.Log(_renderers[i].material.name);
        }

        StartCoroutine(ResetMaterialAfterDelay(1.7f));

        //Debug.Log($"플레이어가 {other.transform.root.name}에게 공격 받음!");
        Debug.Log("공격받은 측의 체력:" + _status.Hp);
    }

    IEnumerator ResetMaterialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        for(int i=0; i<_renderers.Count; i++)
            _renderers[i].material.color = Color.white;
    }
} 