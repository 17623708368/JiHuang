using System.Collections;
using System.Collections.Generic;
using JKFrame;
using UnityEngine;

public class AI_StateBase :StateBase
{
   protected AIBase AI;
   public override void Init(IStateMachineOwner owner, int stateType, StateMachine stateMachine)
   {
      base.Init(owner, stateType, stateMachine);
      AI=owner as AIBase;
   }
}
