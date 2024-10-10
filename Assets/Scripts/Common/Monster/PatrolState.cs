using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class PatrolState : StateMachineBehaviour
{
    public Monster monster;
    [SerializeField] Transform _centerPoint; // 순찰 위치 정할 기준점
    public float _range;                    //  순찰 위치 정할 범위
    public float _patrolSpeed = 1f;         //  순찰 속도
    public float _chaseSpeed = 5f;          //  추격

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monster = animator.gameObject.GetComponent<Monster>();
        monster.Agent.speed = _patrolSpeed;
        _centerPoint = animator.transform;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (monster.Agent.isActiveAndEnabled == false) return;

        // 랜덤하게 순찰 지점 정하기(플레이어 관측 X, 목표까지의 거리가 멈추는 거리보다 작아졌을 때 다시 순찰 지점 계산)
        if (!monster.CanSeePlayer && monster.Agent.remainingDistance <= monster.Agent.stoppingDistance)
        {
            Vector3 point;
            if (RandomPoint(_centerPoint.position, _range, out point))
            {
                Debug.DrawRay(point, Vector3.up, Color.red, 3.0f); // 순찰 지점 표시
                monster.Agent.SetDestination(point);
                monster.Agent.speed = _patrolSpeed;
                return;
            }
        }
        else if (monster.CanSeePlayer && monster.Agent.remainingDistance <= monster.Agent.stoppingDistance) // 공격 범위 안에 있을 때
        {
            monster.Agent.ResetPath();
            animator.SetBool("isAttack", true);
        }
        else if (monster.CanSeePlayer && monster.Agent.remainingDistance > monster.Agent.stoppingDistance) // 플레이어 관측 O, 공격범위 X
        {
            monster.Agent.SetDestination(monster.Target.position); // 목표 지정
            monster.Agent.speed = _chaseSpeed;  // 추격 속도로 설정
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("isPatrol", false);
    }

    // 랜덤 순찰 지점
    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range; //random point in a sphere 
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {   
            // 범위가 크면 이 값을 늘리는 것이 좋다.
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }
}