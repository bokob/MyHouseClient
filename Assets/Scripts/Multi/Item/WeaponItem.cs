using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WeaponItem : Item
{
    public Define.WeaponItem _weaponName;
    ItemCylinder _itemCylinder; // 아이템 소환될 곳
    void Start()
    {
        _itemType = Define.Item.Weapon;
        _itemCylinder = transform.parent.parent.GetComponent<ItemCylinder>();
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
        PlayerStatus status = other.GetComponent<PlayerStatus>();
        if (status == null || base._itemType != Define.Item.Weapon) return;

        _itemCylinder.GetComponent<PhotonView>().RPC("HideSpawnItem", RpcTarget.AllBuffered);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("아이템이 사정거리 안에 입장");

        WeaponManager playerWeaponManager = other.GetComponent<PlayerStatus>()._weaponManager;
        if(playerWeaponManager != null)
            playerWeaponManager.nearMeleeObject = gameObject;
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("아이템이 사정거리 안에 존재");
        WeaponManager playerWeaponManager = other.GetComponent<PlayerStatus>()._weaponManager;

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
        Debug.Log("아이템이 사정거리 벗어남");
        WeaponManager playerWeaponManager = other.GetComponent<PlayerStatus>()._weaponHolder.GetComponent<WeaponManager>();
        if (playerWeaponManager == null) return;

        playerWeaponManager.nearMeleeObject = null;
    }
}