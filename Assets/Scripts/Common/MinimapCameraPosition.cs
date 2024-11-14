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

        // �÷��̾�
        player = transform.root.GetChild(2).gameObject;

        // �̴ϸ� ��ġ ����
        target = transform;
        target.position = new Vector3(0, 0, 0);
        target.SetPositionAndRotation(new Vector3(0,30,0), Quaternion.Euler(90,0,0));

        _secondFloorLayer = LayerMask.NameToLayer("Floor");
    }
    void Update()
    {
        if(!target) return;

        // �̴ϸ� ��ġ ���� (�÷��̾��� �ڽĿ� ī�޶� �����Ƿ�)
        transform.position = new Vector3(
            (transform.position.x),
            (transform.position.y),
            (transform.position.z));

        // 2�� ������ ����
        if (player.transform.position.y > 7)
        {
            minimapCamera.cullingMask |= (1 << _secondFloorLayer);
        }
        else
        {
            minimapCamera.cullingMask &= ~(1 << _secondFloorLayer);
        }

        // ���� ���̰� ���� �̴ϸ� ������ Ȱ��ȭ ����
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