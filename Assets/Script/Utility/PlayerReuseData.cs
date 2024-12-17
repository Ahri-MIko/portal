using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReuseData
{
    public Vector2 MoveInputs { get; set; }//共有的属性
    public Vector2 LookInputs { get; set; }

    //玩家的一些共有属性
    public float yaw;
    public float pitch;
    public float smoothYaw;
    public float smoothPitch;

    public float yawSmoothV;
    public float pitchSmoothV;
    public float verticalVelocity;
    public Vector3 velocity;
    public Vector3 smoothV;
    public Vector3 rotationSmoothVelocity;
    public Vector3 currentRotation;
    public Vector2 lastHorizontalVelocity;

    public bool jumping;
    public float lastGroundedTime;
    public bool disabled;

    public CollisionFlags collisionFlags;

    public Vector3 targetRotation;
    public float rotationVelocity;
    
}
