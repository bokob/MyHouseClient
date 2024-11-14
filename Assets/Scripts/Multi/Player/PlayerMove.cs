using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using Photon.Pun;

public class PlayerMove : MonoBehaviour
{
    public float _moveSpeed = 3.5f;      // ������ �ӵ�
    public float _sprintSpeed = 5.335f;  // �޸��� �ӵ�
    float _speed;

    [Range(0.0f, 0.3f)]
    public float _rotationSmoothTime = 0.12f;   // ������ ���� ��ȯ
    public float _speedChangeRate = 10.0f;      // �ӵ� ����
    public float _sensitivity = 1f;             // �ΰ���
    
    float _targetRotation = 0.0f;
    float _rotationVelocity;
    float _verticalVelocity;
    float _terminalVelocity = 53.0f;
    bool _rotateOnMove = true;
    
    float _jumpTimeoutDelta;
    float _fallTimeoutDelta;

    [Space(10)]
    public float _jumpHeight = 1.2f;        // ���� ����
    public float _gravity = -15.0f;         // ����Ƽ �������� �⺻ �߷�: -9.81f
    [Space(10)]
    public float _jumpTimeout = 0.50f;      // ���� ��Ÿ��
    public float _fallTimeout = 0.15f;      // �������� ���·� �����ϴµ� �ɸ��� �ð�
    public bool _grounded = true;           // ���鿡 ��Ҵ��� ����
    public float _groundedOffset = 0.14f;  // ���� ���� �� üũ�ϴ� �� y�� ��ġ
    public float _groundedRadius = 0.28f;   // ĳ���� ��Ʈ�ѷ����� ��ü �����ؼ� ����üũ�� ��, ��ü ������
    public LayerMask _groundLayers;         // ���� �ش��ϴ� ���̾� ����ũ

    CharacterController _controller;
    public PlayerInputs _input;
    PlayerStatus _status;

    // player
#if ENABLE_INPUT_SYSTEM
    PlayerInput _playerInput;
#endif

    [SerializeField] CameraController _mainCamera;

    // �ִϸ��̼� ����
    public Animator animator;
    // animation IDs
    int _animIDSpeed;
    int _animIDGrounded;
    int _animIDJump;
    int _animIDFreeFall;
    int _animIDMotionSpeed;
    float _animationBlend;
    bool _hasAnimator;

    public AudioClip _landingAudioClip;                     // �߼Ҹ�
    [Range(0, 1)] public float _footstepAudioVolume = 0.5f; // �߼Ҹ� ũ��

    void Awake()
    {
        _status = GetComponent<PlayerStatus>();
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputs>();
#if ENABLE_INPUT_SYSTEM 
        _playerInput = GetComponent<PlayerInput>();
#else
		Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

        if (_status._isLocalPlayer)
        {
            _controller.enabled = false;
            return;
        }

        // player input is only enabled on owning players
        _playerInput.enabled = true;
        _controller.enabled = true;
    }

    // Start is called before the first frame update
    void Start()
    {

        SetRoleAnimator();




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

    [PunRPC]
    public void SetRoleAnimator()
    {
        if (_status.Role == Define.Role.Robber)
            animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
        else if (_status.Role == Define.Role.Houseowner)
            animator = transform.GetChild(1).gameObject.GetComponent<Animator>();
        else
            Debug.Log("���� ������ ���������µ�?");

        _hasAnimator = (animator != null) ? true : false;

        Debug.LogWarning("_hasAnimator: " + _hasAnimator);
    }


    // Update is called once per frame
    void Update()
    {
        //if (!IsLocalPlayer) return;

        GroundedCheck();    // ����üũ
        JumpAndGravity();   // ����
        Move();             // �̵�
    }

    // �ִϸ��̼� �Ķ���� �ؽ÷� ����
    void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    // �̵�
    void Move()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = _input.sprint ? _sprintSpeed : _moveSpeed;

        // sp�� 0�̸� �⺻ �̵��ӵ�
        if (_status.Sp == 0)
            targetSpeed = _moveSpeed;

        // �ȴ޸��� ���׹̳� ȸ��
        if(!_input.sprint)
            _status.ChargeSp();

        // ������ ������ 0 ���ͷ� ó��
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

            // �޸��� �ִ� ��쿡 ���׹̳� ����
            if (_input.sprint)
                _status.DischargeSp();

        }
        else
        {
            //Debug.Log(OwnerClientId + "�� �����̰� �־��");
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // �÷��̾� �����̰� �ϱ�
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        // update animator if using character
        if (_hasAnimator)
        {
            animator.SetFloat(_animIDSpeed, _animationBlend);
            animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }
    }

    // ���� üũ
    void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _groundedOffset, transform.position.z);
        _grounded = Physics.CheckSphere(spherePosition, _groundedRadius, _groundLayers, QueryTriggerInteraction.Ignore);

        // "Grounded" �ִϸ��̼� �Ķ���� ����
        if (_hasAnimator)
        {
            animator.SetBool(_animIDGrounded, _grounded);
        }
    }

    // ����
    void JumpAndGravity()
    {
        Debug.Log("���� �Լ� ���� ���� ��");

        // ���� ��� ���׹̳��� 0���� Ŀ�� ����
        if (_grounded && _status.Sp > 0)
        {
            Debug.Log("���� ��Ұ�, ���׹̳��� 0 �ʰ�");

            // reset the fall timeout timer
            _fallTimeoutDelta = _fallTimeout;

            // update animator if using character
            if (_hasAnimator)
            {
                animator.SetBool(_animIDJump, false);
                animator.SetBool(_animIDFreeFall, false);
            }

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
                _verticalVelocity = -2f;

            Debug.Log("���� Ű ������ ����");
            // Jump
            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                Debug.Log("���� Ű ������ �� ��");
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);

                // update animator if using character
                if (_hasAnimator)
                {
                    animator.Play("JumpStart");
                    animator.SetBool(_animIDJump, true);
                    Debug.Log("���� ���� �ִϸ��̼�");
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
                    animator.SetBool(_animIDFreeFall, true);
            }

            // if we are not grounded, do not jump
            _input.jump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
            _verticalVelocity += _gravity * Time.deltaTime;
    }

    // �ٴڿ� ��� ���� Ȯ���� ���� Gizmo
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

    // ���� ���� �� ���� �Ҹ� ���� �ϴ� �ִϸ��̼� �̺�Ʈ
    void OnLand(AnimationEvent animationEvent)
    {
        if (_controller == null || _landingAudioClip == null)
            return;

        if (animationEvent.animatorClipInfo.weight > 0.5f)
            AudioSource.PlayClipAtPoint(_landingAudioClip, transform.TransformPoint(_controller.center), _footstepAudioVolume);
    }
}
