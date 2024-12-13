
using UnityEngine;

public class AI_PatrolState:AI_StateBase
{
  private Vector3 targetVector3;
  public override void Enter()
  {
    AI.NavMeshAgent.enabled = true;
    targetVector3 = AI.GetAIRandomPoint();
    AI.PlayAnimation("Move");
    AI.NavMeshAgent.SetDestination(targetVector3);
  }

  public override void Update()
  {
    if (AI.AIDistance>0)
    {
      if (Vector3.Distance(Player_Controller.Instance.transform.position,AI.transform.position)<AI.AIDistance)
      {
        AI.ChangeState(AIState.Pursue);
        return;
      }
    }
    if ( Vector3.Distance(AI.transform.position,targetVector3)<0.5f)
    {
      AI.ChangeState(AIState.Idle);
    }
  }

  public override void Exit()
  {
    AI.SaveAIData();
    AI.NavMeshAgent.enabled = false;
  }
}