using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    NavMeshAgent _nmAgent;
    Animator _anim;
    Status _status;

    // 순찰 관련
    public Transform _centerPoint;  // 순찰 위치 정할 기준점
    public float _range;            // 순찰 위치 정할 범위
    public float _patrolSpeed = 1f; // 순찰 속도

    // 상태 관련
    public float Hp { get; private set; } = 300f;                   // 체력
    public int _attack { get; private set; } = 30;                   // 공격력
    public Define.MonsterState _state = Define.MonsterState.Patrol; // 현재 상태
    public bool _isDead = false;

    // 적 시야 관련
    public float _radius;              // 시야 범위
    [Range(0, 360)]
    public float _angle;               // 시야각
    public LayerMask _targetMask;      // 목표
    public LayerMask _obstructionMask; // 장애물
    public bool _canSeePlayer;

    // 추격 관련
    public float _chaseRange = 10f; // 추격 범위
    public float _lostDistance; // 놓치는 거리

    // 공격 관련
    public float _attackRange = 0.1f; // 공격 범위
    public float _attackDelay = 2f; // 공격 간격
    float nextAttackTime = 0f;

    public Transform _target = null; // 목표

    List<Renderer> _renderers; // 피해 입었을 때 렌더러 색 변환에 사용할 리스트
    List<Color> _originColors;

    void Awake()
    {
        MonsterInit(); // 몬스터 세팅
    }

    void MonsterInit()
    {
        Debug.Log("시작");
        _anim = GetComponent<Animator>();
        _nmAgent = GetComponent<NavMeshAgent>();
        _centerPoint = transform;
        _status = GetComponent<Status>();
        _status.Hp = Hp;

        // 하위의 모든 매터리얼 구하기
        _renderers = new List<Renderer>();
        Transform[] underTransforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < underTransforms.Length; i++)
        {
            Renderer renderer = underTransforms[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                _renderers.Add(renderer);
                if (renderer.material.color == null) Debug.Log("왜 색이 널?");
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
                break;
            case Define.MonsterState.Patrol:
                StartCoroutine(Patrol());
                break;
            case Define.MonsterState.Chase:
                StartCoroutine(Chase());
                break;
            case Define.MonsterState.Attack:
                StartCoroutine(Attack());
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

            if (Vector3.Angle(transform.forward, directionToTarget) < _angle / 2) // 플레이어로부터 부채꼴처럼 볼 수 있게
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

        if (_target==null) // 목표물이 사라지면
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

    IEnumerator OnHit(Collider other) {
        if (_state != Define.MonsterState.None)
        {
            AnimatorStateInfo currentAnimStateInfo = _anim.GetCurrentAnimatorStateInfo(0);

            if (!currentAnimStateInfo.IsName("Surprised"))
            {
                _anim.Play("Surprised", 0, 0);
                currentAnimStateInfo = _anim.GetCurrentAnimatorStateInfo(0);
                // SetDestination 을 위해 한 frame을 넘기기위한 코드
                HitChangeMaterials(other); // 매터리얼 변환
                yield return new WaitForSeconds(currentAnimStateInfo.length);
            }

            Debug.Log("공격받은 측의 체력:" + _status.Hp);

            if (_status.Hp <= 0)
                Dead();
            else
                ChangeState(Define.MonsterState.Attack);
        }
    }

    public void HitChangeMaterials(Collider other)
    {
        // 태그가 무기 또는 몬스터
        if (other.tag == "Melee" || other.tag == "Gun")
        {
            for (int i = 0; i < _renderers.Count; i++)
            {
                _renderers[i].material.color = Color.red;
                Debug.Log("색변한다.");
                Debug.Log(_renderers[i].material.name);
            }

            StartCoroutine(ResetMaterialAfterDelay(0.5f));
        }
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
        if (_state == Define.MonsterState.None) return;

        // 태그가 무기 태그인 경우
        if (other.tag == "Melee" || other.tag == "Gun")
        {
            if (Hp > 0)
            {
                StopAllCoroutines();
                ChangeState(Define.MonsterState.Hit);
                StartCoroutine(OnHit(other));
            }
        }
        else 
            Debug.Log("왜 안닿지?");
    }

    public void Dead()
    {
        if (_state != Define.MonsterState.None && _status.Hp <= 0)
        {
            _isDead = true;
            _nmAgent.ResetPath();
            _anim.Play("Die", 0, 0);
            _state = Define.MonsterState.None; // 시체
            StartCoroutine(DeadSinkCoroutine());
        }
    }

    IEnumerator DeadSinkCoroutine()
    {
        Debug.Log("시체처리");
        _nmAgent.enabled = false; // 안하면 밑으로 안내려가짐
        yield return new WaitForSeconds(3f);
        while (transform.position.y > -1.5f)
        {
            Debug.Log("땅속으로 들어가는중");
            transform.Translate(Vector3.down * 0.1f * Time.deltaTime);
            yield return null;
        }
        Destroy(gameObject);
    }

    void OnTakeDamage(AnimationEvent animationEvent)
    {
        if (_target != null)
        {
            _target.GetComponent<Status>().TakedDamage(_attack);

            if(_target.GetComponent<PlayerController>()!=null)
            {
                _target.GetComponent<PlayerController>().HitChangeMaterials();
            }
            if (_target.GetComponent<Person>() != null)
            {
                _target.GetComponent<Person>().HitChangeMaterials();
            }
        }
    }
}