using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusItem : Item
{
    [SerializeField]
    float _pickupRange;
    void Start()
    {
        base.ItemInit();
        _pickupRange = 2f;
    }

    // Update is called once per frame
    void Update()
    {
        base.Floating();
    }

    /// <summary>
    /// 상태 관련 아이템 획득
    /// </summary>
    /// <param name="other"></param>
    void TakeStatusItem(Collider other)
    {
        PlayerStatus status = other.GetComponent<PlayerStatus>();
        if (base.itemType == Define.Item.Heart)
        {
            if ((int)status.Hp == (int)status.MaxHp)
                return;
            status.Heal();
        }
        else if (base.itemType == Define.Item.Energy)
        {
            if ((int)status.Sp == (int)status.MaxSp)
                return;
            status.SpUp();
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// 줍기
    /// </summary>
    /// <param name="other"></param>
    void PickUp(Collider other)
    {
        Debug.Log("아이템 줍기");
        TakeStatusItem(other);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("아이템이 사정거리 안에 존재");
        PickUp(other);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _pickupRange);
    }
}
