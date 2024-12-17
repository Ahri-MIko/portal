using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState : PlayerGroundStateBase
{
    public PlayerWalkState(PlayerStateMachine StateMachine) : base(StateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        StartAnimation(playerStateMachine.player.animationData.WalkHashNumber);
    }
    public override void Update()
    {
        base.Update();
        Vector2 velocity = new Vector2(playerStateMachine.playerReuseData.velocity.x, playerStateMachine.playerReuseData.velocity.z);
        Vector3 input = GetInputDir();
        if ( input.magnitude<0.1f)
        {
            playerStateMachine.ChangeState(playerStateMachine.PlayerIdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(playerStateMachine.player.animationData.WalkHashNumber);
    }
}
