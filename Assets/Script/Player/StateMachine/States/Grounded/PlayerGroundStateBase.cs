using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundStateBase : PlayerStatesBase
{
    public PlayerGroundStateBase(PlayerStateMachine StateMachine) : base(StateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        StartAnimation(playerStateMachine.player.animationData.GroundHashNumber);
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(playerStateMachine.player.animationData.GroundHashNumber);
        playerStateMachine.playerReuseData.lastHorizontalVelocity = new Vector2(playerStateMachine.playerReuseData.velocity.x, playerStateMachine.playerReuseData.velocity.z);
    }

}
