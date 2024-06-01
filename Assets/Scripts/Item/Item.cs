using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public float _floatHeight = 0.5f; // 아이템의 떠다니는 높이
    public float _floatSpeed = 1.0f;  // 아이템의 떠다니는 속도
    public float _rotateSpeed = 30f;  // 아이템의 회전 속도
    public float _floatScale = 0.1f;  // sin 함수의 반환 값에 곱해줄 스케일링 팩터, 완만하게 움직이게 하려고 사용

    // 빈 오브젝트니까 자식 오브젝트로 있는 Mesh 가져오기 위한 변수
    Transform childMesh;

    // 아이템 타입
    [SerializeField]
    protected Define.Item itemType;

    // 아이템 초기 세팅
    protected void ItemInit()
    {
        // Mesh를 가져온다.
        childMesh = transform.GetChild(0);

        // SphereCollider 설정
        SphereCollider itemCollider = GetComponent<SphereCollider>();
        if(itemCollider == null)
        {
            gameObject.AddComponent<SphereCollider>();
            itemCollider = GetComponent<SphereCollider>();
        }
        itemCollider.isTrigger = true;
        itemCollider.radius = 35f;
    }

    // 아이템 제자리에 떠다니기
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
}
