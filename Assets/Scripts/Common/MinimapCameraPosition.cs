using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCameraPosition : MonoBehaviour
{
    Camera minimapCamera;
    [SerializeField] bool x, y, z;
    [SerializeField] GameObject player;
    Transform target;
    [SerializeField] GameObject _minimapImage;
    [SerializeField] GameObject _minimapText;

    [SerializeField]  int secondFloorLayer;

    void Start()
    {
        minimapCamera = GetComponent<Camera>();

        // 플레이어
        player = transform.root.GetChild(2).gameObject;

        // 미니맵 위치 설정
        target = transform;
        target.position = new Vector3(0, 0, 0);
        target.SetPositionAndRotation(new Vector3(0,30,0), Quaternion.Euler(90,0,0));

        secondFloorLayer = LayerMask.NameToLayer("Floor");
    }
    void Update()
    {
        if(!target) return;

        // 미니맵 위치 고정 (플레이어의 자식에 카메라가 있으므로)
        transform.position = new Vector3(
            (transform.position.x),
            (transform.position.y),
            (transform.position.z));

        // 2층 렌더링 여부
        if (player.transform.position.y > 7)
        {
            minimapCamera.cullingMask |= (1 << secondFloorLayer);
        }
        else
        {
            minimapCamera.cullingMask &= ~(1 << secondFloorLayer);
        }
    }

    public void OnOffMinimapPanel(bool isActive)
    {
        _minimapImage.SetActive(isActive);
        _minimapText.SetActive(!isActive);
    }
}
