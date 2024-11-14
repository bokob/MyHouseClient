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

    #region ���� �� �ɷ�ġ ����
    public bool _isDead = false;
    public bool _isHit = false;
    List<Renderer> _renderers; // ���� �Ծ��� �� ������ �� ��ȯ�� ����� ����Ʈ
    List<Color> _originColors;

    public Define.Role Role { get; set; }
    [field: SerializeField] public float Hp { get; set; } = 300f;   // ü��
    public float Sp { get; set; }
    public float MaxHp { get; set; }
    public float MaxSp { get; set; }
    public float Defence { get; set; }
    [field: SerializeField] public int _attack { get; private set; } = 30;  // ���ݷ�
    #endregion

    #region �þ� ����
    public float _radius;              // �þ� ����
    [Range(0, 360)]
    public float _angle;               // �þ߰�
    public LayerMask _targetMask;      // ��ǥ
    public LayerMask _obstructionMask; // ��ֹ�
    public bool CanSeePlayer { get; private set; }
    #endregion

    #region �߰� ����
    public float _chaseRange = 10f; // �߰� ����
    public float _lostDistance; // ��ġ�� �Ÿ�
    #endregion

    #region ���� �� ���� ����
    public NavMeshAgent Agent { get; private set; }
    public Transform Target { get; private set; } = null; // ��ǥ
    #endregion

    public bool _isTakingDamage = false; // �̱� �ߺ� Ÿ�� ������
    public int _monsterCount = 0; // ���� ��

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

        // ������ ��� ���͸��� ���ϱ�
        _renderers = new List<Renderer>();
        Transform[] underTransforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < underTransforms.Length; i++)
        {
            Renderer renderer = underTransforms[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                _renderers.Add(renderer);
                if (renderer.material.color == null) Debug.Log("���� ��");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        FieldOfViewCheck();
    }

    void FieldOfViewCheck() // �þ�
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, _radius, _targetMask);

        if (rangeChecks.Length != 0)
        {
            Transform findTarget = rangeChecks[0].transform;
            Vector3 directionToTarget = (findTarget.position - transform.position).normalized;

            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            if (angleToTarget < _angle / 2 || angleToTarget > 360 - (_angle / 2)) // �÷��̾�κ��� ��ä�� ���, 270���� ����
            {
                float distanceToTarget = Vector3.Distance(transform.position, findTarget.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, _obstructionMask))
                {
                    Target = findTarget; // ��ǥ ����
                    CanSeePlayer = true; // �÷��̾� ����
                }
                else // �� ������ ���
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
        else if (CanSeePlayer) // ���� �ִٰ� �þ߿��� �������
        {
            CanSeePlayer = false;
            Target = null;
        }
    }

    public void TakedDamage(int attack) // ������ �ޱ�
    {
        if (Hp <= 0) return; // ��ü�� ��� ����

        // ���ذ� ������� ȸ���Ǵ� ������ �Ͼ�Ƿ� ������ ���� 0�̻����� �ǰԲ� ����
        float damage = Mathf.Max(0, attack);
        Hp -= damage;
        _anim.SetTrigger("setHit");
        HitChangeMaterials();
        Debug.Log(gameObject.name + "(��)�� " + damage + " ��ŭ ���ظ� �Ծ���!");
        Debug.Log("���� ü��: " + Hp);
        if (!_isDead && Hp <= 0)
        {
            Dead();
        }
    }

    public void HitChangeMaterials() // �ܺο��� ���� �ٲٷ��� �� �� ���
    {
        for (int i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].material.color = Color.red;
            Debug.Log("�� ����");
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
        Agent.ResetPath(); // ��Ȱ��ȭ �Ǳ� ���� ���� �ִ� ��� ����
        _collider.enabled = false;
        _anim.SetTrigger("setDie");
        StartCoroutine(DeadSinkCoroutine());
    }

    IEnumerator DeadSinkCoroutine()
    {
        Debug.Log("��ü ó�� ����");
        Agent.enabled = false; // �׾��� �� ��Ȱ��ȭ ���Ѿ� ���� �Ȼ���
        yield return new WaitForSeconds(3f);
        while (transform.position.y > -5f)
        {
            Debug.Log("����ɴ���");
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