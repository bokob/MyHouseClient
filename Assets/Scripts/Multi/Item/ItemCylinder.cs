using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;

public class ItemCylinder : MonoBehaviour
{
    [Tooltip("재생성 관련")]
    [SerializeField] float _fadeDuration = 0f; // 사라지는 시간
    [SerializeField] float _respawnTimeSetValue; // 재생성 시간
    public bool _usedItem = false; // 아이템 사용되었는지(쿨타임)

    [Tooltip("아이템 재등장 시간 UI 관련")]
    //public GameObject _timerHolder;
    //public TextMeshPro _itemTimer;  // 아이템 재생성 시간 표시할 UI
    public float _respawnTime;      // 아이템 실린더에 표시될 시간

    [Tooltip("아이템 실린더에서 관리할 아이템 관련")]
    public Define.Item _spawnItemType = Define.Item.None;
    public GameObject _spawnItemObject;

    public void InitSpawnItem(int _spawnItemTypeNum, int childIdxNum)
    {
        DisableItemType();
        _usedItem = false;

        // 스폰할 아이템
        _spawnItemType = (Define.Item)_spawnItemTypeNum;
        GameObject spawnItemObjectParent = transform.GetChild((int)_spawnItemType).gameObject;
        _spawnItemObject = spawnItemObjectParent.transform.GetChild(childIdxNum).gameObject;

        // 스크립트 보유하고 있지 않으면, 스크립트 추가
        if (_spawnItemType == Define.Item.Status && _spawnItemObject.GetComponent<StatusItem>() == null)
        {
            StatusItem tmp = _spawnItemObject.AddComponent<StatusItem>();
            tmp._statusName = (Define.StatusItem)childIdxNum;
        }
        else if (_spawnItemType == Define.Item.Weapon && _spawnItemObject.GetComponent<WeaponItem>() == null)
        {
            WeaponItem tmp = _spawnItemObject.AddComponent<WeaponItem>();
            tmp._weaponName = (Define.WeaponItem)childIdxNum;
        }

        _spawnItemObject.SetActive(true);

        // 재생성 시간 UI 비활성화
        SetRespawnTime();
        //_timerHolder.SetActive(false);
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
            //_itemTimer.text = Mathf.FloorToInt(_respawnTime).ToString();
            if (_respawnTime <= 0)
            {
                // 마스터 클라이언트에게 아이템 소환 요청
                GameManager._instance.gameObject.GetComponent<PhotonView>().RPC("ReceiveRequestToSpawnItemRPC", RpcTarget.MasterClient, int.Parse(gameObject.name));
            }
        }
    }

    [PunRPC]
    public void HideSpawnItem()
    {
        _usedItem = true;
        if(_spawnItemObject != null)
            _spawnItemObject.gameObject.SetActive(false);
        //_timerHolder.SetActive(true);
    }

    public void SetRespawnTime()
    {
        _respawnTime = _respawnTimeSetValue;
    }

    // 아이템 소환 전 활성화 되어 있는 아이템 비활성화 처리
    public void DisableItemType()
    {
        Transform statusItem = transform.GetChild(1);
        Transform weaponItem = transform.GetChild(2);
        foreach(Transform item in statusItem)
        {
            item.gameObject.SetActive(false);
        }
        foreach (Transform item in weaponItem)
        {
            item.gameObject.SetActive(false);
        }
    }
}