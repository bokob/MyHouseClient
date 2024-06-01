using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;

//TODO
// Player에서 접근하는게 아닌 총 자체에서 사용하는 것으로 하기
public class Gun : Weapon
{
    #region 총 관련 변수
    [Tooltip("사거리")] [SerializeField] float _range;
    [Tooltip("재장전 시간")] [SerializeField] float _reloadTime;
    [Tooltip("한 번 장전 시, 장전할 탄약 수")] [SerializeField] int _reloadBulletCount;
    [Tooltip("현재 탄약 수")] [SerializeField] int _currentBulletCount;
    [Tooltip("최대 탄약 수")] [SerializeField] int _maxBulletMagazine;
    [Tooltip("총 탄약 수")] [SerializeField] int _totalBulletCount;
    [Tooltip("총 소리")] [SerializeField] AudioClip _fireSound;
    [Tooltip("음원")] [SerializeField] AudioSource _audioSource;
    [Tooltip("총구 섬광 효과")] [SerializeField] ParticleSystem _muzzleFlash;
    [Tooltip("연사 속도")] [SerializeField] float _fireRate = 15f;
    [Tooltip("다음 격발 타이밍")] [SerializeField] float _nextTimeToFire = 0f;
    #endregion

    #region 사격 및 조준 관련 변수
    [Tooltip("조준 카메라")] [SerializeField] CinemachineVirtualCamera _aimVirtualCamera;
    [Tooltip("일반 마우스 민감도")] [SerializeField] float _normalSensitivity;
    [Tooltip("조준 마우스 민감도")] [SerializeField] float _aimSensitivity;
    [Tooltip("조준 가능 Layer")] [SerializeField] LayerMask _aimColliderLayerMask;
    [Tooltip("조준하고 있는 위치")] [SerializeField] Transform _debugTransform;
    [Tooltip("발사되는 총알")] [SerializeField] Transform _pfBulletProjectile;
    [Tooltip("총알 발사되는 위치")] [SerializeField] Transform _spawnBulletPosition;
    [Tooltip("Raycast 맞은 오브젝트")] [SerializeField] Transform _hitTransform;
    [Tooltip("피격 O 여부")] [SerializeField] GameObject _vfxHitGreen;
    [Tooltip("피격 X 오브젝트")] [SerializeField] GameObject _vfxHitRed;
    [Tooltip("재장전 중인지 여부")] [SerializeField] bool _isReload = false;
    [Tooltip("사격 중인지 여부")] [SerializeField] bool _isShoot = false;
    [Tooltip("조준 중인지 여부")] [SerializeField] bool _isAim = false;
    [Tooltip("마우스 조준 좌표")][SerializeField] Vector3 _mouseWorldPosition;

    PlayerController _playerController;
    PlayerInputs _playerInputs;
    Animator _animator;
    public RigBuilder _rigBuilder; // IK 활성/비활성화를 조절하기 위해 접근
    #endregion

    Vector3 _originalRotation; // 총의 원래 회전값

    void Start()
    {
        _originalRotation = transform.localEulerAngles;

        _playerController = base.Master.gameObject.GetComponent<PlayerController>();
        _playerInputs = base.Master.gameObject.GetComponent<PlayerInputs>();
        _animator = base.Master.gameObject.GetComponent<Animator>();
        //rigBuilder = transform.root.GetChild(0).GetComponent<RigBuilder>();
    }

