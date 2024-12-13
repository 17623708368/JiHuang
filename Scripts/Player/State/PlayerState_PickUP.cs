using System;
using System.Collections;
using JKFrame;
using UnityEngine;


class PlayerState_PickUP : PlayerStateBase
{
    private bool isPlayerOver=true;
    public override void Enter()
    {
        player.PlayAnimation("PickUp");
        this.StartCoroutine(DoMovement());
    }

    public override void Update()
    {
        if (!isPlayerOver) return;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (h != 0 || v != 0)
        {
            player.ChangeState(PlayerState.Move);
        }
    }

    IEnumerator DoMovement()
    {
        isPlayerOver = false;
        yield return new WaitForSeconds(0.5f);
        isPlayerOver=true;
    }
}