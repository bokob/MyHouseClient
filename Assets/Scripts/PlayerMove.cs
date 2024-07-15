using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    public float _moveSpeed = 2.0f;      // 움직임 속도
    public float _sprintSpeed = 5.335f;  // 달리기 속도
    float _speed;

    [Range(0.0f, 0.3f)]
    public float _rotationSmoothTime = 0.12f;   // 움직임 방향 전환
    public float _speedChangeRate = 10.0f;      // 속도 가속
    public float _sensitivity = 1f;             // 민감도
    
    float _targetRotation = 0.0f;
    float _rotationVelocity;
    float _verticalVelocity;
    float _terminalVelocity = 53.0f;
    bool _rotateOnMove = true;
    
    float _jumpTimeoutDelta;
    float _fallTimeoutDelta;

    [Space(10)]
    public float _jumpHeight = 1.2f;        // 점프 높이
    public float _gravity = -15.0f;         // 유니티 엔진에서 기본 중력: -9.81f
    [Space(10)]
    public float _jumpTimeout = 0.50f;      // 점프 쿨타임
    public float _fallTimeout = 0.15f;      // 떨어지는 상태로 진입하는데 걸리는 시간
    public bool _grounded = true;           // 지면에 닿았는지 여부
    public float _groundedOffset = 0.14f;  // 땅에 닿을 때 체크하는 원 y축 위치
    public float _groundedRadius = 0.28f;   // 캐릭터 컨트롤러에서 구체 형성해서 지면체크할 때, 구체 반지름
    public LayerMask _groundLayers;         // 땅에 해당하는 레이어 마스크

    CharacterController _controller;
    public PlayerInputs _input;
    PlayerStatus _status;

    // player
#if ENABLE_INPUT_SYSTEM
    PlayerInput _playerInput;
#endif

    [SerializeField] CameraController _mainCamera;

    // 애니메이션 관련
    Animator _animator;
    // animation IDs
    int _animIDSpeed;
    int _animIDGrounded;
    int _animIDJump;
    int _animIDFreeFall;
    int _animIDMotionSpeed;
    float _animationBlend;
    bool _hasAnimator;

    public AudioClip _landingAudioClip;                     // 발소리
    [Range(0, 1)] public float _footstepAudioVolume = 0.5f; // 발소리 크기

    // Start is called before the first frame update
    void Start()
    {
        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputs>();
        _status = GetComponent<PlayerStatus>();
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

    // Update is called once per frame
    void Update()
    {
        GroundedCheck();    // 지면체크
        JumpAndGravity();   // 점프
        Move();             // 이동
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

    // 이동
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
    }

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
        if (_grounded && _status.Sp > 0)
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

    public void SetRotateOnMove(bool newRotateOnMove)
    {
        _rotateOnMove = newRotateOnMove;
    }

    // 땅에 닿을 때 착지 소리 나게 하는 애니메이션 이벤트
    void OnLand(AnimationEvent animationEvent)
    {
        if (_controller == null || _landingAudioClip == null)
            return;

        if (animationEvent.animatorClipInfo.weight > 0.5f)
            AudioSource.PlayClipAtPoint(_landingAudioClip, transform.TransformPoint(_controller.center), _footstepAudioVolume);
    }
}
