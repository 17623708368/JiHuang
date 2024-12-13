using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 死亡状态
/// </summary>
public class AI_DeadState : AI_StateBase
{
     public override void Enter()
     {
          AI.InputCheckColler.enabled = false;
          AI.PlayAnimation("Dead");
          AI.AddAnimationEvent("DeadOver",DeadOver);
     }

     public override void Exit()
     {
          AI.InputCheckColler.enabled = true;
          AI.RemoveAnimationEvent("DeadOver");
     }

     private void DeadOver()
     {
          AI.Dead();
     }
}
