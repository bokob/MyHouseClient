using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

public class ModifiedMonster_S : MonoBehaviour
{
    [SerializeField]
    private IObjectPool<ModifiedMonster_S> _managedPool;

    NavMeshAgent _nmAgent;
    Animator _anim;
    MonsterStatus_S _status;
    MonsterController_S _mContoller;

    // ���� ����
    public Transform _centerPoint;  // ���� ��ġ ���� ������
    public float _range = 10f;            // ���� ��ġ ���� ����
    public float _patrolSpeed = 1f; // ���� �ӵ�

    // ���� ����
    public float Hp { get; private set; } = 100f;                   // ü��
    public int _attack { get; private set; } = 30;                   // ���ݷ�
    public Define.MonsterState _state = Define.MonsterState.Patrol; // ���� ����
    public bool _isDead = false;

    // �� �þ� ����
    public float _radius;              // �þ� ����
    [Range(0, 360)]
    public float _angle;               // �þ߰�
    public LayerMask _targetMask;      // ��ǥ
    public LayerMask _obstructionMask; // ��ֹ�
    public bool _canSeePlayer;

    // �߰� ����
    public float _chaseRange = 10f; // �߰� ����
    public float _lostDistance; // ��ġ�� �Ÿ�

    // ���� ����
    public float _attackRange = 0.1f; // ���� ����
    public float _attackDelay = 2f; // ���� ����
    float nextAttackTime = 0f;

    // ���� ī��Ʈ ����

    //public int _monsterCount = 0;

    public Transform _target; // 목표

    List<Renderer> _renderers; // ���� �Ծ��� �� ������ �� ��ȯ�� ����� ����Ʈ
    List<Color> _originColors;

    void Awake()
    {
        //MonsterInit(); // ���� ����
    }

    private void Start()
    {
        MonsterInit();
    }

    void MonsterInit()
    {
        Debug.Log("����");
        _anim = GetComponent<Animator>();
        _nmAgent = GetComponent<NavMeshAgent>();
        _mContoller = transform.parent.parent.GetComponent<MonsterController_S>();
        _centerPoint = transform;
        _status = GetComponent<MonsterStatus_S>();
        _status.Hp = Hp;
        _mContoller._monsterCount += 1;
        

        // ������ ��� ���͸��� ���ϱ�
        _renderers = new List<Renderer>();
        Transform[] underTransforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < underTransforms.Length; i++)
        {
            Renderer renderer = underTransforms[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                _renderers.Add(renderer);
                if (renderer.material.color == null) Debug.Log("�� ���� ��?");
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

        FieldOfViewCheck(); // �þ߿� �÷��̾� �ִ��� Ȯ��

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

    void FieldOfViewCheck() // �þ�
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, _radius, _targetMask);

        if (rangeChecks.Length != 0)
        {
            Transform findTarget = rangeChecks[0].transform;
            Vector3 directionToTarget = (findTarget.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < _angle / 2) // �÷��̾�κ��� ��ä��ó�� �� �� �ְ�
            {
                float distanceToTarget = Vector3.Distance(transform.position, findTarget.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, _obstructionMask))
                {
                    _target = findTarget; // ��ǥ ����
                    _canSeePlayer = true; // �÷��̾� ����
                }
                else // �� ������ ���
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
        else if (_canSeePlayer) // ���� �ִٰ� �þ߿��� �������
        {
            _canSeePlayer = false;
            _target = null;
        }
    }

    IEnumerator Idle() // ���
    {
        // �ִϸ����� ���� ���� ���
        AnimatorStateInfo currentAnimStateInfo = _anim.GetCurrentAnimatorStateInfo(0);

        if (!currentAnimStateInfo.IsName("Idle"))
            _anim.Play("Idle", 0, 0);

        yield return new WaitForSeconds(currentAnimStateInfo.length);

        if (_canSeePlayer) // �÷��̾� ����
        {
            StopAllCoroutines();
            _nmAgent.SetDestination(_target.position); // ��ǥ ����
            ChangeState(Define.MonsterState.Chase);
        }
        else
        {
            StopAllCoroutines();
            ChangeState(Define.MonsterState.Patrol);
        }
    }
    IEnumerator Patrol() // ����
    {
        Debug.Log("����");
        // �ִϸ����� ���� ���� ���
        AnimatorStateInfo currentAnimStateInfo = _anim.GetCurrentAnimatorStateInfo(0);

        if (!currentAnimStateInfo.IsName("Move"))
            _anim.Play("Move", 0, 0);

        // �����ϰ� ���� ���� ���ϱ�
        if (_nmAgent.remainingDistance <= _nmAgent.stoppingDistance) // �÷��̾� ������ ��
        {
            Vector3 point;
            if (RandomPoint(_centerPoint.position, _range, out point))
            {
                Debug.DrawRay(point, Vector3.up, Color.red, 3.0f); // �� ���� ǥ��

                _nmAgent.SetDestination(point);

                yield return null;
            }
        }
        else if(_canSeePlayer && _nmAgent.remainingDistance <= _nmAgent.stoppingDistance) // ���� ���� �ȿ� ���� ��
        {
            StopAllCoroutines(); // ��� �ڷ�ƾ ����
            _nmAgent.ResetPath();
            ChangeState(Define.MonsterState.Attack);  // ����
        }
        else if(_canSeePlayer && _nmAgent.remainingDistance > _nmAgent.stoppingDistance) // ���ݹ��� ���̸� �߰�
        {
            StopAllCoroutines(); // ��� �ڷ�ƾ ����
            _nmAgent.SetDestination(_target.position); // ��ǥ ����
            ChangeState(Define.MonsterState.Chase);  // �߰�
        }
    }

    IEnumerator Chase() // �߰�
    {
        AnimatorStateInfo currentAnimStateInfo = _anim.GetCurrentAnimatorStateInfo(0);

        if (!currentAnimStateInfo.IsName("Move"))
        {
            _anim.Play("Move", 0, 0);
            // SetDestination �� ���� �� frame�� �ѱ������ �ڵ�
            yield return null;
        }

        // ��ǥ������ ���� �Ÿ��� ���ߴ� �������� �۰ų� ������
        if (_canSeePlayer && _nmAgent.remainingDistance <= _nmAgent.stoppingDistance)
        {
            StopAllCoroutines();
            _nmAgent.ResetPath();
            ChangeState(Define.MonsterState.Attack);
        }
        else if(_canSeePlayer) // ��ǥ�� �þ߿� �ִµ� ��� �����̸� ��� �ٽ� ����ؼ� �߰�
        {
            _nmAgent.SetDestination(_target.position);
        }
        else if (!_canSeePlayer) // �þ߿��� ��������� Idle�� ��ȯ
        {
            StopAllCoroutines();
            _nmAgent.ResetPath();
            ChangeState(Define.MonsterState.Idle);
            yield return null;
        }
        else
        {
            // �ִϸ��̼��� �� ����Ŭ ���� ���
            yield return new WaitForSeconds(currentAnimStateInfo.length);
        }
    }

    IEnumerator Attack()
    {
        AnimatorStateInfo currentAnimStateInfo = _anim.GetCurrentAnimatorStateInfo(0);
        _nmAgent.isStopped = true;

        if (_target==null) // ��ǥ���� �������
        {
            StopAllCoroutines();
            _nmAgent.isStopped = false;
            ChangeState(Define.MonsterState.Patrol); // ����
        }
        else _nmAgent.SetDestination(_target.position);

        if (!currentAnimStateInfo.IsName("Attack"))
        {
            _anim.Play("Attack", 0, 0);
            AnimatorStateInfo attackStateInfo = _anim.GetCurrentAnimatorStateInfo(0);
            // SetDestination �� ���� �� frame�� �ѱ������ �ڵ�
            // yield return null;

            //if (_target != null)
            //{
            //    _target.GetComponent<Status>().TakedDamage(_attack);
            //}
        }

        // �þ� �������� �������
        if (!_canSeePlayer)
        {
            StopAllCoroutines();
            _nmAgent.isStopped = false;
            ChangeState(Define.MonsterState.Patrol); // ����
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
                // SetDestination �� ���� �� frame�� �ѱ������ �ڵ�
                HitChangeMaterials(other); // ���͸��� ��ȯ
                yield return new WaitForSeconds(currentAnimStateInfo.length);
            }

            Debug.Log("���ݹ��� ���� ü��:" + _status.Hp);

            if (_status.Hp <= 0)
                Dead();
            else
                ChangeState(Define.MonsterState.Attack);
        }
    }

