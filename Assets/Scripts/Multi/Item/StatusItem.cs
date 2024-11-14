using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusItem : Item
{
    public Define.StatusItem _statusName;

    ItemCylinder _itemCylinder; // ������ ��ȯ�� ��
    void Start()
    {
        _itemType = Define.Item.Status;
        _itemCylinder = transform.parent.parent.GetComponent<ItemCylinder>();
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
    void TakeStatusItem(Collider other)
    {
        PlayerStatus status = other.GetComponent<PlayerStatus>();
        if (status == null || base._itemType != Define.Item.Status) return;

        Debug.Log("[��Ƽ]��Ʈ �Խ��ϴ�.");

        //StartCoroutine(_itemCylinder.FadeOutAndRespawn());
        _itemCylinder.GetComponent<PhotonView>().RPC("HideSpawnItem", RpcTarget.AllBuffered);
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
        Debug.Log("�������� �����Ÿ� �ȿ� ����");

        // ������ ��Ÿ�� �ƴҽÿ��� ����

        if (_itemCylinder == null) Debug.LogWarning("������ �Ǹ��� ��");

        if (!_itemCylinder._usedItem)
            TakeStatusItem(other);
    }
}