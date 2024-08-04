using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define;

public class GameManager : MonoBehaviour
{
    static public GameManager _instance;

    [Tooltip("The prefab to use for representing the player")]
    [SerializeField]
    private GameObject _playerPrefab;
    public Transform[] _spawnPoints;
    public GameObject _localPlayer;
    // Start is called before the first frame update

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
    }

    void Update()
    {

    }

    private void OnDestroy()
    {
    }

    public void SapwnPlayer()
    {
        Debug.Log("소환");

        Transform spawnPoint = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)];

        GameObject player = PhotonNetwork.Instantiate(_playerPrefab.name, spawnPoint.position, Quaternion.identity);
        _localPlayer = player;

        PlayerStatus status = player.transform.GetChild(2).GetComponent<PlayerStatus>();
        PhotonView photonView = player.transform.GetChild(2).gameObject.GetComponent<PhotonView>();

        status.IsLocalPlayer();  // 로컬 플레이어에 맞게 설정
        photonView.RPC("SetNickname", RpcTarget.AllBuffered, NetworkManager._instance._nickName); // 이름 설정

        // --- 역할 지정(정상 구동) --
        Define.Role role = (PhotonNetwork.IsMasterClient) ? Define.Role.Houseowner : Define.Role.Robber;
        photonView.RPC("SetRole", RpcTarget.AllBuffered, role);
        // ----------------------

        if (status.Role == Define.Role.Robber)
            photonView.RPC("TransformIntoRobber", RpcTarget.AllBuffered);
        else if (status.Role == Define.Role.Houseowner) 
            photonView.RPC("TransformIntoHouseowner", RpcTarget.AllBuffered);
    }
}