using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : StateMachine
{
    //���һЩ������ݵĵط�
    public PlayerReuseData playerReuseData;

    public Player player { get; set; }


    public PlayerIdleState PlayerIdleState { get; private set; }
    public PlayerWalkState PlayerWalkState { get; private set; }
    public PlayerJumpState PlayerJumpState { get; private set; }
    public PLayerFallState PlayerFallState { get; private set; }

    //������ݵĵط�
    public PlayerStateMachine(Player player_in)
    {
        player = player_in;
        playerReuseData = new PlayerReuseData();

        PlayerIdleState = new PlayerIdleState(this);

        PlayerWalkState = new PlayerWalkState(this);

        PlayerJumpState = new PlayerJumpState(this);

        PlayerFallState = new PLayerFallState(this);
       
        
    }


}
