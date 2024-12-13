using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;

/// <summary>
/// 玩家待机状态
/// </summary>
public class Player_Idle : PlayerStateBase
{
    public override void Enter()
    {
         player.PlayAnimation("Idle");
       
    }

    public override void Update()
    {
        //  玩如果有任何移动相关的按键，就切去Move状态
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (h != 0 || v != 0) player.ChangeState(PlayerState.Move);
    }
}
