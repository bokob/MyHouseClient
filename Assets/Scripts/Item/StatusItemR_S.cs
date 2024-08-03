using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusItemR_S : Item
{
    [SerializeField]
    float _pickupRange;
    [SerializeField]
    float _fadeDuration = 0.2f; // 사라지는 시간
    [SerializeField]
    float _respawnTime = 30.0f; // 재생성 시간
    Renderer renderer;
    Collider itemCollider;
    private bool _usedItem; // 아이템 사용되었는지(쿨타임)
    public TextMeshPro _itemTimer;
    public GameObject _timerHolder;
    float _respawn;
    void Start()
    {
        base.ItemInit();
        _pickupRange = 2f;
        renderer = transform.GetChild(0).GetComponent<Renderer>();
        itemCollider = transform.GetChild(0).GetComponent<Collider>();
        _timerHolder.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        base.Floating();
        if(Input.GetKeyDown(KeyCode.I)) StartCoroutine(FadeOutAndRespawn()); // 임시 코드
        if(_usedItem != false)
        {
            _respawn -= Time.deltaTime;
            int _itemTime = Mathf.FloorToInt(_respawn);
            _itemTimer.text = _itemTime.ToString() + "s";
        }
    }

    /// <summary>
    /// 상태 관련 아이템 획득
    /// </summary>
    /// <param name="other"></param>
    void TakeStatusItem(Collider other)
    {
        PlayerStatus status = other.GetComponent<PlayerStatus>();
        if (base.itemType == Define.Item.Heart)
        {
            if ((int)status.Hp == (int)status.MaxHp)
                return;
            status.Heal();
        }
        else if (base.itemType == Define.Item.Energy)
        {
            if ((int)status.Sp == (int)status.MaxSp)
                return;
            status.SpUp();
        }
        StartCoroutine(FadeOutAndRespawn());
    }

    /// <summary>
    /// 줍기
    /// </summary>
    /// <param name="other"></param>
    void PickUp(Collider other)
    {
        Debug.Log("아이템 줍기");
        TakeStatusItem(other);
        _respawn = _respawnTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("아이템이 사정거리 안에 존재");
        if(_usedItem != true) PickUp(other); // 아이템 쿨타임 아닐시에만 적용
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _pickupRange);
    }

    /// <summary>
    /// 아이템 투명화
    /// </summary>
    /// <param name="other"></param>
    IEnumerator FadeOutAndRespawn()
    {
        float currentTime = 0f;
        Color initialColor = renderer.material.color;

        while (currentTime < _fadeDuration)
        {
            currentTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, currentTime / _fadeDuration);
            Color newColor = initialColor;
            newColor.a = alpha;
            renderer.material.color = newColor;
            yield return null;
        }

        renderer.enabled = false;
        itemCollider.enabled = false;
        _usedItem = true;
        _timerHolder.SetActive(true);

        yield return new WaitForSeconds(_respawnTime);

        renderer.enabled = true;
        itemCollider.enabled = true;
        renderer.material.color = initialColor;
        _usedItem = false;
        _timerHolder.SetActive(false);
    }
}
