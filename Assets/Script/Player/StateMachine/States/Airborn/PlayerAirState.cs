using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirState : PlayerStatesBase
{
    public PlayerAirState(PlayerStateMachine StateMachine) : base(StateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        StartAnimation(playerStateMachine.player.animationData.AirboneHashNumber);
    }

    public override void Update()
    {
        base.Update();
        playerStateMachine.playerReuseData.velocity.x = playerStateMachine.playerReuseData.lastHorizontalVelocity.x;
        playerStateMachine.playerReuseData.velocity.z = playerStateMachine.playerReuseData.lastHorizontalVelocity.y;

    }

    public override CollisionFlags PlayerMove(Vector3 vec)
    {
        float x = playerStateMachine.playerReuseData.velocity.x;
        float z = playerStateMachine.playerReuseData.velocity.z;
        return playerStateMachine.player.characterController.Move(new Vector3(x, playerStateMachine.playerReuseData.verticalVelocity, z)*Time.deltaTime);
        
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(playerStateMachine.player.animationData.AirboneHashNumber);

    }

}
