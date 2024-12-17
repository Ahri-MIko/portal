using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerStatesBase : IState
{
    
    public PlayerStateMachine playerStateMachine { get; private set; }

    public PlayerStatesBase(PlayerStateMachine StateMachine)
    {
        playerStateMachine = StateMachine;//在创建状态的时候会得到状态
    }


    public virtual void Enter()
    {
        //Debug.Log("State:" + GetType().Name);//打印名字
        AddInputCallback();
    }

    public virtual void Exit()
    {
        DeleteInputCallback();
    }

    //所有状态共享的玩家输入
    public void HandleInput()
    {
        ReadPlayerInputs();
    }

    public void onAnimationEnter()
    {
        
    }

    public void OnAnimationExit()
    {
       
    }

    public void OnAnimationTransitionToStateary()
    {
        
    }

    public void OnTriggerEnter(Collider collider)
    {
      
    }

    public virtual void OnTriggerExit(Collider collider)
    {
       
    }

    public virtual void Update()
    {
        RotateCamera();
        RotateToTarget();
        Move();
    }

    public  void PhysicsUpdate()
    {
     
    }


    

    #region Get PlayerInput
    void ReadPlayerInputs()
    {
        playerStateMachine.playerReuseData.MoveInputs = playerStateMachine.player.playerInputs.PlayerinputActions.Move.ReadValue<Vector2>();
        playerStateMachine.playerReuseData.LookInputs = playerStateMachine.player.playerInputs.PlayerinputActions.Look.ReadValue<Vector2>();
    }
    #endregion

    #region Player Move
    void RotateCamera()
    {
        // 缓存引用，提高性能
        PlayerSO tempSO = playerStateMachine.player.playerso;
        PlayerReuseData tempReuseableData = playerStateMachine.playerReuseData;
        Vector2 look = tempReuseableData.LookInputs;

        // 检查输入是否超出最大值
        if (look.magnitude > tempSO.InputRotationMax)
        {
            look = Vector2.zero;
        }

        // 更新相机角度
        tempReuseableData.yaw += look.x * tempSO.mouseSensitivity;
        tempReuseableData.pitch -= look.y * tempSO.mouseSensitivity;

        // 限制俯仰角度
        tempReuseableData.pitch = Mathf.Clamp(tempReuseableData.pitch, tempSO.pitchMinMax.x, tempSO.pitchMinMax.y);

        // 使用Lerp进行平滑插值
        tempReuseableData.smoothPitch = Mathf.Lerp(
            tempReuseableData.smoothPitch,
            tempReuseableData.pitch,
            Time.deltaTime * tempSO.rotationSpeed
        );

        tempReuseableData.smoothYaw = Mathf.Lerp(
            tempReuseableData.smoothYaw,
            tempReuseableData.yaw,
            Time.deltaTime * tempSO.rotationSpeed
        );

        /*
          transform.eulerAngles = Vector3.up * smoothYaw;
        cam.transform.localEulerAngles = Vector3.right * smoothPitch;*/

        playerStateMachine.player.Point.eulerAngles = Vector3.up * tempReuseableData.smoothYaw;
        // 应用旋转
        playerStateMachine.player.cam.transform.localEulerAngles = new Vector3(
            tempReuseableData.smoothPitch,
            0.0f,
            0.0f
        );
    }
    void Move()
    {
        CharacterController controller = playerStateMachine.player.characterController;
        Vector3 worldDir = GetWorldInput();
        float currentSpeed = playerStateMachine.player.playerso.runSpeed;

        // 从数据中读取当前垂直速度
        float verticalVelocity = playerStateMachine.playerReuseData.verticalVelocity;

        // 应用重力
        verticalVelocity -= playerStateMachine.player.playerso.gravity * Time.deltaTime;

        Vector3 targetVelocity = worldDir * currentSpeed;
        playerStateMachine.playerReuseData.velocity = Vector3.SmoothDamp(
            playerStateMachine.playerReuseData.velocity,
            targetVelocity,
            ref playerStateMachine.playerReuseData.smoothV,
            playerStateMachine.player.playerso.smoothMoveTime
        );

        Vector3 vec = playerStateMachine.playerReuseData.velocity;
        vec.y = verticalVelocity; // 使用更新后的垂直速度

        playerStateMachine.playerReuseData.collisionFlags = PlayerMove(vec);

        // 碰到地面时重置垂直速度
        if (playerStateMachine.playerReuseData.collisionFlags == CollisionFlags.Below)
        {
            playerStateMachine.playerReuseData.jumping = false;
            verticalVelocity = 0;
            playerStateMachine.playerReuseData.lastGroundedTime = Time.time;
        }

        //明天要做的就是将这个跳跃的逻辑写到里面,然后设置相应的参数来控制



        
        playerStateMachine.playerReuseData.verticalVelocity = verticalVelocity;
    }
    public virtual CollisionFlags PlayerMove(Vector3 vec)
    {
        return playerStateMachine.player.characterController.Move(vec * Time.deltaTime);
    }

    void RotateToTarget()
    {
        if (GetInputDir() == Vector3.zero)
            return;
        float InputDirection = GetInputDirection(GetInputDir());
        InputDirection = AddCamRotation(InputDirection);
        if (InputDirection != playerStateMachine.playerReuseData.targetRotation.y)
        {
            UpdateTargetRotationData(InputDirection);
        }

        float currentrotation = playerStateMachine.player.playerModel.transform.eulerAngles.y;
        float EulerRotation = Mathf.SmoothDampAngle(currentrotation, playerStateMachine.playerReuseData.targetRotation.y, ref playerStateMachine.playerReuseData.rotationVelocity, playerStateMachine.player.playerso.rotationSmoothTime);
        Quaternion QuanionRotation = Quaternion.Euler(new Vector3(0f, EulerRotation, 0f));
        playerStateMachine.player.playerModel.transform.rotation = QuanionRotation;
    }

    Vector3 GetWorldInput()
    {
        Vector3 worldDir = playerStateMachine.player.cam.transform.TransformDirection(GetInputDir());
        // 将 y 设为 0 并重新归一化，这样保持水平移动速度恒定
        worldDir.y = 0;
        if (worldDir.magnitude > 0)
        {
            worldDir.Normalize();
        }
        return worldDir;
    }

    public Vector3 GetInputDir()//获得当前输入的方向
    {
        Vector3 inputDir = playerStateMachine.playerReuseData.MoveInputs;

        return new Vector3(inputDir.x,0,inputDir.y);
    }

    float GetInputDirection(Vector3 angle)
    {
        //得到玩家输入的角度数
        float InputDirection = TransV2F(angle);
        //这里要注意的是角度的旋转最好保持在正数,不然会找不到最短路径
        if (InputDirection < 0f)
        {
            InputDirection += 360;
        }

        return InputDirection;
    }
    protected float TransV2F(Vector3 direction)
    {
        return MathF.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
    }

    float AddCamRotation(float InputDirection)
    {
        //得到相机的角度后将此角度添加到输入的角度上去
        InputDirection = InputDirection + playerStateMachine.player.cam.transform.eulerAngles.y;
        //也是为了确保旋转的角度过大导致错误,但是好像测试的时候没啥问题
        if (InputDirection > 360f)
        {
            InputDirection -= 360f;
        }
        return InputDirection;
    }

    void UpdateTargetRotationData(float inputDirection)
    {
        playerStateMachine.playerReuseData.targetRotation.y = inputDirection;
        
    }
    #endregion

    #region Player Input Events
    public  virtual void  AddInputCallback()
    {
        playerStateMachine.player.playerInputs.PlayerinputActions.Jump.started += ChangeTojumpState;
    }

    public virtual void DeleteInputCallback()
    {
        playerStateMachine.player.playerInputs.PlayerinputActions.Jump.started -= ChangeTojumpState;
    }

    private void ChangeTojumpState(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        playerStateMachine.ChangeState(playerStateMachine.PlayerJumpState);//转化到jump阶段
    }
    #endregion

    #region Animation
    public void StartAnimation(int Animation)
    {
        playerStateMachine.player.animator.SetBool(Animation,true);
    }
    public void StopAnimation(int Animation)
    {
        playerStateMachine.player.animator.SetBool(Animation, false);
    }
    #endregion
}
