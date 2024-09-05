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
        if (!other.CompareTag("Player")) return;

        Debug.Log("아이템이 사정거리 안에 입장");

        WeaponManager_S playerWeaponManager = other.GetComponent<PlayerStatus_S>()._weaponManager_S;
        if (playerWeaponManager != null)
            playerWeaponManager.nearMeleeObject = gameObject;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("아이템이 사정거리 안에 존재");
        WeaponManager_S playerWeaponManager = other.GetComponent<PlayerStatus_S>()._weaponManager_S;

        if (playerWeaponManager == null) return;

        // 플레이어가 있고, 근처 근접 무기 탐색에 성공했고, 아이템 줍기 버튼을 눌렀고, 아이템 쿨타임 아닐 때
        if (playerWeaponManager.nearMeleeObject != null && playerWeaponManager._isPickUp && !_itemCylinder._usedItem)
        {
            TakeWeaponItem(other);
            playerWeaponManager._isUsePickUpWeapon = true;
        }
        playerWeaponManager._isPickUp = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("아이템이 사정거리 벗어남");
        WeaponManager_S playerWeaponManager = other.GetComponent<PlayerStatus_S>()._weaponManager_S.GetComponent<WeaponManager_S>();
        if (playerWeaponManager == null) return;

        playerWeaponManager.nearMeleeObject = null;
    }
}