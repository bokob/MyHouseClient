using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee_S : Weapon
{
    PlayerMove_S _playerMove;
    PlayerInputs _playerInputs;

    BoxCollider _meleeArea;       // ���� ���� ����
    TrailRenderer _trailEffet;    // �ֵθ� �� ȿ��
    Animator _animator;

    [Header("���� ����")]
    bool _isSwingReady;  // ���� �غ�
    float _swingDelay;   // ���� ������
    bool _isStabReady;  // ���� �غ�
    float _stabDelay;   // ���� ������
    [SerializeField] bool isAttack = false;

    #region ���� ȿ�� ����
    public LayerMask _sliceMask; // �ڸ� ����� ���̾� ����ũ
    public float _cutForce = 250f; // �ڸ� �� �������� ��

    Vector3 _entryPoint; // ������Ʈ�� �� ����
    Vector3 _exitPoint; // ������Ʈ�� �հ� ���� ����
    bool _hasExited = false; // ������Ʈ�� �հ� �������� ���θ� �����ϴ� ���� (����� ��)
    #endregion

    void Awake()
    {
        InitWeapon();
    }

    void InitWeapon()
    {
        base.Type = Define.Type.Melee;

        _meleeArea = gameObject.GetComponent<BoxCollider>();
        _trailEffet = gameObject.GetComponentInChildren<TrailRenderer>();

        // ���� ���� �ʱ�ȭ
        WeaponData weapon = GameManager_S._instance.GetWeaponStatusByName(transform.name);
        if (weapon != null)
        {
            Debug.Log($"Weapon Name: {weapon.Name}. Attack: {weapon.Attack}, Rate: {weapon.Rate}");
            Attack = weapon.Attack;
            Rate = weapon.Rate;
            Range = weapon.Range;
        }
        else
        {
            Debug.LogWarning("Weapon not found!");
        }
    }


    void Start()
    {
        _playerMove = transform.root.GetChild(2).GetComponent<PlayerMove_S>();
        _playerInputs = transform.root.GetChild(2).GetComponent<PlayerInputs>();

        if (transform.root.childCount > 2) // �̱� ����
        {
            _animator = transform.root.GetChild(2).GetChild(0).GetComponent<Animator>();
        }

        _meleeArea = gameObject.GetComponent<BoxCollider>();
        _trailEffet = gameObject.GetComponentInChildren<TrailRenderer>();
    }

    void Update()
    {
        _swingDelay += Time.deltaTime;
        _stabDelay += Time.deltaTime;

        _playerInputs.shoot = false;
        _playerInputs.aim = false;
        _playerInputs.reload = false;
    }

    public override void Use()
    {
        _isSwingReady = base.Rate < _swingDelay; // ���ݼӵ��� ���� �����̺��� ������ �����غ� �Ϸ�
        _isStabReady = base.Rate < _stabDelay;

        // ������ Ʈ�������� �����ͼ� ���� ��ġ�� ������ ����
        Transform weaponTransform = transform;  // ������ Ʈ�������� ���� ����ϰų� _weaponTransform ������ ��� ����

        if (_playerInputs == null) _playerInputs = weaponTransform.GetComponentInParent<PlayerInputs>();
        if (_playerMove == null) _playerMove = weaponTransform.GetComponentInParent<PlayerMove_S>();

        if (_playerInputs.swing || _playerInputs.stab)
        {
            if(!_playerMove._grounded) // ������ �ƴϸ� ���� X
            {
                _playerInputs.swing = false;
                _playerInputs.stab = false;
                return;
            }

            StopCoroutine("MeleeAttackEffect");
            if (_playerInputs.swing && _isSwingReady) // �ֵθ���
            {
                _animator.SetBool("isSwing", true);
                _swingDelay = 0;
            }
            else if (_playerInputs.stab && _isStabReady) // ���
            {
                Debug.Log("���");
                _animator.SetBool("isStab", true);
                _stabDelay = 0;
            }
            _playerInputs.swing = false;
            _playerInputs.stab = false;
            StartCoroutine("MeleeAttackEffect");
        }
        else
        {
            // �������ڸ��� �ֵθ��� ���� ����(����Ƽ Play ���� �� Ŭ�� ������)
            _playerInputs.swing = false;
            _playerInputs.stab = false;
        }
    }

    /// <summary>
    /// �ڷ�ƾ���� isAttack, Collider, TrailRenderer Ư�� �ð� ���ȸ� true
    /// </summary>
    IEnumerator MeleeAttackEffect()
    {
        isAttack = true;
        yield return new WaitForSeconds(0.5f);
        _meleeArea.enabled = true;
        _trailEffet.enabled = true;
        SoundManager._instance.PlayEffect("MeleeAttack");   // �ֵθ��� ȿ����

        yield return new WaitForSeconds(0.5f);
        _animator.SetBool("isSwing", false);
        _animator.SetBool("isStab", false);
        _meleeArea.enabled = false;

        yield return new WaitForSeconds(0.5f);
        _trailEffet.enabled = false;
        isAttack = false;
    }

    void OnTriggerEnter(Collider other)
    {
        _hasExited = true;
        
        _entryPoint = other.ClosestPoint(transform.position); // �ڸ��� �����ϴ� ����

        // �ڱ� �ڽſ��� ���� ��� ����
        if (other.CompareTag("Player") && transform.root.GetChild(2) == other.transform.root.GetChild(2)) 
            return;

        if (other.GetComponent<IStatus>() != null && isAttack)
        {
            other.GetComponent<IStatus>().TakedDamage(Attack);
        }
    }

    #region ���� ���
    void OnTriggerStay(Collider other)
    {
        Debug.Log("����");
    }

    // ���� �� �Ǹ� ���̾ ���� ����
    void OnTriggerExit(Collider other)
    {
        _exitPoint = other.ClosestPoint(transform.position); // �ڸ��� ����� ����

        Vector3 cutDirection = _exitPoint - _entryPoint; // �ڸ��� ����
        Vector3 cutInPlane = (_entryPoint + _exitPoint) / 2; // �߰� ���� (����� �߽�)
        Vector3 cutPlaneNormal = Vector3.Cross((_entryPoint - _exitPoint), (_entryPoint - transform.position)).normalized; // ����� ���� ����

        // ������ ��� ���հ��� 0�̱� ������ ���� ���� 0, ���� ���Ƿ� ����
        if (cutPlaneNormal.x == 0 && cutPlaneNormal.y == 0 && cutPlaneNormal.z == 0)
        {
            cutPlaneNormal = (_entryPoint - _exitPoint).normalized; // ���� �ڸ��� ������ normalize �ؼ� �־���� ��

            bool isHorizontalCut = Mathf.Abs(cutDirection.x) > Mathf.Abs(cutDirection.y); // �������� �߶�����
            if (isHorizontalCut) // ���η� �ڸ��� ���
                cutPlaneNormal = Vector3.up; // x �� �������� �ڸ��� ������ ��� ���� �� ������ Vector3.up���� ����
            else // ���η� �ڸ��� ���
                cutPlaneNormal = Vector3.right; // y �� �������� �ڸ��� ������ ��� ���� �� ������ Vector3.right���� ����
        }

        LayerMask cutableMask = 1 << other.gameObject.layer; // ���� ������Ʈ�� ���̾� ����ũ
        if ((_sliceMask.value & cutableMask) != 0)  // �ڸ� �� �ִ� ���̾ ���ԵǾ� �ִٸ�
        {
            Debug.LogWarning("�ڸ� �� �ִ� ������Ʈ");
            Cutter.Cut(other.gameObject, cutInPlane, cutPlaneNormal); // ������Ʈ�� �ڸ���

            // �ڸ� �� �������� ���� �����Ͽ� ������Ʈ�� �о
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddForce(-cutPlaneNormal * _cutForce); // cutDirection ��ſ� cutPlaneNormal�� ���
            
            _hasExited = true;
        }
        else
        {
            Debug.LogWarning("�߸��� ����");
        }
    }
    #endregion
}