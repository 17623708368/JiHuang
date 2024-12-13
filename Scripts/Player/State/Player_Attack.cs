using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;

/// <summary>
/// Íæ¼Ò¹¥»÷×´Ì¬
/// </summary>
public class Player_Attack : PlayerStateBase
{
    public Quaternion attackDir;
    public override void Enter()
    {
        attackDir = player.attackDir;
        player.PlayAnimation("Attack");
    }
    public override void Update()
    {
        // Ðý×ªµ½¹¥»÷·½Ïò
        player.playerTransform.localRotation = Quaternion.Slerp(player.playerTransform.localRotation, attackDir, Time.deltaTime * player.rotateSpeed * 2);
    }
}
