using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 근거리, 원거리 무기들이 상속받는 일반화된 무기 클래스
/// </summary>
public class Weapon : MonoBehaviourPunCallbacks
{
    public Define.Type Type { get; set; } // 무기 타입

    public Transform Master { get; set; } // 주인

    public int Attack { get; set; }       // 공격력
    public float Rate { get; set; } = 0.5f;      // 공격속도
    public float Range { get; set; } = 5f;


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
    /// 최상위 부모를 주인으로 기록하는 메서드
    /// </summary>
    public void RecordMaster()
    {
        // 최상위 부모를 Master로 설정
        Master = transform.root.GetChild(2);
        Debug.Log("무기 주인: " + Master.name);
    }
}