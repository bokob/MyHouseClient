using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus_S : MonoBehaviour
{
    #region 상태 및 능력치
    [field: SerializeField] public Define.Role Role = Define.Role.None;
    [field: SerializeField] public float Hp { get; set; } = 100;    // 체력
    [field: SerializeField] public float Sp { get; set; } = 100;    // 스테미나
    [field: SerializeField] public float MaxHp { get; private set; } = 100; // 최대 체력
    [field: SerializeField] public float MaxSp { get; private set; } = 100; // 최대 스테미나
    [field: SerializeField] public float Defence { get; private set; } = 1; // 방어력
    #endregion

    #region 애니메이션 및 피해
    Animator _animator;
    List<Renderer> _renderers;
    #endregion

    [Header("EndUI")]
    public int score = 0;
    public GameObject endUI;
    public TextMeshProUGUI endText;
    

    void Awake()
    {
        _animator = GetComponent<Animator>();
        InitRole();
        endUI.SetActive(false);
        
        // 렌더 가져오기
        _renderers = new List<Renderer>();
        Transform[] underTransforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < underTransforms.Length; i++)
        {
            Renderer renderer = underTransforms[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                _renderers.Add(renderer);
                // if (renderer.material.color == null) Debug.Log("왜 색이 널?");
            }
        }
    }

    private void Start() {
        TransformIntoHouseowner();
    }

    void Update()
    {
        Dead();
        //TransformIntoHouseowner();
    }

    /// <summary>
    /// 역할 초기화
    /// </summary>
    public void InitRole()
    {
        /*
         TODO
        호스트면, Houseowner으로 하고, 클라이언트면 Robber

        싱글은 집주인만
         */
        Role = Define.Role.Houseowner;
    }



    /// <summary>
    /// 데미지 입기
    /// </summary>
    /// <param name="attack"> 가할 공격력 </param>
    public void TakedDamage(int attack)
    {
        // 피해가 음수라면 회복되는 현상이 일어나므로 피해의 값을 0이상으로 되게끔 설정
        float damage = Mathf.Max(0, attack - Defence);
        Hp -= damage;

        Debug.Log(gameObject.name + "(이)가 " + damage + " 만큼 피해를 입었음!");
        Debug.Log("남은 체력: " + Hp);
    }

    /// <summary>
    /// 최대 체력의 0.2만큼 회복
    /// </summary>
    public void Heal()
    {
        // 현재 체력이 최대 체력보다 작을 때만 회복 적용
        if (Hp < MaxHp)
        {
            // 회복량
            float healAmount = MaxHp * 0.2f;

            // 회복량과 현재 체력과의 합이 최대 체력을 넘지 않도록 조절
            float healedAmount = Mathf.Clamp(Hp + healAmount, 0, MaxHp) - Hp;

            Debug.Log("이전 체력" + Hp);
            // 체력 회복
            Hp += healedAmount;
            Debug.Log("체력을 " + healedAmount + "만큼 회복!");
            Debug.Log("현재 체력: " + Hp);
        }
        else
        {
            Debug.Log("최대 체력. 회복할 필요 없음.");
        }
    }

    /// <summary>
    /// 최대 스테미나까지 전부 회복
    /// </summary>
    public void SpUp()
    {
        // 현재 스테미나가 최대 스테미나보다 작을 때만 회복 적용
        if (Sp < MaxSp)
        {
            // 회복량과 현재 스테미나와의 합이 최대 스테미나를 넘지 않도록 조절
            float healedAmount = Mathf.Clamp(Sp + MaxSp, 0, MaxHp) - Sp;

            Debug.Log("이전 스테미나" + Sp);
            // 스테미나 회복
            Sp += healedAmount;
            Debug.Log("전부 회복! 현재 Sp: " + Sp);
        }
        else
        {
            Debug.Log("최대 Sp. 회복할 필요 없음.");
        }
    }

    /// <summary>
    /// 스테미나 차오르기
    /// </summary>
    public void ChargeSp()
    {
        Sp += Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    /// <summary>
    /// 스테미나 깎이기
    /// </summary>
    public void DischargeSp()
    {
        Sp -= Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    /// <summary>
    /// 점프시, 스테미나 감소
    /// </summary>
    public void JumpSpDown()
    {
        Sp -= 3;
    }

    /// <summary>
    /// 방어력 증가
    /// </summary>
    public void DefenceUp()
    {

    }

    /// <summary>
    /// 집주인으로 변신
    /// </summary>
    public void TransformIntoHouseowner()
    {
        transform.GetChild(0).gameObject.SetActive(false); // 강도 비활성화
        transform.GetChild(1).gameObject.SetActive(true);  // 집주인 활성화
        Role = Define.Role.Houseowner;

        Debug.Log("현재 상태: " + Role);

        Camera.main.gameObject.GetComponent<CameraController_S>().SetHouseownerView(); // 집주인 시점으로 설정

        Debug.Log("집주인 시점을 변환");

        Debug.Log("집주인으로 변신 완료");
    }

    
    /// <summary>
    /// 사망
    /// </summary>
    public void Dead()
    {
        if (Role != Define.Role.None && Hp <= 0)
        {
            endUI.SetActive(true);
            endText.text = score.ToString();
            _animator.SetTrigger("setDie");
            Role = Define.Role.None; // 시체
            StartCoroutine(DeadSinkCoroutine());
        }
    }

    /// <summary>
    /// 시체 바닥으로 가라앉기
    /// </summary>
    /// <returns></returns>
    IEnumerator DeadSinkCoroutine()
    {
        yield return new WaitForSeconds(3f);
        while (transform.position.y > -1.5f)
        {
            transform.Translate(Vector3.down * 0.1f * Time.deltaTime);
            yield return null;
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// 피해 받으면 Material 붉게 변화
    /// </summary>
    public void HitChangeMaterials()
    {
        // 태그가 무기 또는 몬스터

        for (int i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].material.color = Color.red;
            Debug.Log("색변한다.");
            //Debug.Log(_renderers[i].material.name);
        }

        StartCoroutine(ResetMaterialAfterDelay(1.7f));

        //Debug.Log($"플레이어가 {other.transform.root.name}에게 공격 받음!");
        Debug.Log("공격받은 측의 체력:" + Hp);
    }

    /// <summary>
    /// 피해 받고 Material 원래대로 복구
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
        //// 자기 자신에게 닿은 경우 무시
        //if (other.transform.root.name == gameObject.name) return;
        if (other.tag == "Melee" || other.tag == "Gun" || other.tag == "Monster")
            HitChangeMaterials();
    }

    public void SetRoleAnimator(RuntimeAnimatorController animController, Avatar avatar)
    {
        _animator.runtimeAnimatorController = animController;
        _animator.avatar = avatar;

        // 애니메이터 속성 교체하고 껐다가 켜야 동작함
        _animator.enabled = false;
        _animator.enabled = true;
    }

    public void ChangeIsHoldGun(bool isHoldGun)
    {
        if (Role != Define.Role.Houseowner) return;
        _animator.SetBool("isHoldGun", isHoldGun);
    }
}
