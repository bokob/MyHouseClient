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
        if (!other.CompareTag("Player")) return;

        Debug.Log("아이템이 사정거리 안에 입장");

        WeaponManager playerWeaponManager = other.GetComponent<PlayerStatus>()._weaponManager;
        if(playerWeaponManager != null)
            playerWeaponManager.nearMeleeObject = gameObject;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("아이템이 사정거리 안에 존재");
        WeaponManager playerWeaponManager = other.GetComponent<PlayerStatus>()._weaponManager;

        if (playerWeaponManager == null) return;

        if (playerWeaponManager.nearMeleeObject == null && playerWeaponManager._isPickUp && !_itemCylinder._usedItem) // 아이템 갓 먹은 경우
        {
            // 아이템 실린더 쿨타임
            TakeWeaponItem(other);
            playerWeaponManager._isUsePickUpWeapon = true;
            playerWeaponManager._isPickUp = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("아이템이 사정거리 벗어남");


        PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();
        if (playerStatus == null) return;
        WeaponManager playerWeaponManager = playerStatus._weaponHolder.GetComponent<WeaponManager>();
        if (playerWeaponManager == null) return;

        playerWeaponManager.nearMeleeObject = null;
    }
}