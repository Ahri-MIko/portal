using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerGroundStateBase
{
    public PlayerIdleState(PlayerStateMachine StateMachine) : base(StateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        StartAnimation(playerStateMachine.player.animationData.IdleHashNumber);
    }
    public override void Update()
    {
        base.Update();
        Vector2 velocity = new Vector2(playerStateMachine.playerReuseData.velocity.x, playerStateMachine.playerReuseData.velocity.z);
        Vector3 input = GetInputDir();
        if ( input.magnitude > 0.1f)
        {
            playerStateMachine.ChangeState(playerStateMachine.PlayerWalkState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(playerStateMachine.player.animationData.IdleHashNumber);
    }
}
