using JKFrame;
using UnityEngine;

public class AI_AttackState:AI_StateBase
{
        
        public override void Enter()
        {
             
                int index = Random.Range(1, 3);
                AI.PlayAnimation("Attack_"+index);
                AI.PlayAudio(AIState.Attack,1);
                AI.weapon.OnTriggerStay(CheckHitOnTriggerStay);
                AI.AddAnimationEvent("StartHit",StartHit);
                AI.AddAnimationEvent("StopHit",StopHit);
                AI.AddAnimationEvent("AttackOver",AttackOver);
                AI.transform.LookAt(Player_Controller.Instance.transform.position);
        }

        protected bool isAttacked=false;
        protected virtual  void StartHit()
        {
                isAttacked = false;
                AI.weapon .enabled=true; 
        }
        protected virtual void StopHit()
        {
                isAttacked = false;
                AI.weapon .enabled=false; 
        }
        protected void AttackOver()
        {
                AI.ChangeState(AIState.Pursue);
        }

        protected void CheckHitOnTriggerStay(Collider other, object[] arg2)
        {
                if (isAttacked==true)return;
                if (other.CompareTag("Player"))
                {
                        isAttacked = true;
                        AI.PlayAudio(AIState.Hit,1);
                        Player_Controller.Instance.Hurt(AI.AttackValue);
                }
        }
        public override void Exit()
        {
                AI.weapon.RemoveTriggerStay(CheckHitOnTriggerStay);
                AI.RemoveAnimationEvent("StartHit");
                AI.RemoveAnimationEvent("StopHit");
                AI.RemoveAnimationEvent("AttackOver");
                AI.weapon .enabled=false; 
        }

     
}
 