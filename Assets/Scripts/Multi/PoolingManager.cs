using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolingManager : MonoBehaviourPunCallbacks
{
    public static PoolingManager _instance;
    public float _rayDistance = 7f;
    public LayerMask _floorLayerMask;

    [SerializeField]
    private GameObject _monsterPrefab;

    private IObjectPool<Monster> _pool;

    private Coroutine spawnCoroutine;

    public List<Transform> _waveSpawnPoints = new List<Transform>();
    float spawnGhostInterval = 0f;  // Monster generation interval. Created after 0 seconds at first.
    int additionalSpawnGhostCount = 0;  // Number of additional monsters to create.

    int playerCountInGame;

    private void Awake()
    {
        _instance = this;

        if (_pool == null)
        {
            _pool = new ObjectPool<Monster>(CreateMonster, OnGetMonster, OnReleaseMonster, maxSize: 100);
        }
    }

    private void Start()
    {
        CheckPlayerCountAndStartCoroutine();
    }

    void Update()
    {
        // playerCount update.
        playerCountInGame = PhotonNetwork.CurrentRoom.PlayerCount;

        // coroutine control for plyaerCount.
        if (playerCountInGame == 1 && spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnGhostsOverTime());
        }
        else if (playerCountInGame > 1 && spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
            AllGhostDestroy();
        }
    }

    private IEnumerator SpawnGhostsOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnGhostInterval);

            additionalSpawnGhostCount++;
            spawnGhostInterval += 60f;

            for (int i = 0; i < _waveSpawnPoints.Count; i++)
            {
                for (int j = 0; j < additionalSpawnGhostCount; j++)
                {
                    CreateMonsterAtSpawnPoint(i);
                }
            }
        }
    }

    private void CreateMonsterAtSpawnPoint(int spawnPointIndex)
    {
        Vector3 randomPosition = _waveSpawnPoints[spawnPointIndex].position + Random.insideUnitSphere * 7f;

        if (Physics.Raycast(randomPosition, Vector3.down, out RaycastHit hit, _rayDistance, _floorLayerMask))
        {
            randomPosition.y = hit.point.y; // Fixed position.y value to floor.y when creating Ghost.
        }

        Monster monster = Instantiate(_monsterPrefab, randomPosition, Quaternion.identity, _waveSpawnPoints[spawnPointIndex]).GetComponent<Monster>();
        if (monster != null)
        {
            monster.SetManagedPool(_pool);
        }
        else
        {
            Debug.LogError("ModifiedMonster_S component not found");
        }
    }

    private Monster CreateMonster()
    {
        // this method is no longer used, but left to initialize the Pool.
        return null;
    }

    private void OnGetMonster(Monster monster)
    {
        monster.gameObject.SetActive(true);
    }

    private void OnReleaseMonster(Monster monster)
    {
        if (monster.transform.position.y <= -1.5f)
        {
            monster.gameObject.SetActive(false);
        }
    }

    public void AllGhostDestroy()
    {
        // all ghost object destroy.
        Monster[] allGhosts = FindObjectsOfType<Monster>();
        foreach (Monster ghost in allGhosts)
        {
            Destroy(ghost.gameObject);
        }
    }

    // callback when player is left the room.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        // coroutine control check playerCount.
        CheckPlayerCountAndStartCoroutine();
    }

    private void CheckPlayerCountAndStartCoroutine()
    {
        playerCountInGame = PhotonNetwork.CurrentRoom.PlayerCount;

        if (playerCountInGame == 1 && spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnGhostsOverTime());
        }
        else if (playerCountInGame > 1 && spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
            AllGhostDestroy();
        }
    }
}