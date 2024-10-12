using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemCylinder_S : MonoBehaviour
{
    [Tooltip("재생성 관련")]
    [SerializeField] float _fadeDuration = 0f; // 사라지는 시간
    [SerializeField] float _respawnTimeSetValue; // 재생성 시간
    public bool _usedItem = false; // 아이템 사용되었는지(쿨타임)

    [Tooltip("아이템 재등장 시간 UI 관련")]
    public GameObject _timerHolder;
    public TextMeshPro _itemTimer;  // 아이템 재생성 시간 표시할 UI
    public float _respawnTime;      // 아이템 실린더에 표시될 시간

    [Tooltip("아이템 실린더에서 관리할 아이템 관련")]
    public Define.Item _spawnItemType = Define.Item.None;
    public GameObject _spawnItemObject;
    public Item _spawnItem;

    void Start()
    {
        InitSpawnItem();
    }

    void InitSpawnItem()
    {
        // 스폰할 아이템
        _spawnItemType = (Define.Item)UnityEngine.Random.Range(1, 3);
        GameObject spawnItemObjectParent = transform.GetChild((int)_spawnItemType).gameObject;
        int childIdx = UnityEngine.Random.Range(0, spawnItemObjectParent.transform.childCount);
        _spawnItemObject = spawnItemObjectParent.transform.GetChild(childIdx).gameObject;

        // 스크립트 보유하고 있지 않으면, 스크립트 추가
        if (_spawnItemType == Define.Item.Status && _spawnItemObject.GetComponent<StatusItem>() == null)
        {
            StatusItem_S tmp = _spawnItemObject.AddComponent<StatusItem_S>();
            tmp._statusName = (Define.StatusItem)childIdx;
        }
        else if (_spawnItemType == Define.Item.Weapon && _spawnItemObject.GetComponent<WeaponItem>() == null)
        {
            WeaponItem_S tmp = _spawnItemObject.AddComponent<WeaponItem_S>();
            tmp._weaponName = (Define.WeaponItem)childIdx;
        }
        _spawnItem = _spawnItemObject.GetComponent<Item>();

        _spawnItemObject.SetActive(true);

        // 재생성 시간 UI 비활성화
        SetRespawnTime();
        _timerHolder.SetActive(false);
    }

    void Update()
    {
        CountRespawnTime();
    }

    // 재생성 시간 세기
    void CountRespawnTime()
    {
        if (_usedItem)
        {
            _respawnTime -= Time.deltaTime;
            _itemTimer.text = Mathf.FloorToInt(_respawnTime).ToString();
            if (_respawnTime <= 0)
            {
                _usedItem = false;
                InitSpawnItem();
            }
        }
    }

    public void HideSpawnItem()
    {
        _usedItem = true;
        _spawnItem.gameObject.SetActive(false);
        _timerHolder.SetActive(true);
    }

    public void SetRespawnTime()
    {
        _respawnTime = _respawnTimeSetValue;
    }
}