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

    private void Awake()
    {
        Debug.Log("Awake 호출됨");
        _pool = new ObjectPool<ModifiedMonster_S>(CreateMonster, OnGetMonster, OnReleaseMonster, OnDestroyMonster, maxSize: 1);
        Debug.Log("_pool 초기화 완료");
    }

    private void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            _pool.Get();
        }
    }

    private ModifiedMonster_S CreateMonster()
    {
        Debug.Log("CreateMonster 호출됨");
        ModifiedMonster_S monster = Instantiate(ghostPrefab, ghostWavePosition.position, Quaternion.identity).GetComponent<ModifiedMonster_S>();
        if (monster != null)
        {
            monster.SetManagedPool(_pool);
            monster.OnMonsterDied += HandleMonsterDied;
            Debug.Log("monster 초기화 완료");
        }
        else
        {
            Debug.LogError("ModifiedMonster_S 컴포넌트를 찾을 수 없습니다.");
        }

        // ModifiedMonster_S 스크립트가 비활성화된 경우 강제로 활성화
        if (!monster.enabled)
        {
            monster.enabled = true;
            Debug.Log("ModifiedMonster_S 스크립트를 강제로 활성화했습니다.");
        }

        return monster;
    }

    private void OnGetMonster(ModifiedMonster_S monster)
    {
        Debug.Log("OnGetMonster 호출됨");
        monster.gameObject.SetActive(true);
    }

    private void OnReleaseMonster(ModifiedMonster_S monster)
    {
        Debug.Log("OnReleaseMonster 호출됨");
        monster.gameObject.SetActive(false);
    }

    private void OnDestroyMonster(ModifiedMonster_S monster)
    {
        Debug.Log("OnDestroyMonster 호출됨");
        Destroy(monster.gameObject);
    }
    private void HandleMonsterDied(ModifiedMonster_S monster)
    {
        Debug.Log("HandleMonsterDied 호출됨");
        _pool.Get(); // 몬스터가 죽을 때 새로운 몬스터 생성
    }
}