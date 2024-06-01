using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 근거리, 원거리 무기들이 상속받는 일반화된 무기 클래스
/// </summary>
public class Weapon : MonoBehaviour
{
    public Define.Type Type { get; protected set; } // 무기 타입

    public Transform Master { get; protected set; } // 주인

    public int Attack { get; protected set; }       // 공격력
    public float Rate { get; protected set; } = 0.5f;      // 공격속도


    void Awake()
    {
        RecordMaster();
        // TODO
        /*
         무기가 다양해질 때 무기 이름이나 타입에 따라
         데미지나 공격속도를 세팅하는 작업을 해줘야 함
         */
    }

    /// <summary>
    /// Use() 실행하면서 각 무기에 맞는 공격 효과 코루틴이 같이 실행된다.
    /// </summary>
    public virtual void Use()
    {
        // TODO
        // 무기에 맞는 공격 기능
    }

    /// <summary>
    /// 현재 주인이 누구인지 확인
    /// </summary>
    //public virtual void MasterPerception()
    //{
    //    if (Master != null)
    //    {
    //        Debug.Log("Master: " + Master.name);
    //    }
    //    else
    //    {
    //        Debug.Log("No master assigned.");
    //    }
    //}

    /// <summary>
    /// 최상위 부모를 주인으로 기록하는 메서드
    /// </summary>
    public void RecordMaster()
    {
        Transform current = transform;

        // 최상위 부모까지 탐색
        while (current.parent != null)
        {
            current = current.parent;
        }

        // 최상위 부모를 Master로 설정
        Master = current;
        Debug.Log("무기 주인: " + Master.name);
    }
}