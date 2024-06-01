using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 한 손 근접 무기
/// </summary>
public class Melee : Weapon
{
    BoxCollider _meleeArea;       // 근접 공격 범위
    TrailRenderer _trailEffet;    // 휘두를 때 효과

    #region 절단 효과
    public LayerMask _sliceMask; // 자를 대상인 레이어 마스크
    public float _cutForce = 250f; // 자를 때 가해지는 힘

    Vector3 _entryPoint; // 오브젝트에 들어간 지점
    Vector3 _exitPoint; // 오브젝트를 뚫고 나간 지점
    bool _hasExited = false; // 오브젝트를 뚫고 나갔는지 여부를 저장하는 변수
    #endregion

    void Awake()
    {
        base.Type = Define.Type.Melee;
        _meleeArea = gameObject.GetComponent<BoxCollider>();
        _trailEffet = gameObject.GetComponentInChildren<TrailRenderer>();

        // TODO
        /*
         * 무기 능력치를 엑셀이나 json을 이용해 관리 예정
         * 따로 읽어와서 그 값들을 세팅해줘야 함
         * 현재 임시로 테스트를 위해 하드코딩 함
        */
        if (gameObject.tag == "Melee")
            base.Attack = 50;

    }

    /// <summary>
    /// Use() 실행하면서 MeleeAttackOn() 코루틴 같이 실행된다.
    /// </summary>
    public override void Use()
    {
        StopCoroutine("MeleeAttackOn");
        StartCoroutine("MeleeAttackOn");
    }

    /// <summary>
    /// 코루틴으로 Collider, TrailRenderer 특정 시간 동안만 활성화
    /// </summary>
    IEnumerator MeleeAttackOn()
    {
        yield return new WaitForSeconds(0.5f);
        _meleeArea.enabled = true;
        _trailEffet.enabled = true;

        yield return new WaitForSeconds(0.5f);
        _meleeArea.enabled = false;

        yield return new WaitForSeconds(0.5f);
        _trailEffet.enabled = false;
    }

    // 칼이 트리거 안에 있을 때 hasExited를 false로 설정
    void OnTriggerEnter(Collider other)
    {
        _hasExited = false;
        _entryPoint = other.ClosestPoint(transform.position);
    }

    void OnTriggerStay(Collider other)
    {
        Debug.Log("관통");
    }
    void OnTriggerExit(Collider other) // 관통 다 되면 레이어에 따라 절단
    {
        // 충돌 지점의 방향을 자르는 방향으로 설정
        _exitPoint = other.ClosestPoint(transform.position);

        Vector3 cutDirection = _exitPoint - _entryPoint;
        Vector3 cutInPlane = (_entryPoint + _exitPoint) / 2;

        //Vector3 cutPlaneNormal = Vector3.Cross((entryPoint - exitPoint), (entryPoint - transform.position)).normalized;
        Vector3 cutPlaneNormal = Vector3.Cross((_entryPoint - _exitPoint), (_entryPoint - transform.position)).normalized;
        Debug.Log(cutPlaneNormal.x + ", " + cutPlaneNormal.y + ", " + cutPlaneNormal.z);

        if (cutPlaneNormal.x == 0 && cutPlaneNormal.y == 0 && cutPlaneNormal.z == 0)
        {
            // 원래 자르던 방향을 normalize 해서 넣어줘야 됨
            cutPlaneNormal = (_entryPoint - _exitPoint).normalized;
            Debug.Log("대체: " + cutPlaneNormal.x + " " + cutPlaneNormal.y + " " + cutPlaneNormal.z);

            bool isHorizontalCut = Mathf.Abs(cutDirection.x) > Mathf.Abs(cutDirection.y);

            // 가로로 자르는 경우
            if (isHorizontalCut)
            {
                // x 축 방향으로 자르기 때문에 cutPlaneNormal을 x 축 방향 벡터로 설정
                cutPlaneNormal = Vector3.up;
            }
            else // 세로로 자르는 경우
            {
                // y 축 방향으로 자르기 때문에 cutPlaneNormal을 y 축 방향 벡터로 설정
                cutPlaneNormal = Vector3.right;
            }
        }

        LayerMask cutableMask = LayerMask.GetMask(LayerMask.LayerToName(other.gameObject.layer));
        //Debug.Log("잘릴 레이어: " + LayerMask.LayerToName(other.gameObject.layer));
        if (_sliceMask.value == cutableMask)
        {
            Debug.LogWarning("자를 수 있는 오브젝트");
            // 오브젝트를 자르기
            Cutter.Cut(other.gameObject, cutInPlane, cutPlaneNormal);

            // 자를 때 가해지는 힘을 적용하여 오브젝트를 밀어냄
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(-cutPlaneNormal * _cutForce); // cutDirection 대신에 cutPlaneNormal을 사용
            }

            _hasExited = true;
        }
        else
        {
            //Debug.Log("sliceMask: " + sliceMask.value);
            //Debug.Log("자를 레이어: " + other.gameObject.layer);
            Debug.LogWarning("왜 안돼?");
        }
    }
}
