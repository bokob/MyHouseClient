using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
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
    public TextMeshProUGUI _playerCount;

    // 아이템 소환 관련
    public GameObject _itemCylinder;
    public float _itemSpawnRange = 20.0f;
    public Vector3 spawnCenter;
    public Transform _itemSpawnTestPosition;

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
        // 계속 확인하는 것은 부하가 심하니까 NetworkManager에 있는 것을 확인하는 식으로 하자.
        _playerCount.text = PhotonNetwork.CountOfPlayers.ToString();

        if (PhotonNetwork.IsMasterClient && Input.GetKeyDown(KeyCode.T))
        {
            SpawnItem();
        }

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

    // 마스터 클라이언트의 죽음을 처리하는 함수
    public void OnMasterClientKilled(Player killer)
    {
        if (PhotonNetwork.IsMasterClient) // 집주인은 자신을 죽인 플레이어를 집주인으로 지정
        {
            // 새로운 마스터 클라이언트 설정
            PhotonNetwork.SetMasterClient(killer);
            Debug.Log("New Master Client is: " + killer.NickName);
        }
    }

    // 플레이어의 죽음을 처리하는 함수
    public void OnPlayerKilled(Player killedPlayer, Player killer)
    {
        if (killedPlayer == PhotonNetwork.MasterClient) // 살해당한 플레이어가 집주인이면
        {
            OnMasterClientKilled(killer);
        }
    }

    [PunRPC]
    void SpawnItem()
    {
        Vector3 randomPosition = GetRandomPosition(spawnCenter, _itemSpawnRange);
        PhotonNetwork.Instantiate(_itemCylinder.name, _itemSpawnTestPosition.position, Quaternion.identity);
    }

    Vector3 GetRandomPosition(Vector3 center, float range)
    {
        Vector3 randomDirection = Random.insideUnitSphere * range;
        randomDirection += center;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, range, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return center;
    }
}