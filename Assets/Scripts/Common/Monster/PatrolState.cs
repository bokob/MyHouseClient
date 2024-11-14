using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class PatrolState : StateMachineBehaviour
{
    public Monster monster;
    [SerializeField] Transform _centerPoint; // ���� ��ġ ���� ������
    public float _range;                    //  ���� ��ġ ���� ����
    public float _patrolSpeed = 1f;         //  ���� �ӵ�
    public float _chaseSpeed = 5f;          //  �߰�

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monster = animator.gameObject.GetComponent<Monster>();
        monster.Agent.speed = _patrolSpeed;
        _centerPoint = animator.transform;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (monster.Agent.isActiveAndEnabled == false) return;

        // �����ϰ� ���� ���� ���ϱ�(�÷��̾� ���� X, ��ǥ������ �Ÿ��� ���ߴ� �Ÿ����� �۾����� �� �ٽ� ���� ���� ���)
        if (!monster.CanSeePlayer && monster.Agent.remainingDistance <= monster.Agent.stoppingDistance)
        {
            Vector3 point;
            if (RandomPoint(_centerPoint.position, _range, out point))
            {
                Debug.DrawRay(point, Vector3.up, Color.red, 3.0f); // ���� ���� ǥ��
                monster.Agent.SetDestination(point);
                monster.Agent.speed = _patrolSpeed;
                return;
            }
        }
        else if (monster.CanSeePlayer && monster.Agent.remainingDistance <= monster.Agent.stoppingDistance) // ���� ���� �ȿ� ���� ��
        {
            monster.Agent.ResetPath();
            animator.SetBool("isAttack", true);
        }
        else if (monster.CanSeePlayer && monster.Agent.remainingDistance > monster.Agent.stoppingDistance) // �÷��̾� ���� O, ���ݹ��� X
        {
            monster.Agent.SetDestination(monster.Target.position); // ��ǥ ����
            monster.Agent.speed = _chaseSpeed;  // �߰� �ӵ��� ����
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("isPatrol", false);
    }

    // ���� ���� ����
    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range; //random point in a sphere 
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {   
            // ������ ũ�� �� ���� �ø��� ���� ����.
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }
}