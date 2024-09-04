using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusItem_S : Item
{
    public Define.StatusItem _statusName;

    ItemCylinder_S _itemCylinder; // 아이템 소환될 곳
    void Start()
    {
        _itemType = Define.Item.Status;
        _itemCylinder = transform.parent.parent.GetComponent<ItemCylinder_S>();
        base.InitItem();
    }

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
        PlayerStatus_S status = other.GetComponent<PlayerStatus_S>();
        if (status == null || base._itemType != Define.Item.Status) return;

        //StartCoroutine(_itemCylinder.FadeOutAndRespawn());
        _itemCylinder.HideSpawnItem();
        if (_statusName == Define.StatusItem.Heart)
        {
            status.Heal();
        }
        else if (_statusName == Define.StatusItem.Energy)
        {
            status.SpUp();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("아이템이 사정거리 안에 존재");

        // 아이템 쿨타임 아닐시에만 적용
        if (!_itemCylinder._usedItem)
            TakeStatusItem(other);
    }
}