using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusItem : Item
{
    [SerializeField]
    float _pickupRange;
    Renderer _renderer;
    ItemCylinder _itemCylinder; // 아이템 소환될 곳

    // 상태 아이템 타입
    public Define.Item StatusItemType
    {
        get { return itemType; }
        set { itemType = value; }
    }

    void Start()
    {
        base.ItemInit();
        _pickupRange = 2f;
        _renderer = transform.GetChild(0).GetComponent<Renderer>();
        _itemCylinder = transform.parent.GetComponent<ItemCylinder>();
        StatusItemType = _itemCylinder._spawnItemType;
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
        if (status == null) return;
        if (base.itemType == Define.Item.Heart)
        {
            status.Heal();
        }
        else if (base.itemType == Define.Item.Energy)
        {
            status.SpUp();
        }
    }

    /// <summary>
    /// 줍기
    /// </summary>
    /// <param name="other"></param>
    void PickUp(Collider other)
    {
        Debug.Log("아이템 줍기");
        TakeStatusItem(other);
        StartCoroutine(_itemCylinder.FadeOutAndRespawn());
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("아이템이 사정거리 안에 존재");

        // 아이템 쿨타임 아닐시에만 적용
        if (!_itemCylinder._usedItem)
            PickUp(other);
    }

    // 상태 아이템 활성화/비활성화
    public void EnableItem(bool state)
    {
        _renderer.enabled = state;
        _collider.enabled = state;
    }

    // 아이템 색 바꾸기
    public void ChangeColor(Color color)
    {
        _renderer.material.color = color;
    }

    // 아이템 색 얻기
    public Color GetItemColor()
    {
        return _renderer.material.color;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _pickupRange);
    }
}
