using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [field: SerializeField] public Define.Role Role = Define.Role.None;
    [field: SerializeField] public float Hp { get; set; } = 100;    // 체력
    [field: SerializeField] public float Sp { get; set; } = 100;    // 스테미나
    [field: SerializeField] public float MaxHp { get; private set; } = 100; // 최대 체력
    [field: SerializeField] public float MaxSp { get; private set; } = 100; // 최대 스테미나
    [field: SerializeField] public float Defence { get; private set; } = 1; // 방어력

    [SerializeField] NewWeaponManager _houseOwnerWeaponManager;

    void Start()
    {
        InitRole();
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

        // 집주인 무기 세팅
        _houseOwnerWeaponManager.InitRoleWeapon();
        Debug.Log("집주인으로 변신 완료");
    }

    /// <summary>
    /// 역할 초기화
    /// </summary>
    public void InitRole()
    {
        /*
         TODO
        호스트면, Houseowner으로 하고, 클라이언트면 Robber

        일단 편의를 위해 강도롤 시작
         */
        Role = Define.Role.Robber;
    }
}
