using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class Monster : MonoBehaviour, IStatus
{
    [SerializeField]
    private IObjectPool<Monster> _managedPool;

    NavMeshAgent _nmAgent;
    Animator _anim;
    CapsuleCollider _collider;

    #region 상태, 능력치
    public Define.MonsterState _state = Define.MonsterState.Patrol; // 현재 상태
    public bool _isDead = false;

    List<Renderer> _renderers; // 피해 입었을 때 렌더러 색 변환에 사용할 리스트
    List<Color> _originColors;


    public Define.Role Role { get; set; }
    [field: SerializeField] public float Hp { get; set; } = 300f;                   // 체력
    public float Sp { get; set; }
    public float MaxHp { get; set; }
    public float MaxSp { get; set; }
    public float Defence { get; set; }


    [field: SerializeField] public int _attack { get; private set; } = 30;                   // 공격력
    #endregion

    #region 시야 관련
    public float _radius;              // 시야 범위
    [Range(0, 360)]
    public float _angle;               // 시야각
    public LayerMask _targetMask;      // 목표
    public LayerMask _obstructionMask; // 장애물
    public bool _canSeePlayer;
    #endregion

    #region 추격 관련
    public float _chaseRange = 10f; // 추격 범위
    public float _lostDistance; // 놓치는 거리
    #endregion

    #region 순찰 및 공격 관련
    public Transform _centerPoint;  // 순찰 위치 정할 기준점
    public float _range;            // 순찰 위치 정할 범위
    public float _patrolSpeed = 1f; // 순찰 속도

    public float _attackRange = 0.1f; // 공격 범위
    public float _attackDelay = 2f; // 공격 간격
    float nextAttackTime = 0f;
    public Transform _target = null; // 목표
    #endregion

    public bool _isTakingDamage = false; // 싱글 중복 타격 방지용
    public int _monsterCount = 0; // 유령 수

    void Awake()
    {
        MonsterInit(); // 몬스터 세팅
    }

    void MonsterInit()
    {
        _anim = GetComponent<Animator>();
        _collider = GetComponent<CapsuleCollider>();
        _nmAgent = GetComponent<NavMeshAgent>();
        _centerPoint = transform;

        if(SceneManager.GetActiveScene().name == "SinglePlayScene" ) 
        { 
            GameManager_S._instance._monsterCount += 1;
        }

        // 하위의 모든 매터리얼 구하기
        _renderers = new List<Renderer>();
        Transform[] underTransforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < underTransforms.Length; i++)
        {
            Renderer renderer = underTransforms[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                _renderers.Add(renderer);
                if (renderer.material.color == null) Debug.Log("색이 널");
            }
        }
    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range; //random point in a sphere 
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            //the 1.0f is the max distance from the random point to a point on the navmesh, might want to increase if range is big
            //or add a for loop like in the documentation
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    void Update()
    {
        if (_isDead) return;

        FieldOfViewCheck(); // 시야에 플레이어 있는지 확인

        switch (_state)
        {
            case Define.MonsterState.Idle:
                StartCoroutine(Idle());
                Debug.Log("Monster Idle");
                break;
            case Define.MonsterState.Patrol:
                StartCoroutine(Patrol());
                Debug.Log("Monster Patrol");
                break;
            case Define.MonsterState.Chase:
                StartCoroutine(Chase());
                Debug.Log("Monster Chase");
                break;
            case Define.MonsterState.Attack:
                StartCoroutine(Attack());
                Debug.Log("Monster Attack!");
                break;
            case Define.MonsterState.Hit:
                break;
        }

    }

    void FieldOfViewCheck() // 시야
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, _radius, _targetMask);

        if (rangeChecks.Length != 0)
        {
            Transform findTarget = rangeChecks[0].transform;
            Vector3 directionToTarget = (findTarget.position - transform.position).normalized;

            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            if (angleToTarget < _angle / 2 || angleToTarget > 360 - (_angle / 2)) // 플레이어로부터 부채꼴처럼 볼 수 있게, 270도
            {
                float distanceToTarget = Vector3.Distance(transform.position, findTarget.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, _obstructionMask))
                {
                    _target = findTarget; // 목표 설정
                    _canSeePlayer = true; // 플레이어 감지
                }
                else // 벽 감지한 경우
                {
                    _canSeePlayer = false;
                    _target = null;
                }
            }
            else
            {
                _canSeePlayer = false;
                _target = null;
            }
        }
        else if (_canSeePlayer) // 보고 있다가 시야에서 사라진거
        {
            _canSeePlayer = false;
            _target = null;
        }
    }

    IEnumerator Idle() // 대기
    {
        // 애니메이터 상태 정보 얻기
        AnimatorStateInfo currentAnimStateInfo = _anim.GetCurrentAnimatorStateInfo(0);

        if (!currentAnimStateInfo.IsName("Idle"))
            _anim.Play("Idle", 0, 0);

        yield return new WaitForSeconds(currentAnimStateInfo.length);

        if (_canSeePlayer) // 플레이어 관측
        {
            StopAllCoroutines();
            _nmAgent.SetDestination(_target.position); // 목표 지정
            ChangeState(Define.MonsterState.Chase);
        }
        else
        {
            StopAllCoroutines();
            ChangeState(Define.MonsterState.Patrol);
        }
    }
    IEnumerator Patrol() // 순찰
    {
        Debug.Log("순찰");
        // 애니메이터 상태 정보 얻기
        AnimatorStateInfo currentAnimStateInfo = _anim.GetCurrentAnimatorStateInfo(0);

        if (!currentAnimStateInfo.IsName("Move"))
            _anim.Play("Move", 0, 0);

        // 랜덤하게 순찰 지점 정하기
        if (_nmAgent.remainingDistance <= _nmAgent.stoppingDistance) // 플레이어 못봤을 때
        {
            Vector3 point;
            if (RandomPoint(_centerPoint.position, _range, out point))
            {
                Debug.DrawRay(point, Vector3.up, Color.red, 3.0f); // 갈 지점 표시

                _nmAgent.SetDestination(point);

                yield return null;
            }
        }
        else if(_canSeePlayer && _nmAgent.remainingDistance <= _nmAgent.stoppingDistance) // 공격 범위 안에 있을 때
        {
            StopAllCoroutines(); // 모든 코루틴 종료
            _nmAgent.ResetPath();
            ChangeState(Define.MonsterState.Attack);  // 공격
        }
        else if(_canSeePlayer && _nmAgent.remainingDistance > _nmAgent.stoppingDistance) // 공격범위 밖이면 추격
        {
            StopAllCoroutines(); // 모든 코루틴 종료
            _nmAgent.SetDestination(_target.position); // 목표 지정
            ChangeState(Define.MonsterState.Chase);  // 추격
        }
    }

    IEnumerator Chase() // 추격
    {
        AnimatorStateInfo currentAnimStateInfo = _anim.GetCurrentAnimatorStateInfo(0);

        if (!currentAnimStateInfo.IsName("Move"))
        {
            _anim.Play("Move", 0, 0);
            // SetDestination 을 위해 한 frame을 넘기기위한 코드
            yield return null;
        }

        // 목표까지의 남은 거리가 멈추는 지점보다 작거나 같으면
        if (_canSeePlayer && _nmAgent.remainingDistance <= _nmAgent.stoppingDistance)
        {
            StopAllCoroutines();
            _nmAgent.ResetPath();
            ChangeState(Define.MonsterState.Attack);
        }
        else if(_canSeePlayer) // 목표가 시야에 있는데 계속 움직이면 경로 다시 계산해서 추격
        {
            _nmAgent.SetDestination(_target.position);
        }
        else if (!_canSeePlayer) // 시야에서 사라졌으면 Idle로 전환
        {
            StopAllCoroutines();
            _nmAgent.ResetPath();
            ChangeState(Define.MonsterState.Idle);
            yield return null;
        }
        else
        {
            // 애니메이션의 한 사이클 동안 대기
            yield return new WaitForSeconds(currentAnimStateInfo.length);
        }
    }

    IEnumerator Attack()
    {
        AnimatorStateInfo currentAnimStateInfo = _anim.GetCurrentAnimatorStateInfo(0);
        _nmAgent.isStopped = true;

        if (_target==null) // 추격 대상을 놓치면
        {
            StopAllCoroutines();
            _nmAgent.isStopped = false;
            ChangeState(Define.MonsterState.Patrol); // 순찰
        }
        else _nmAgent.SetDestination(_target.position);

        if (!currentAnimStateInfo.IsName("Attack"))
        {
            _anim.Play("Attack", 0, 0);
            AnimatorStateInfo attackStateInfo = _anim.GetCurrentAnimatorStateInfo(0);
            // SetDestination 을 위해 한 frame을 넘기기위한 코드
            // yield return null;

            //if (_target != null)
            //{
            //    _target.GetComponent<Status>().TakedDamage(_attack);
            //}
        }

        // 시야 범위에서 사라지면
        if (!_canSeePlayer)
        {
            StopAllCoroutines();
            _nmAgent.isStopped = false;
            ChangeState(Define.MonsterState.Patrol); // 순찰
        }
        else if(_canSeePlayer && _nmAgent.remainingDistance > _nmAgent.stoppingDistance)
        {
            _nmAgent.isStopped = false;
            ChangeState(Define.MonsterState.Chase);
        }

        yield return null;
    }

    public IEnumerator OnHit() {
        if (_state != Define.MonsterState.None)
        {
            AnimatorStateInfo currentAnimStateInfo = _anim.GetCurrentAnimatorStateInfo(0);

            if (!currentAnimStateInfo.IsName("Surprised"))
            {
                _anim.Play("Surprised", 0, 0);
                currentAnimStateInfo = _anim.GetCurrentAnimatorStateInfo(0);
                // SetDestination 을 위해 한 frame을 넘기기위한 코드
                yield return new WaitForSeconds(currentAnimStateInfo.length);
            }
            if (Hp > 0)
            {
                ChangeState(Define.MonsterState.Attack);
            }
            //else
            //{
            //    Dead();
            //}
        }
    }

    public void HitChangeMaterials() // 외부에서 색만 바꾸려고 할 때 사용
    {
        for (int i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].material.color = Color.red;
            Debug.Log("색 변동");
            Debug.Log(_renderers[i].material.name);
        }

        StartCoroutine(ResetMaterialAfterDelay(0.5f));
    }

    IEnumerator ResetMaterialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Color originColor = new Color(0xF6 / 255f, 0xC6 / 255f, 0xFE / 255f);
        for (int i = 0; i < _renderers.Count; i++)
            _renderers[i].material.color = originColor;
    }


    void ChangeState(Define.MonsterState newState)
    {
        _state = newState;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_state == Define.MonsterState.None ) return;
        if(SceneManager.GetActiveScene().name == "SinglePlayScene" )
        {
            return;
        }

        // 태그가 무기 태그인 경우
        if (other.tag == "Melee" || other.tag == "Gun")
        {
            HitStart();
        }
        else 
            Debug.Log("닿지 않음");
    }

    public void Dead()
    {
        _nmAgent.ResetPath(); // 비활성화 되기 전에 해주기
        if (_state != Define.MonsterState.None && Hp <= 0)
        {
            _state = Define.MonsterState.None; // 시체처리
            _collider.enabled = false;
            _isDead = true;

            if (SceneManager.GetActiveScene().name == "SinglePlayScene")
            {
                GameManager_S._instance._monsterCount -= 1;
                GameManager_S._instance.Score += 1;
            }
            
            _anim.Play("Die", 0, 0);
            StartCoroutine(DeadSinkCoroutine());
        }
    }

    IEnumerator DeadSinkCoroutine()
    {
        Debug.Log("시체 처리 시작");
        _nmAgent.enabled = false; // 죽었을 때 비활성화 시켜야 오류 안생김
        yield return new WaitForSeconds(3f);
        while (transform.position.y > -1.5f)
        {
            Debug.Log("가라앉는중");
            transform.Translate(Vector3.down * 0.05f * Time.deltaTime);
            yield return null;
        }
    }

    public void SetManagedPool(IObjectPool<Monster> pool)
    {
        _managedPool = pool;
        Debug.Log("SetManagedPool 실행");
    }

    /// <summary>
    /// 데미지 입기
    /// </summary>
    /// <param name="attack"> 가할 공격력 </param>
    public void TakedDamage(int attack)
    {
        if (_state == Define.MonsterState.None) return; // 시체일 경우 종료

        // 피해가 음수라면 회복되는 현상이 일어나므로 피해의 값을 0이상으로 되게끔 설정
        float damage = Mathf.Max(0, attack);
        Hp -= damage;
        HitChangeMaterials();
        Debug.Log(gameObject.name + "(이)가 " + damage + " 만큼 피해를 입었음!");
        Debug.Log("남은 체력: " + Hp);
        if (Hp<=0)
        {
            Dead();
        }
    }

    void OnTakeDamage(AnimationEvent animationEvent)
    {
        if (_target != null)
        {
            _target.GetComponent<PlayerStatus_S>().TakedDamage(_attack);
        }
    }
    public void HitStart()
    {
        if (Hp > 0)
            {
                
                if (_state != Define.MonsterState.Hit)
                {
                    ChangeState(Define.MonsterState.Hit);
                    StartCoroutine(OnHit());
                }
            }
    }
}