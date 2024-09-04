using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class Monster : MonoBehaviour, IStatus
{
    [SerializeField]
    private IObjectPool<Monster> _managedPool;
    Animator _anim;
    CapsuleCollider _collider;

    #region 상태 및 능력치 관련
    public bool _isDead = false;
    List<Renderer> _renderers; // 피해 입었을 때 렌더러 색 변환에 사용할 리스트
    List<Color> _originColors;

    public Define.Role Role { get; set; }
    [field: SerializeField] public float Hp { get; set; } = 300f;   // 체력
    public float Sp { get; set; }
    public float MaxHp { get; set; }
    public float MaxSp { get; set; }
    public float Defence { get; set; }
    [field: SerializeField] public int _attack { get; private set; } = 30;  // 공격력
    #endregion

    #region 시야 관련
    public float _radius;              // 시야 범위
    [Range(0, 360)]
    public float _angle;               // 시야각
    public LayerMask _targetMask;      // 목표
    public LayerMask _obstructionMask; // 장애물
    public bool CanSeePlayer { get; private set; }
    #endregion

    #region 추격 관련
    public float _chaseRange = 10f; // 추격 범위
    public float _lostDistance; // 놓치는 거리
    #endregion

    #region 순찰 및 공격 관련
    public NavMeshAgent Agent { get; private set; }
    public Transform Target { get; private set; } = null; // 목표
    #endregion

    public bool _isTakingDamage = false; // 싱글 중복 타격 방지용
    public int _monsterCount = 0; // 유령 수

    void Awake()
    {
        MonsterInit();
    }

    void MonsterInit()
    {
        _anim = GetComponent<Animator>();
        _collider = GetComponent<CapsuleCollider>();
        Agent = GetComponent<NavMeshAgent>();

        if (SceneManager.GetActiveScene().name == "SinglePlayScene")
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

    // Update is called once per frame
    void Update()
    {
        FieldOfViewCheck();
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
                    Target = findTarget; // 목표 설정
                    CanSeePlayer = true; // 플레이어 감지
                }
                else // 벽 감지한 경우
                {
                    CanSeePlayer = false;
                    Target = null;
                }
            }
            else
            {
                CanSeePlayer = false;
                Target = null;
            }
        }
        else if (CanSeePlayer) // 보고 있다가 시야에서 사라진거
        {
            CanSeePlayer = false;
            Target = null;
        }
    }

    public void TakedDamage(int attack) // 상대에게 데미지 입히기
    {
        if (Hp <= 0) return; // 시체일 경우 종료

        // 피해가 음수라면 회복되는 현상이 일어나므로 피해의 값을 0이상으로 되게끔 설정
        float damage = Mathf.Max(0, attack);
        Hp -= damage;
        HitChangeMaterials();
        Debug.Log(gameObject.name + "(이)가 " + damage + " 만큼 피해를 입었음!");
        Debug.Log("남은 체력: " + Hp);
        if (!_isDead && Hp <= 0)
        {
            Dead();
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

    public void Dead()
    {
        if (SceneManager.GetActiveScene().name == "SinglePlayScene")
        {
            GameManager_S._instance._monsterCount -= 1;
            GameManager_S._instance.Score += 1;
        }

        _isDead = true;
        Agent.ResetPath(); // 비활성화 되기 전에 해주기
        _collider.enabled = false;
        _anim.Play("Die", 0, 0);
        StartCoroutine(DeadSinkCoroutine());
    }

    IEnumerator DeadSinkCoroutine()
    {
        Debug.Log("시체 처리 시작");
        Agent.enabled = false; // 죽었을 때 비활성화 시켜야 오류 안생김
        yield return new WaitForSeconds(3f);
        while (transform.position.y > -5f)
        {
            Debug.Log("가라앉는중");
            transform.Translate(Vector3.down * 0.05f * Time.deltaTime);
            yield return null;
        }
    }

    void OnTakeDamage(AnimationEvent animationEvent)
    {
        if (Target != null)
        {
            Target.GetComponent<IStatus>().TakedDamage(_attack);
        }
    }

    public void SetManagedPool(IObjectPool<Monster> pool)
    {
        _managedPool = pool;
    }
}