    public void HitChangeMaterials(Collider other)
    {
        if (other.tag == "Melee" || other.tag == "Gun")
        {
            for (int i = 0; i < _renderers.Count; i++)
            {
                _renderers[i].material.color = Color.red;
                Debug.Log("�����Ѵ�.");
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
        //if (_state == Define.MonsterState.None) return;

        // �±װ� ���� �±��� ���
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
            Debug.Log("�� �ȴ���?");
    }

    public void Dead()
    {
        if (_state != Define.MonsterState.None && _status.Hp <= 0)
        {
            _isDead = true;
            _mContoller._monsterCount -= 1;
            _mContoller._score += 1;
            _nmAgent.ResetPath();
            _anim.Play("Die", 0, 0);
            _state = Define.MonsterState.None; // ��ü
            StartCoroutine(DeadSinkCoroutine());
        }

        _managedPool.Release(this);
    }

    IEnumerator DeadSinkCoroutine()
    {
        Debug.Log("��üó��");
        _nmAgent.enabled = false; // ���ϸ� ������ �ȳ�������
        yield return new WaitForSeconds(3f);
        while (transform.position.y > -1.5f)
        {
            Debug.Log("�������� ������");
            transform.Translate(Vector3.down * 0.1f * Time.deltaTime);
            yield return null;
        }
    }

    void OnTakeDamage(AnimationEvent animationEvent)
    {
        if (_target != null)
        {
            _target.GetComponent<PlayerStatus_S>().TakedDamage(_attack);

            if(_target.GetComponent<PlayerStatus_S>()!=null)
            {
                _target.GetComponent<PlayerStatus_S>().HitChangeMaterials();
            }
            if (_target.GetComponent<Person>() != null)
            {
                _target.GetComponent<Person>().HitChangeMaterials();
            }
        }
    }

    public void SetManagedPool(IObjectPool<ModifiedMonster_S> pool)
    {
        _managedPool = pool;
        Debug.Log("SetManagedPool ȣ���");
    }
}
