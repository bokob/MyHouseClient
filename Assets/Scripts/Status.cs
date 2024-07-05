using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Status : MonoBehaviour
{
    public float Hp { get; set; } = 100;    // 체력
    public float Sp { get; set; } = 100;    // 스테미나
    public float MaxHp { get; private set; } = 100; // 최대 체력
    public float MaxSp { get; private set; } = 100; // 최대 스테미나
    public float Defence { get; private set; } = 1; // 방어력

    /// <summary>
    /// 데미지 처리 함수
    /// </summary>
    public void TakedDamage(int attack)
    {
        // 피해가 음수라면 회복되는 현상이 일어나므로 피해의 값을 0이상으로 되게끔 설정
        float damage = Mathf.Max(0, attack - Defence);
        Hp -= damage;

        Debug.Log(gameObject.name + "(이)가 " + damage + " 만큼 피해를 입었음!");
        Debug.Log("남은 체력: " + Hp);
    }

    // 최대 체력의 0.2만큼 회복
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

    // 최대 스테미나 전부 회복
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

    public void ChargeSp()
    {
        Sp += Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    public void DischargeSp()
    {
        Sp -= Time.deltaTime * 20;
        Sp = Mathf.Clamp(Sp, 0, MaxSp);
    }

    public void JumpSpDown()
    {
        Sp -= 3;
    }

    public void DefenceUp()
    {

    }
}
