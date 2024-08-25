using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCameraPosition : MonoBehaviour
{
    Camera minimapCamera;
    [SerializeField] bool x, y, z;
    [SerializeField] GameObject player;
    Transform target;

    [SerializeField]  int secondFloorLayer;

    private void Start()
    {
        minimapCamera = GetComponent<Camera>();

        // 플레이어
        player = transform.root.GetChild(1).gameObject;

        // 미니맵 위치 설정
        target = transform;
        target.position = new Vector3(0, 0, 0);
        target.SetPositionAndRotation(new Vector3(0,30,0), Quaternion.Euler(90,0,0));

        secondFloorLayer = LayerMask.NameToLayer("Floor");
    }
    private void Update()
    {
        if(!target) return;

        // 미니맵 위치 고정 (플레이어의 자식에 카메라가 있으므로)
        transform.position = new Vector3(
            (transform.position.x),
            (transform.position.y),
            (transform.position.z));

        // 2층 렌더링 여부
        if (player.transform.position.y > 10)
        {
            minimapCamera.cullingMask |= (1 << secondFloorLayer);
        }
        else
        {
            minimapCamera.cullingMask &= ~(1 << secondFloorLayer);
        }
    }
}
