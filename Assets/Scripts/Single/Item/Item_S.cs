using System.Collections;
using System.Collections.Generic;
using UnityEditor.Compilation;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public abstract class Item_S : MonoBehaviour
{
    public float _floatHeight = 0.5f; // 아이템의 떠다니는 높이
    public float _floatSpeed = 1.0f;  // 아이템의 떠다니는 속도
    public float _rotateSpeed = 30f;  // 아이템의 회전 속도
    public float _floatScale = 0.1f;  // sin 함수의 반환 값에 곱해줄 스케일링 팩터, 완만하게 움직이게 하려고 사용
    public SphereCollider _collider; // 아이템 범위
    public Define.Item _itemType = Define.Item.None;

    public float _pickupRange;
    protected Renderer _renderer;
    Transform childMesh; // 빈 오브젝트니까 자식 오브젝트로 있는 Mesh 가져오기 위한 변수

    protected ItemCylinder_S _itemCylinder; // 아이템 소환될 곳

    /// <summary>
    /// 아이템 초기화
    /// </summary>
    protected void InitItem()
    {
        // Mesh를 가져온다.
        childMesh = transform.GetChild(0);

        // SphereCollider 설정
        _collider = GetComponent<SphereCollider>();
        _collider.isTrigger = true;

        _renderer = transform.GetChild(0).GetComponent<Renderer>();
        _itemCylinder = transform.parent.parent.GetComponent<ItemCylinder_S>();
    }

    /// <summary>
    /// 아이템 제자리에 떠다니기
    /// </summary>
    protected void Floating()
    {
        // 아이템을 회전 (월드 좌표 기준)
        // 자기 위치를 기점으로 월드 좌표 방향(Vector3.up) 회전
        childMesh.RotateAround(childMesh.position, Vector3.up, _rotateSpeed * Time.deltaTime);

        // childMesh.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        // 아이템이 위아래로 떠다닐 높이
        float newY = Mathf.Sin(Time.time * _floatSpeed) * _floatScale + _floatHeight;
        childMesh.position = new Vector3(childMesh.position.x, newY, childMesh.position.z);
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
