using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//这个脚本存储了角色的一些动画的bool数据

[Serializable]
public class AnimationData
{
    [field:Header("MainStates")]
    [field: SerializeField] public string GroundParameters { get; private set; } = "Grounded";
    [field: SerializeField] public string AirboneParameters { get; private set; } = "Airborn";

    [field:Header("DetailStates")]
    [field: SerializeField] public string idleParameters { get; private set; } = "isIdleing";
    [field: SerializeField] public string walkParameters { get; private set; } = "isWalking";
    [field: SerializeField] public string fallParameters { get; private set; } = "IsFalling";
    [field: SerializeField] public string jumpParameters { get; private set; } = "isJumping";

    public int GroundHashNumber { get; private set; }
    public int AirboneHashNumber { get; private set; }

    public int IdleHashNumber { get; private set; }
    public int WalkHashNumber {  get; private set; }
    public int FallHashNumber {  get; private set; }
    public int JumpHashNumber { get; private set; }


    public void initialize()
    {
        GroundHashNumber = Animator.StringToHash(GroundParameters);
        AirboneHashNumber = Animator.StringToHash(AirboneParameters);

        IdleHashNumber = Animator.StringToHash(idleParameters);
        WalkHashNumber = Animator.StringToHash(walkParameters);
        FallHashNumber = Animator.StringToHash(fallParameters);
        JumpHashNumber = Animator.StringToHash(jumpParameters);
    }

}
