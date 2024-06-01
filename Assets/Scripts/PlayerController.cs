using System;
using System.Collections;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Transactions;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [Header("Player")]
    public bool IsDead = false;
    public float MoveSpeed = 2.0f;      // 움직임 속도
    public float SprintSpeed = 5.335f;  // 달리기 속도

    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f; // 움직임 방향 전환

    public float SpeedChangeRate = 10.0f; // 속도 가속

    public float Sensitivity = 1f;

    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Space(10)]
    public float JumpHeight = 1.2f; // 점프 높이
    public float Gravity = -15.0f; // 유니티 엔진에서 기본 중력은 -9.81f

    [Space(10)]
    public float JumpTimeout = 0.50f; // 점프 쿨타임
    public float FallTimeout = 0.15f; // 떨어지는 상태로 진입하는데 걸리는 시간

    public bool Grounded = true; // 지면에 닿았는지 여부
    public float GroundedOffset = -0.14f; // 지면 거칠기
    public float GroundedRadius = 0.28f; // 캐릭터 컨트롤러에서 구체 형성해서 지면체크할 때, 구체 반지름
    public LayerMask GroundLayers; // 땅에 해당하는 레이어 마스크

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    protected GameObject _mainCamera;
    // cinemachine
    protected float _cinemachineTargetYaw;
    protected float _cinemachineTargetPitch;

    protected CharacterController _controller;
    public PlayerInputs _input;

    // player
#if ENABLE_INPUT_SYSTEM
    protected PlayerInput _playerInput;
#endif
    protected float _speed;
    protected float _animationBlend;
    protected float _targetRotation = 0.0f;
    protected float _rotationVelocity;
    protected float _verticalVelocity;
    protected float _terminalVelocity = 53.0f;
    protected bool _rotateOnMove = true;

    public Status _status;

    // timeout deltatime
    protected float _jumpTimeoutDelta;
    protected float _fallTimeoutDelta;

    protected Animator _animator;
    // animation IDs
    protected int _animIDSpeed;
    protected int _animIDGrounded;
    protected int _animIDJump;
    protected int _animIDFreeFall;
    protected int _animIDMotionSpeed;
    protected bool _hasAnimator;

    protected const float _threshold = 0.01f;

    [Header("무기 관련")]
    [SerializeField] protected WeaponManager weaponManager;

    [Header("공격 관련")]
    bool isSwingReady;  // 공격 준비
    float swingDelay;   // 공격 딜레이
    bool isStabReady;  // 공격 준비
    float stabDelay;   // 공격 딜레이

    [SerializeField] public Define.Role PlayerRole { get; set; } = Define.Role.None;

    private void Awake()
    {
        PlayerInit();
        // 플레이어 무기 세팅
        weaponManager.PlayerWeaponInit();
    }

    void PlayerInit()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
        // PlayerCameraRoot 설정
        CinemachineCameraTarget = transform.GetChild(2).gameObject;
        
        _status = gameObject.GetComponent<Status>();
        weaponManager = GetComponent<WeaponManager>();
    }

    private void Update()
    {
        if (!IsOwner && PlayerRole == Define.Role.None) return;

        JumpAndGravity();   // 점프
        GroundedCheck();    // 지면체크
        Move();             // 이동
        MeleeAttack();      // 근접 공격
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    // 입력장치(키보드, 마우스 인식)
    protected bool IsCurrentDeviceMouse
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
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

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
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
        
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
        float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

        // sp가 0이면 기본 이동속도
        if (_status.Sp == 0)
            targetSpeed = MoveSpeed;

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
                Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // normalise input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        // if there is a move input rotate player when the player is moving
        if (_input.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                RotationSmoothTime);

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
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

        // "Grounded" 애니메이션 파라미터 변경
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, Grounded);
        }
    }

    // 점프
    void JumpAndGravity()
    {
        // 땅에 닿고 스테미나가 0보다 커야 점프
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // 스테미나 없으면 점프 못하게 막음
            if (_status.Sp <= 0) return;

            // Jump
            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, true);
                }

                _status.JumpSpDown();
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }
            }

            // if we are not grounded, do not jump
            _input.jump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    // 바닥에 닿는 범위 확인을 위한 Gizmo
    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
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
        if (weaponManager._melee == null || weaponManager._melee.activeSelf == false || weaponManager.meleeWeapon == null)
            return;

        swingDelay += Time.deltaTime;
        stabDelay += Time.deltaTime;
        isSwingReady = weaponManager.meleeWeapon.Rate < swingDelay; // 공격속도가 공격 딜레이보다 작으면 공격준비 완료
        isStabReady = weaponManager.meleeWeapon.Rate < stabDelay;

        if (_input.swing && isSwingReady && Grounded) // 휘두르기
        {
            Debug.Log("휘두르기");
            weaponManager.meleeWeapon.Use();
            _animator.SetTrigger("setSwing");
            swingDelay = 0;
        }
        else if (_input.stap && isStabReady && Grounded) // 찌르기
        {
            Debug.Log("찌르기");
            weaponManager.meleeWeapon.Use();
            _animator.SetTrigger("setStab");
            stabDelay = 0;
            
        }
        _input.swing = false;
        _input.stap = false;
    }

    // 땅에 닿을 때 착지 소리 나게 하는 애니메이션 이벤트
    private void OnLand(AnimationEvent animationEvent)
    {
        if (_controller == null || LandingAudioClip == null)
            return;

        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }
    }


    // 카메라 각도 제한
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    // 카메라 회전
    private void CameraRotation()
    {
        if (PlayerRole == Define.Role.Robber) return;

        // if there is an input and camera position is not fixed
        if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            // 정조준 할 때 천천히 돌아가야 하니까 Sensitivity를 넣어준다.
            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier * Sensitivity;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier * Sensitivity;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // 시네마신 카메라가 목표를 따라감
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }

    public void SetSensitivity(float newSensitivity)
    {
        Sensitivity = newSensitivity;
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

    private void OnTriggerEnter(Collider other)
    {
        // 자기 자신에게 닿은 경우 무시
        if (other.transform.root.name == gameObject.name) return;
        
        // 태그가 무기 태그인 경우
        if(other.tag == "Melee" || other.tag == "Gun")
        {
            // 데미지 적용
            _status.TakedDamage(other.GetComponent<Weapon>().Attack);

            Dead();

            Debug.Log($"플레이어가 {other.transform.root.name}에게 공격 받음!");
        }
    }
} 