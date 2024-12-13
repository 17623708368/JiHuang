using UnityEngine;

public class MonsterPlant_PursueState:AI_PursueState
{
  public override void Enter()
  {
    AI.PlayAnimation("Run");
    AI.NavMeshAgent.enabled = true;
    AI.NavMeshAgent.speed = (AI as MonsterPlant_Contorller).RunSpeed;
  }

  public override void Update()
  {    
    if (!GameSceneManager.Instance.IsGameOver)
    {
      if  (Vector3.Distance(AI.transform.position,Player_Controller.Instance.transform.position)<AI.Radius+AI.AttackDistance)
      {
        AI.ChangeState(AIState.Attack);
      }
      else
      {
        AI.NavMeshAgent.SetDestination(Player_Controller.Instance.transform.position);
        AI.SaveAIData();
        if (Vector3.Distance(Player_Controller.Instance.transform.position,AI.transform.position)>(AI as MonsterPlant_Contorller).AIMaxDistance)
        {
          AI.ChangeState(AIState.Idle);
          return;
        }

        CheckAndTransferMapChunk();
      }
      
    }
  }

  public override void Exit()
  {
    AI.NavMeshAgent.speed = (AI as MonsterPlant_Contorller).WalkSpeed;
    base.Exit();
  }
}
