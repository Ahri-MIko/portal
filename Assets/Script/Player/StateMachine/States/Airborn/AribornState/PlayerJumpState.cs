using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerAirState
{
    public PlayerJumpState(PlayerStateMachine StateMachine) : base(StateMachine)
    {
    }

    // Start is called before the first frame update
    public override void Enter()
    {
        base.Enter();
        StartAnimation(playerStateMachine.player.animationData.JumpHashNumber);        
        playerStateMachine.player.playerInputs.DisableInputAction(playerStateMachine.player.playerInputs.PlayerinputActions.Move);//��Ծ��״̬�½����������
        if (playerStateMachine.player.characterController.isGrounded || !playerStateMachine.playerReuseData.jumping)
        {
            playerStateMachine.playerReuseData.jumping = true;
            playerStateMachine.playerReuseData.verticalVelocity = playerStateMachine.player.playerso.jumpForce;//���ڴ�ֱ����
        }

    }

    public override void Update()
    {
        base.Update();
        if (playerStateMachine.playerReuseData.verticalVelocity <=0)
        {
            playerStateMachine.ChangeState(playerStateMachine.PlayerFallState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(playerStateMachine.player.animationData.JumpHashNumber);
    }
}
