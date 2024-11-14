using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemCylinder_S : MonoBehaviour
{
    [Tooltip("����� ����")]
    [SerializeField] float _fadeDuration = 0f; // ������� �ð�
    [SerializeField] float _respawnTimeSetValue; // ����� �ð�
    public bool _usedItem = false; // ������ ���Ǿ�����(��Ÿ��)

    [Tooltip("������ ����� �ð� UI ����")]
    public GameObject _timerHolder;
    public TextMeshPro _itemTimer;  // ������ ����� �ð� ǥ���� UI
    public float _respawnTime;      // ������ �Ǹ����� ǥ�õ� �ð�

    [Tooltip("������ �Ǹ������� ������ ������ ����")]
    public Define.Item _spawnItemType = Define.Item.None;
    public GameObject _spawnItemObject;
    public Item _spawnItem;

    void Start()
    {
        InitSpawnItem();
    }

    void InitSpawnItem()
    {
        // ������ ������
        _spawnItemType = (Define.Item)UnityEngine.Random.Range(1, 3);
        GameObject spawnItemObjectParent = transform.GetChild((int)_spawnItemType).gameObject;
        int childIdx = UnityEngine.Random.Range(0, spawnItemObjectParent.transform.childCount);
        _spawnItemObject = spawnItemObjectParent.transform.GetChild(childIdx).gameObject;

        // ��ũ��Ʈ �����ϰ� ���� ������, ��ũ��Ʈ �߰�
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

        // ����� �ð� UI ��Ȱ��ȭ
        SetRespawnTime();
        _timerHolder.SetActive(false);
    }

    void Update()
    {
        CountRespawnTime();
    }

    // ����� �ð� ����
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