using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemCylinder : MonoBehaviour
{   
    [SerializeField] float _fadeDuration = 0f; // 사라지는 시간
    [Tooltip("재생성 시간")][SerializeField] float _respawnTimeSetValue = 30.0f; // 재생성 시간
    public bool _usedItem; // 아이템 사용되었는지(쿨타임)
    
    // 아이템 재등장 UI
    public GameObject _timerHolder; 
    public TextMeshPro _itemTimer;  // 아이템 재생성 시간 표시할 UI
    float _respawnTime = 30;             // 아이템 실린더에 표시될 시간

    // 아이템 실린더에서 관리할 아이템
    public Define.Item _spawnItemType = Define.Item.None;
    public Item _spawnItem;

    void Awake()
    {
        // 스폰할 아이템
        GameObject spawmItemObject = transform.GetChild((int)_spawnItemType).gameObject;
        // 스크립트 추가
        spawmItemObject.AddComponent<StatusItem>();
        _spawnItem = spawmItemObject.GetComponent<Item>();

        spawmItemObject.SetActive(true);

        // 재생성 시간 UI 비활성화
        _timerHolder.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) StartCoroutine(FadeOutAndRespawn()); // 테스트 코드

        CountRespawnTime();
    }

    // 재생성 시간 세기
    void CountRespawnTime()
    {
        if (_usedItem)
        {
            _respawnTime -= Time.deltaTime;
            _itemTimer.text = Mathf.FloorToInt(_respawnTime).ToString();
        }
    }

    /// <summary>
    /// 아이템 투명화
    /// </summary>
    /// <param name="other"></param>
    public IEnumerator FadeOutAndRespawn()
    {
        float currentTime = 0f;
        StatusItem statusItem = _spawnItem.GetComponent<StatusItem>();

        Color initialColor = statusItem.GetItemColor();

        // 아이템 안보이게 하기
        while (currentTime < _fadeDuration)
        {
            currentTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, currentTime / _fadeDuration);
            Color newColor = initialColor;
            newColor.a = alpha;
            statusItem.ChangeColor(newColor);
            yield return null;
        }

        // 아이템 대기시간 동안 안보이게 하기
        _usedItem = true;
        statusItem.EnableItem(false);
        _timerHolder.SetActive(true);
        yield return new WaitForSeconds(_respawnTimeSetValue);

        // 아이템 리스폰
        _usedItem = false;
        ResetRespawnTime();
        statusItem.EnableItem(true);
        statusItem.ChangeColor(initialColor);
        _timerHolder.SetActive(false);
    }

    public void ResetRespawnTime()
    {
        _respawnTime = _respawnTimeSetValue;
    }
}