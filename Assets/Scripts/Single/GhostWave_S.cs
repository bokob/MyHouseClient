using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GhostWave : MonoBehaviour
{
    [SerializeField]
    private GameObject ghostPrefab;

    private IObjectPool<ModifiedMonster_S> _pool;

    public Transform ghostWavePosition;
    float spawnGhostInterval = 60f;  // 몬스터 생성 간격, 처음에 60초 있다가 생성.
    int additionalSpawnGhostCount = 0;  // 추가 생성할 몬스터 수

    private void Awake()
    {
        if (_pool == null)
        {
            Debug.Log("Centralized pool initialization in Awake");
            _pool = new ObjectPool<ModifiedMonster_S>(CreateMonster, OnGetMonster, OnReleaseMonster, OnDestroyMonster, maxSize: 100);
        }
    }

    private void Start()
    {
        // 처음에 각 GhostWave 오브젝트에서 2마리씩 생성
        for (int i = 0; i < 2; i++)
        {
            _pool.Get();
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

            for (int i = 0; i < additionalSpawnGhostCount; i++)
            {
                CreateMonster();
            }
        }
    }

    private ModifiedMonster_S CreateMonster()
    {
        Vector3 randomPosition = ghostWavePosition.position + Random.insideUnitSphere * 7f;
        randomPosition.y = 0; // Ghost 생성 시 position.y 값이 0이도록 고정

        Debug.Log("CreateMonster called");
        ModifiedMonster_S monster = Instantiate(ghostPrefab, randomPosition, Quaternion.identity).GetComponent<ModifiedMonster_S>();
        if (monster != null)
        {
            monster.SetManagedPool(_pool);
            Debug.Log("monster initialized");
        }
        else
        {
            Debug.LogError("ModifiedMonster_S component not found");
        }

        return monster;
    }

    private void OnGetMonster(ModifiedMonster_S monster)
    {
        Debug.Log("OnGetMonster called");
        monster.gameObject.SetActive(true);
    }

    private void OnReleaseMonster(ModifiedMonster_S monster)
    {
        Debug.Log("OnReleaseMonster called");
        monster.gameObject.SetActive(false);
    }

    private void OnDestroyMonster(ModifiedMonster_S monster)
    {
        Debug.Log("OnDestroyMonster called");
        Destroy(monster.gameObject);
    }
}