using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItem_S : Item
{
    public Define.WeaponItem _weaponName;

    ItemCylinder_S _itemCylinder; // 아이템 소환될 곳
    void Start()
    {
        _itemType = Define.Item.Weapon;
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
    public void TakeWeaponItem(Collider other)
    {
        PlayerStatus_S status = other.GetComponent<PlayerStatus_S>();
        if (status == null || base._itemType != Define.Item.Weapon) return;

        _itemCylinder.HideSpawnItem();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("아이템이 사정거리 안에 입장");

        PlayerStatus_S status = other.GetComponent<PlayerStatus_S>();
        if(status != null)
            status.nearMeleeObject = gameObject;
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag != "Player") return;
        Debug.Log("아이템이 사정거리 안에 존재");
        PlayerStatus_S status = other.GetComponent<PlayerStatus_S>();
        // 플레이어가 있고, 근처 근접 무기 탐색에 성공했고, 아이템 줍기 버튼을 눌렀고, 아이템 쿨타임 아닐 때
        if (status != null && status.nearMeleeObject != null && status._isPickUp && !_itemCylinder._usedItem)
            TakeWeaponItem(other);
        status._isPickUp = false;
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("아이템이 사정거리 벗어남");
        PlayerStatus_S status = other.GetComponent<PlayerStatus_S>();

        if (status == null) return;

        status.nearMeleeObject = null;
    }
}