using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItem_S : Item
{
    public Define.WeaponItem _weaponName;

    ItemCylinder_S _itemCylinder; // ������ ��ȯ�� ��
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
    /// ���� ���� ������ ȹ��
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

        Debug.Log("�������� �����Ÿ� �ȿ� ����");

        WeaponManager_S playerWeaponManager = other.GetComponent<PlayerStatus_S>()._weaponManager_S;
        if (playerWeaponManager != null)
            playerWeaponManager.nearMeleeObject = gameObject;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("�������� �����Ÿ� �ȿ� ����");
        WeaponManager_S playerWeaponManager = other.GetComponent<PlayerStatus_S>()._weaponManager_S;

        if (playerWeaponManager == null) return;

        if(playerWeaponManager.nearMeleeObject == null && playerWeaponManager._isPickUp && !_itemCylinder._usedItem) // ������ �� ���� ���
        {
            // ������ �Ǹ��� ��Ÿ��
            TakeWeaponItem(other);
            playerWeaponManager._isUsePickUpWeapon = true;
            playerWeaponManager._isPickUp = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("�������� �����Ÿ� ���");
        WeaponManager_S playerWeaponManager = other.GetComponent<PlayerStatus_S>()._weaponManager_S.GetComponent<WeaponManager_S>();
        if (playerWeaponManager == null) return;

        playerWeaponManager.nearMeleeObject = null;
    }
}