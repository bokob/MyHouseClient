using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus_S : MonoBehaviour
{
    #region ���� �� �ɷ�ġ
    [field: SerializeField] public Define.Role Role = Define.Role.None;
    [field: SerializeField] public float Hp { get; set; } = 100;    // ü��
    [field: SerializeField] public float Sp { get; set; } = 100;    // ���׹̳�
    [field: SerializeField] public float MaxHp { get; private set; } = 100; // �ִ� ü��
    [field: SerializeField] public float MaxSp { get; private set; } = 100; // �ִ� ���׹̳�
    [field: SerializeField] public float Defence { get; private set; } = 1; // ����
    #endregion

    #region �ִϸ��̼� �� ����
    Animator _animator;
    List<Renderer> _renderers;
    #endregion

    [Header("EndUI")]
    public int score = 0;
    public float endTime = 6f;
    public float fadeDuration = 4.0f;
    public GameObject endUI;
    public Image fadeImage;
    public TextMeshProUGUI endText;
    public TextMeshProUGUI quitText;
    bool _dead;

    WeaponManager_S _weaponManager_S;
    private GameObject nearMeleeObject;
    private string meleeItemName;

    void Awake()
    {
        _animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
        InitRole();
        endUI.SetActive(false);

        _weaponManager_S = transform.root.GetComponentInChildren<WeaponManager_S>();

        // ���� ��������
        _renderers = new List<Renderer>();
        Transform[] underTransforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < underTransforms.Length; i++)
        {
            Renderer renderer = underTransforms[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                _renderers.Add(renderer);
                // if (renderer.material.color == null) Debug.Log("�� ���� ��?");
            }
        }
    }

    void Update()
    {
        Dead();
        if(_dead)
        {
            endTime -= Time.deltaTime;
            quitText.text = Mathf.FloorToInt(endTime) + " seconds to quit.";
        }

        if (Input.GetKeyDown(KeyCode.P) && nearMeleeObject != null && _weaponManager_S._selectedWeapon.tag != "Gun")
        {
            GetMeleeItem();
        }
    }

    /// <summary>
    /// ���� �ʱ�ȭ
    /// </summary>
    public void InitRole()
    {
        /*
         TODO
        ȣ��Ʈ��, Houseowner���� �ϰ�, Ŭ���̾�Ʈ�� Robber

        �̱��� �����θ�
         */
        Role = Define.Role.Houseowner;
    }



    /// <summary>
    /// ������ �Ա�
    /// </summary>
    /// <param name="attack"> ���� ���ݷ� </param>
    public void TakedDamage(int attack)
    {
        if (Role == Define.Role.None) return; // 시체일 경우 종료

        // 피해가 음수라면 회복되는 현상이 일어나므로 피해의 값을 0이상으로 되게끔 설정
        float damage = Mathf.Max(0, attack);
        Hp -= damage;
        if (Hp > 0)
        {
            HitChangeMaterials();
            Debug.Log(gameObject.name + "(이)가 " + damage + " 만큼 피해를 입었음!");
            Debug.Log("남은 체력: " + Hp);
        }
        else
        {
            Dead();
        }
    }

    /// <summary>
    /// �ִ� ü���� 0.2��ŭ ȸ��
    /// </summary>
    public void Heal()
    {
        // ���� ü���� �ִ� ü�º��� ���� ���� ȸ�� ����
        if (Hp < MaxHp)
        {
            // ȸ����
            float healAmount = MaxHp * 0.2f;

            // ȸ������ ���� ü�°��� ���� �ִ� ü���� ���� �ʵ��� ����
            float healedAmount = Mathf.Clamp(Hp + healAmount, 0, MaxHp) - Hp;

            Debug.Log("���� ü��" + Hp);
            // ü�� ȸ��
            Hp += healedAmount;
            Debug.Log("ü���� " + healedAmount + "��ŭ ȸ��!");
            Debug.Log("���� ü��: " + Hp);
        }
        else
        {
            Debug.Log("�ִ� ü��. ȸ���� �ʿ� ����.");
        }
    }

    /// <summary>
    /// �ִ� ���׹̳����� ���� ȸ��
    /// </summary>
    public void SpUp()
    {
        // ���� ���׹̳��� �ִ� ���׹̳����� ���� ���� ȸ�� ����
        if (Sp < MaxSp)
        {
            // ȸ������ ���� ���׹̳����� ���� �ִ� ���׹̳��� ���� �ʵ��� ����
            float healedAmount = Mathf.Clamp(Sp + MaxSp, 0, MaxHp) - Sp;

            Debug.Log("���� ���׹̳�" + Sp);
            // ���׹̳� ȸ��
            Sp += healedAmount;
            Debug.Log("���� ȸ��! ���� Sp: " + Sp);
        }
        else
        {
            Debug.Log("�ִ� Sp. ȸ���� �ʿ� ����.");
        }
    }

    /// <summary>
    /// ���׹̳� ��������
    /// </summary>
    public void ChargeSp()
    {
        Sp += Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    /// <summary>
    /// ���׹̳� ���̱�
    /// </summary>
    public void DischargeSp()
    {
        Sp -= Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    /// <summary>
    /// ������, ���׹̳� ����
    /// </summary>
    public void JumpSpDown()
    {
        Sp -= 3;
    }

    /// <summary>
    /// ���� ����
    /// </summary>
    public void DefenceUp()
    {

    }

    /// <summary>
    /// ���
    /// </summary>
    public void Dead()
    {
        if (Role != Define.Role.None && Hp <= 0)
        {
            _dead = true;
            Role = Define.Role.None; // 시체
            _animator.SetTrigger("setDie");
            StartCoroutine(DeadSinkCoroutine());

            // 게임 정산
            endUI.SetActive(true);
            score = GameManager_S._instance._score;
            StartCoroutine(FadeInRoutine());
            endText.text = "Killed Ghost : " + score.ToString();
        }
    }

    /// <summary>
    /// ���� ������
    /// </summary>
    /// <returns></returns>
    IEnumerator DeadSinkCoroutine()
    {
        yield return new WaitForSeconds(5f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// ���� ������ Material �Ӱ� ��ȭ
    /// </summary>
    public void HitChangeMaterials()
    {
        for (int i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].material.color = Color.red;
            Debug.Log("�����Ѵ�.");
            //Debug.Log(_renderers[i].material.name);
        }
        StartCoroutine(ResetMaterialAfterDelay(1.7f));
    }

    /// <summary>
    /// ���� �ް� Material ������� ����
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator ResetMaterialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        for (int i = 0; i < _renderers.Count; i++)
            _renderers[i].material.color = Color.white;
    }

    void OnTriggerEnter(Collider other)
    {
        //// �ڱ� �ڽſ��� ���� ��� ����
        if (other.transform.root.name == gameObject.name) return;

        if (other.tag == "Monster")
            HitChangeMaterials();
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "MeleeItem")
        {
            nearMeleeObject = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "MeleeItem")
        {
            nearMeleeObject = null;
        }
    }

    public void GetMeleeItem()
    {
        meleeItemName = nearMeleeObject.name;
        _weaponManager_S.PickUp(meleeItemName);
        Destroy(nearMeleeObject);
    }

    public void SetRoleAnimator(RuntimeAnimatorController animController, Avatar avatar)
    {
        _animator.runtimeAnimatorController = animController;
        _animator.avatar = avatar;

        // �ִϸ����� �Ӽ� ��ü�ϰ� ���ٰ� �Ѿ� ������
        _animator.enabled = false;
        _animator.enabled = true;
    }

    public void ChangeIsHoldGun(bool isHoldGun)
    {
        if (Role != Define.Role.Houseowner) return;
        _animator.SetBool("isHoldGun", isHoldGun);
    }

    // 게임 오버 화면 서서히 나타나게 하기
    private IEnumerator FadeInRoutine()
    {
        float elapsedTime = 1.0f;
        Color color = fadeImage.color;
        color.a = 0.0f; // 시작 알파 값 (완전히 투명)

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0.0f, 1.0f, elapsedTime / fadeDuration); // ?�파 값을 1?�서 0?�로 ?�서??변�?
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1.0f; // 최종 알파 값 (완전히 불투명)
        fadeImage.color = color;
    }
}
