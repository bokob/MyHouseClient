using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Photon.Pun;
using Photon.Realtime;

public class GhostWave_M : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject ghostPrefab;

    private IObjectPool<ModifiedMonster_S> _pool;
    private Coroutine spawnCoroutine;

    public Transform ghostWavePosition;
    float spawnGhostInterval = 0;  // 몬스터 생성 간격 / 처음에 바로 생성. 그 이후엔 60초씩 더 있다가 생성.
    int additionalSpawnGhostCount = 0;  // 추가 생성할 몬스터 수.

    int playerCountInGame; // 인게임에 있는 플레이어 수.

    private void Awake()
    {
        if (_pool == null)
        {
            _pool = new ObjectPool<ModifiedMonster_S>(CreateMonster, OnGetMonster, OnReleaseMonster, OnDestroyMonster, maxSize: 100);
        }
    }

    private void Start()
    {
        // 플레이어가 한 명인지 아닌지 확인하여 코루틴 시작 여부 결정.
        CheckPlayerCountAndStartCoroutine();
    }

    void Update()
    {
        // 플레이어 수 업데이트.
        playerCountInGame = PhotonNetwork.CurrentRoom.PlayerCount;

        // 플레이어 수에 따라 코루틴 제어.
        if (playerCountInGame == 1 && spawnCoroutine == null)
        {
            Debug.Log("인원 수 1명!!!!");
            spawnCoroutine = StartCoroutine(SpawnGhostsOverTime());
        }
        else if (playerCountInGame > 1 && spawnCoroutine != null)
        {
            Debug.Log("현재 인원은 1명 이상인 " + playerCountInGame + "명!!! 스탑 코루틴 및 기존 고스트 제거");
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

            for (int i = 0; i < additionalSpawnGhostCount; i++)
            {
                CreateMonster();
            }
        }
    }

    private ModifiedMonster_S CreateMonster()
    {
        Vector3 randomPosition = ghostWavePosition.position + Random.insideUnitSphere * 7f;
        randomPosition.y = 0; // Ghost 생성 시 position.y 값이 0이도록 고정.
        ModifiedMonster_S monster = Instantiate(ghostPrefab, randomPosition, Quaternion.identity).GetComponent<ModifiedMonster_S>();
        if (monster != null)
        {
            monster.SetManagedPool(_pool);
        }
        else
        {
            Debug.LogError("ModifiedMonster_S component not found");
        }

        return monster;
    }

    private void OnGetMonster(ModifiedMonster_S monster)
    {
        monster.gameObject.SetActive(true);
    }

    private void OnReleaseMonster(ModifiedMonster_S monster)
    {
        monster.gameObject.SetActive(false);
    }

    private void OnDestroyMonster(ModifiedMonster_S monster)
    {
        Destroy(monster.gameObject);
    }

    public void AllGhostDestroy()
    {
        // 모든 고스트 오브젝트를 제거.
        ModifiedMonster_S[] allGhosts = FindObjectsOfType<ModifiedMonster_S>();
        foreach (ModifiedMonster_S ghost in allGhosts)
        {
            Destroy(ghost.gameObject);
        }
    }

    // 플레이어가 방 떠날 때 호출되는 콜백.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        // 플레이어 수 변경을 확인하여 코루틴 제어.
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