using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolingManager_S : MonoBehaviour
{
    public static PoolingManager_S _instance;
    
    [SerializeField]
    private GameObject _monsterPrefab;

    private IObjectPool<Monster> _pool;

    public List<Transform> _waveSpawnPoints = new List<Transform>();
    float spawnGhostInterval = 60f;  // Monster generation interval. Created after 60 seconds at first.
    int additionalSpawnGhostCount = 0;  // Number of additional monsters to create.

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
        // Initially create two from each GhostWave object.
        for (int i = 0; i < _waveSpawnPoints.Count; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                CreateMonsterAtSpawnPoint(i);
        }
        }

        StartCoroutine(SpawnGhostsOverTime());
    }

    private IEnumerator SpawnGhostsOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnGhostInterval);

            additionalSpawnGhostCount++;
            spawnGhostInterval += 20f;

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
        randomPosition.y = 0; // Fixed position.y value to 0 when creating Ghost.

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
}