using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    public GameObject[] players;

    // ������ ��ȯ ����
    public List<ItemCylinder> _itemCylinders = new List<ItemCylinder>();
    public float _itemSpawnRange = 20.0f;
    public Vector3 spawnCenter;
    public Transform _itemSpawnTestPosition;

    [Header("Weapon Status")]
    public List<WeaponData> weaponStatusList; // ���� �ɷ�ġ �ҷ��� �� ������ ��

    bool startSpawnItem = false;

    private void Awake()
    {
        Define._sceneName = SceneManager.GetActiveScene().name;

        if (_instance == null)
        {
            _instance = this;
            LoadWeaponData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient && startSpawnItem == false)
        {
            SpawnItem();
            startSpawnItem = true;
        }

        players = GameObject.FindGameObjectsWithTag("Player");
    }

    public void SapwnPlayer()
    {
        Debug.Log("��ȯ");

        Transform spawnPoint = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Length)];

        GameObject player = PhotonNetwork.Instantiate(_playerPrefab.name, spawnPoint.position, Quaternion.identity);
        _localPlayer = player;

        PlayerStatus status = player.transform.GetChild(2).GetComponent<PlayerStatus>();
        PhotonView photonView = player.transform.GetChild(2).gameObject.GetComponent<PhotonView>();

        status.IsLocalPlayer();  // ���� �÷��̾ �°� ����
        photonView.RPC("SetNickname", RpcTarget.AllBuffered, NetworkManager._instance._nickName); // �̸� ����
        
        // ���� ����
        Define.Role role = (PhotonNetwork.IsMasterClient) ? Define.Role.Houseowner : Define.Role.Robber;
        photonView.RPC("SetRole", RpcTarget.AllBuffered, role);
    }

    void SpawnItem()
    {
        for (int i = 0; i < _itemCylinders.Count; i++)
        {

            int randomItemType = UnityEngine.Random.Range(1, 3);
            GameObject spawnItemObjectParent = _itemCylinders[i].gameObject.transform.GetChild(randomItemType).gameObject;
            int randomItemIdx = UnityEngine.Random.Range(0, spawnItemObjectParent.transform.childCount);

            GetComponent<PhotonView>().RPC("SetSpawnItemRPC", RpcTarget.AllBuffered, i, randomItemType, randomItemIdx);
        }
    }

    [PunRPC]
    void SetSpawnItemRPC(int itemCylinderIdx, int itemType, int itemIdx) // ������ �Ǹ��� ��ȣ, ������ Ÿ��, ���� ����������
    {
        _itemCylinders[itemCylinderIdx].GetComponent<ItemCylinder>().InitSpawnItem(itemType, itemIdx);
    }

    [PunRPC]
    void ReceiveRequestToSpawnItemRPC(int itemCylinderIdx) // ������ ��ȯ ��û ����
    {
        int randomItemType = UnityEngine.Random.Range(1, 3);
        GameObject spawnItemObjectParent = _itemCylinders[itemCylinderIdx].gameObject.transform.GetChild(randomItemType).gameObject;
        int randomItemIdx = UnityEngine.Random.Range(0, spawnItemObjectParent.transform.childCount);

        GetComponent<PhotonView>().RPC("SetSpawnItemRPC", RpcTarget.AllBuffered, itemCylinderIdx, randomItemType, randomItemIdx);
    }

    Vector3 GetRandomPosition(Vector3 center, float range)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * range;
        randomDirection += center;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, range, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return center;
    }

    // ���� ���� �ε�
    void LoadWeaponData()
    {
        string filePath = "WeaponData";
        try
        {
            string jsonContent = Resources.Load<TextAsset>(filePath).ToString();
            weaponStatusList = JsonConvert.DeserializeObject<List<WeaponData>>(jsonContent);

            if (weaponStatusList != null)
            {
                Debug.Log("Weapon data loaded successfully.");

                if (weaponStatusList.Count == 0)
                {
                    Debug.LogWarning("Weapon list is empty.");
                }
                else
                {
                    foreach (var weaponStatus in weaponStatusList)
                    {
                        Debug.Log($"Weapon: {weaponStatus.Name}, Type: {weaponStatus.Type}, {(int)weaponStatus.Type}, Attack: {weaponStatus.Attack}, Rate: {weaponStatus.Rate}");
                    }
                }
            }
        }
        catch (JsonException ex)
        {
            Debug.LogError($"JSON parsing error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load weapon data: {ex.Message}");
        }
    }

    public WeaponData GetWeaponStatusByName(string weaponName)
    {
        return weaponStatusList.Find(weapon => weapon.Name == weaponName);
    }
}