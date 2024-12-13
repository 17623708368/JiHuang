using System.Collections;
using System.Collections.Generic;
using JKFrame;
using UnityEngine;

public class AI_IdelState:AI_StateBase
{
    public Coroutine GoPatrolCoroutine;
    public override void Enter()
    {
        //播放待机动画动画
        AI.PlayAnimation( "Idle");
      
        //开启协程存储一个
        GoPatrolCoroutine=  MonoManager.Instance.StartCoroutine(goPatrol());
        if (Random.Range(0,30)==0)
        {
           AI.PlayAudio(AIState.Idle,1);
        }
    }

    public override void Update()
    {
        if (AI.AIDistance>0)
        {
            if (Vector3.Distance(Player_Controller.Instance.transform.position,AI.transform.position)<AI.AIDistance)
            {
                AI.ChangeState(AIState.Pursue);
            }
        }
    }

    public IEnumerator goPatrol()
    {
      yield return CoroutineTool.GetIEnumerator(Random.Range(2,6));
      AI.ChangeState(AIState.Patrol);
         
    }

    public override void Exit()
    {
        MonoManager.Instance.StopCoroutine(GoPatrolCoroutine);
    }
}