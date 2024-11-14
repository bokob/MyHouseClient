using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinimapCameraPosition : MonoBehaviour
{
    Camera minimapCamera;
    [SerializeField] bool x, y, z;
    [SerializeField] GameObject player;
    Transform target;
    [SerializeField] GameObject _minimapImage;
    [SerializeField] GameObject _minimapText;

    [SerializeField]  int _secondFloorLayer;

    void Start()
    {
        minimapCamera = GetComponent<Camera>();

        // 플레이어
        player = transform.root.GetChild(2).gameObject;

        // 미니맵 위치 설정
        target = transform;
        target.position = new Vector3(0, 0, 0);
        target.SetPositionAndRotation(new Vector3(0,30,0), Quaternion.Euler(90,0,0));

        _secondFloorLayer = LayerMask.NameToLayer("Floor");
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
            minimapCamera.cullingMask |= (1 << _secondFloorLayer);
        }
        else
        {
            minimapCamera.cullingMask &= ~(1 << _secondFloorLayer);
        }

        // 높이 차이가 나는 미니맵 아이콘 활성화 여부
        int idx = 0;
        if(SceneManager.GetActiveScene().name == "MultiPlayScene")
        {
            foreach (GameObject playerObject in GameManager._instance.players)
            {
                GameObject minimapIcon = playerObject;
                if(playerObject.GetComponent<PlayerStatus>().Role == Define.Role.Robber)
                {
                    minimapIcon = minimapIcon.transform.GetChild(0).GetChild(0).gameObject;
                }
                else if(playerObject.GetComponent<PlayerStatus>().Role == Define.Role.Houseowner)
                {
                    minimapIcon = minimapIcon.transform.GetChild(1).GetChild(0).gameObject;
                }
                

                if (Mathf.Abs(minimapIcon.transform.position.y - player.transform.position.y) > 6)
                {
                    minimapIcon.gameObject.SetActive(false);
                }
                else
                {
                    minimapIcon.gameObject.SetActive(true);
                }
            }
        }
    }

    public void OnOffMinimapPanel(bool isActive)
    {
        _minimapImage.SetActive(isActive);
        _minimapText.SetActive(!isActive);
    }
}