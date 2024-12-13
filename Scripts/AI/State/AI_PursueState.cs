
using UnityEngine;

public class AI_PursueState : AI_StateBase
{
    public override void Enter()
    {
        AI.PlayAnimation("Move");
        AI.NavMeshAgent.enabled = true;
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
                AI.SaveAIData();
                AI.NavMeshAgent.SetDestination(Player_Controller.Instance.transform.position);
                CheckAndTransferMapChunk();
            }
        }
    }

    public void CheckAndTransferMapChunk()
    {
        MapChunkController newMapChunk = MapManager.Instance.GetMapChunkByWorldPosition(AI.transform.position);
        if (AI.MapChunk!=newMapChunk)
        {
             AI.MapChunk.RemoveAIObjectOnTransfer(AI.AIData.ID);
             newMapChunk.AddAIObjectOnTransfer(AI.AIData,AI);
        }
    
    }
    public override void Exit()
    {
        AI.NavMeshAgent.enabled = false;
    }
}