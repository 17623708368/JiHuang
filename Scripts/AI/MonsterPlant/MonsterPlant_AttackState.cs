using JKFrame;
using UnityEngine;

public class MonsterPlant_AttackState : AI_AttackState
{
    private int index;

    public override void Enter()
    {
        index = Random.Range(1, 3);

        if (index == 1)
        {
            AI.weapon.OnTriggerStay(CheckHitOnTriggerStay);
        }
        else if (index == 2)
        {
            (AI as MonsterPlant_Contorller).Weapon2.OnTriggerStay(CheckHitOnTriggerStay);
        }

        AI.PlayAnimation("Attack_" + index);
        AI.PlayAudio(AIState.Attack, 1);

        AI.AddAnimationEvent("StartHit", StartHit);
        AI.AddAnimationEvent("StopHit", StopHit);
        AI.AddAnimationEvent("AttackOver", AttackOver);
        AI.transform.LookAt(Player_Controller.Instance.transform.position);
    }

    protected override void StartHit()
    {
        if (index == 1)
        {
            base.StartHit();
        }
        else if (index == 2)
        {
            isAttacked = false;
            (AI as MonsterPlant_Contorller).Weapon2.enabled = true;
        }
    }


    protected override void StopHit()
    {
        if (index == 1)
        {
            base.StopHit();
        }
        else if (index == 2)
        {
            isAttacked = false;
            (AI as MonsterPlant_Contorller).Weapon2.enabled = false;
        }
    }

    public override void Exit()
    {
        base.Exit();
        (AI as MonsterPlant_Contorller).Weapon2.enabled = false;
    }
}