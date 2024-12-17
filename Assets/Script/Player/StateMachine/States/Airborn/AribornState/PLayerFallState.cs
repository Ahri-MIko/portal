using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PLayerFallState : PlayerAirState
{
    public PLayerFallState(PlayerStateMachine StateMachine) : base(StateMachine)
    {

    }

    public override void Enter()
    {
        base.Enter();
        StartAnimation(playerStateMachine.player.animationData.FallHashNumber);
    }

    public override void Update()
    {
        base.Update();
        if(playerStateMachine.playerReuseData.collisionFlags == CollisionFlags.Below || playerStateMachine.player.characterController.isGrounded)
        {
            playerStateMachine.ChangeState(playerStateMachine.PlayerIdleState);
        }
    }

    public override void Exit()
    {
        Debug.Log("Exit from falling");
        playerStateMachine.player.playerInputs.RecoverInputAction(playerStateMachine.player.playerInputs.PlayerinputActions.Move);
        base.Exit();
        StopAnimation(playerStateMachine.player.animationData.FallHashNumber);

    }
}