    void HitRayCheck()
    {
        _mouseWorldPosition = Vector3.zero;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (_isShoot) // 쏠 때 반동
        {
            Debug.Log("<반동>");
            _animator.Play("Recoil");
            float recoilAmount = 20f; // 반동 정도

            // 반동을 위한 무작위한 변위 생성
            float randomX = Random.Range(-recoilAmount, recoilAmount);
            float randomY = Random.Range(-recoilAmount, recoilAmount);

            screenCenterPoint += new Vector2(randomX, randomY);
        }

        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        _hitTransform = null;
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, _aimColliderLayerMask))
        {
            Debug.DrawRay(ray.origin, ray.direction * raycastHit.distance, Color.red);
            _debugTransform.position = raycastHit.point;
            _mouseWorldPosition = raycastHit.point;
            _hitTransform = raycastHit.transform;
        }
        else // 충돌 안했을 때
        {
            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.blue);
        }
    }

    // 조준
    void Aim()
    {
        if (_playerInputs.aim)
        {
            _isAim = true;

            transform.localEulerAngles = new Vector3(-125f, 0f, 90f);

            _rigBuilder.enabled = true; // IK 설정

            // 조준 시점으로 카메라 변경
            _aimVirtualCamera.gameObject.SetActive(true);
            _playerController.SetSensitivity(_aimSensitivity);
            _playerController.SetRotateOnMove(false);
            _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));
            _animator.SetBool("isAim", true);

            // 조준하는 방향으로 회전
            Vector3 worldAimTarget = _mouseWorldPosition;
            worldAimTarget.y = base.Master.position.y;
            Vector3 aimDirection = (worldAimTarget - base.Master.position).normalized;

            base.Master.forward = Vector3.Lerp(base.Master.forward, aimDirection, Time.deltaTime * 20f);
        }
        else
        {
            _isAim = false;
            _rigBuilder.enabled = false; // IK 해제

            _animator.SetBool("isAim", false);
            transform.localEulerAngles = _originalRotation;


            // 원래 시점으로 카메라 변경
            _aimVirtualCamera.gameObject.SetActive(false);
            _playerController.SetSensitivity(_normalSensitivity);
            _playerController.SetRotateOnMove(true);
            _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));
        }
    }

    // 격발
    public void Fire()
    {
        if (_currentBulletCount <= 0)
        {
            // 총알 없을 때 사격하려하면 자동 장전되게 하려고 했음
            //Debug.Log("재장전시작");
            //animator.SetBool("isReload", true);
            //StartCoroutine(ReloadCoroutine());

            return;
        }

        if (Input.GetButton("Fire1") && Time.time >= _nextTimeToFire && _isAim && !_isReload)
        {
            // 발사 속도 계산
            _nextTimeToFire = Time.time + 1f / _fireRate;
            
            if (_currentBulletCount > 0)
                Shoot();
        }
    }

    //// 맞았을 때 효과
    //void HitEffect()
    //{
    //    if (blood != null)
    //    {
    //        blood.GetHit();
    //    }
    //}

    // 사격
    void Shoot()
    {
        Debug.Log("발사");
        if (_hitTransform != null)
        {
            // 무언가 맞았으면
            if (_hitTransform.GetComponent<BulletTarget>() != null)
            {
                GameObject GreenEffect = Instantiate(_vfxHitGreen, _mouseWorldPosition, Quaternion.identity);
                Destroy(GreenEffect, 0.1f);


                //hitTransform.GetComponent<PlayerController>().OnHit(GreenEffect.GetComponent<Collider>());
            }
            else
            {
                Instantiate(_vfxHitRed, _mouseWorldPosition, Quaternion.identity);
            }


            Rigidbody hitRigidbody = _hitTransform.GetComponent<Rigidbody>();
            if (hitRigidbody != null)
            {
                // 충격 가할 방향
                Vector3 forceDirection = _hitTransform.position - base.Master.position;
                // 충격 적용
                hitRigidbody.AddForce(forceDirection.normalized * 50.0f, ForceMode.Impulse);
            }
        }

        // 탄약 날라가는 로직
        // ProjectBullet();
        
        _currentBulletCount--;
        _muzzleFlash.Play();
        PlayAudioSource(_fireSound);
        // 총기 반동 코루틴 실행
        StartCoroutine(ReactionCoroutine());
        _playerInputs.shoot = false;
    }

    // 장전
    void Realod()
    {
        if (_playerInputs.reload && !_isReload && _currentBulletCount < _reloadBulletCount)
        {
            Debug.Log("재장전시작");
            _animator.SetBool("isReload", true);
            StartCoroutine(ReloadCoroutine());
        }
    }

    // 장전 코루틴
    IEnumerator ReloadCoroutine()
    {
        if (_totalBulletCount > 0)
        {
            _isReload = true;

            _totalBulletCount += _currentBulletCount;
            _currentBulletCount = 0;

            yield return new WaitForSeconds(_reloadTime);

            if (_totalBulletCount >= _reloadBulletCount)
            {
                _currentBulletCount = _reloadBulletCount;
                _totalBulletCount -= _reloadBulletCount;
            }
            else
            {
                _currentBulletCount = _totalBulletCount;
                _totalBulletCount = 0;
            }

            _isReload = false;
            _playerInputs.reload = false;
            _animator.SetBool("isReload", false);
            Debug.Log("재장전 종료");
        }
        else
        {
            _animator.SetBool("isReload", false);
        }
    }

    // 반동 코루틴
    IEnumerator ReactionCoroutine()
    {
        _isShoot = true;
        yield return new WaitForSeconds(1f);
        _isShoot = false;
    }

    // 사격 소리
    void PlayAudioSource(AudioClip _clip)
    {
        _audioSource.clip = _clip;
        _audioSource.Play();
    }

    // 총알 발사
    void ProjectBullet()
    {
        Vector3 aimDir = (_mouseWorldPosition - _spawnBulletPosition.position).normalized;
        Instantiate(_pfBulletProjectile, _spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
    }

    // 총 사용
    public override void Use()
    {
        HitRayCheck();
        Aim();
        Fire();
        Realod();
    }

    public int GetCurrentBullet()
    {
        return _currentBulletCount;
    }

    public int GetTotalBullet()
    {
        return _totalBulletCount;
    }
}