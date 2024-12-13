using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPlant_Contorller : AIBase
{
    [SerializeField] private Collider weapon2;
    public Collider Weapon2 => weapon2;
    [SerializeField] private float aiMaxDistance;
    public float AIMaxDistance => aiMaxDistance;

    [SerializeField] float walkSpeed;
    public float WalkSpeed=>walkSpeed;
    [SerializeField] float runSpeed;
    public float RunSpeed=>runSpeed;

    public override void ChangeState(AIState state)
    {
        switch (state)
        {
            case AIState.Idle:
                StateMachine.ChangeState<AI_IdelState>((int)state);
                break;
            case AIState.Attack:
                StateMachine.ChangeState<MonsterPlant_AttackState>((int)state);
                break;
            case AIState.Patrol:
                StateMachine.ChangeState<AI_PatrolState>((int)state);
                break;
            case AIState.Hurt:
                StateMachine.ChangeState<AI_HurtState>((int)state);
                break;
            case AIState.Dead:
                StateMachine.ChangeState<AI_DeadState>((int)state);
                break;
            case AIState.Pursue:
                StateMachine.ChangeState<MonsterPlant_PursueState>((int)state);
                break;
        }

        currentAIState = state;
    }
}