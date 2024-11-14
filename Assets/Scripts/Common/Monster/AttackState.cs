using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : StateMachineBehaviour
{
    Monster monster;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monster = animator.gameObject.GetComponent<Monster>();
        monster.Agent.isStopped = true;
        animator.speed = 1.2f;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (monster.Agent.isActiveAndEnabled == false) return;

        if (!monster.CanSeePlayer || monster.Agent.remainingDistance > monster.Agent.stoppingDistance) // �÷��̾� ���� X OR ���� ���� X 
        {
            monster.Agent.isStopped = false;
            animator.SetBool("isPatrol", true);
        }
        else if (monster.CanSeePlayer && monster.Agent.remainingDistance <= monster.Agent.stoppingDistance) // �÷��̾� ���� O, ���� ���� O
        {
            monster.Agent.SetDestination(monster.Target.position);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("isAttack", false);
        animator.speed = 1.0f;
    }
}