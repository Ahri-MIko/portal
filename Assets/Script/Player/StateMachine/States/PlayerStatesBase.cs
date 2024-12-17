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
        playerStateMachine = StateMachine;//�ڴ���״̬��ʱ���õ�״̬
    }


    public virtual void Enter()
    {
        //Debug.Log("State:" + GetType().Name);//��ӡ����
        AddInputCallback();
    }

    public virtual void Exit()
    {
        DeleteInputCallback();
    }

    //����״̬������������
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
        // �������ã��������
        PlayerSO tempSO = playerStateMachine.player.playerso;
        PlayerReuseData tempReuseableData = playerStateMachine.playerReuseData;
        Vector2 look = tempReuseableData.LookInputs;

        // ��������Ƿ񳬳����ֵ
        if (look.magnitude > tempSO.InputRotationMax)
        {
            look = Vector2.zero;
        }

        // ��������Ƕ�
        tempReuseableData.yaw += look.x * tempSO.mouseSensitivity;
        tempReuseableData.pitch -= look.y * tempSO.mouseSensitivity;

        // ���Ƹ����Ƕ�
        tempReuseableData.pitch = Mathf.Clamp(tempReuseableData.pitch, tempSO.pitchMinMax.x, tempSO.pitchMinMax.y);

        // ʹ��Lerp����ƽ����ֵ
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
        // Ӧ����ת
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

        // �������ж�ȡ��ǰ��ֱ�ٶ�
        float verticalVelocity = playerStateMachine.playerReuseData.verticalVelocity;

        // Ӧ������
        verticalVelocity -= playerStateMachine.player.playerso.gravity * Time.deltaTime;

        Vector3 targetVelocity = worldDir * currentSpeed;
        playerStateMachine.playerReuseData.velocity = Vector3.SmoothDamp(
            playerStateMachine.playerReuseData.velocity,
            targetVelocity,
            ref playerStateMachine.playerReuseData.smoothV,
            playerStateMachine.player.playerso.smoothMoveTime
        );

        Vector3 vec = playerStateMachine.playerReuseData.velocity;
        vec.y = verticalVelocity; // ʹ�ø��º�Ĵ�ֱ�ٶ�

        playerStateMachine.playerReuseData.collisionFlags = PlayerMove(vec);

        // ��������ʱ���ô�ֱ�ٶ�
        if (playerStateMachine.playerReuseData.collisionFlags == CollisionFlags.Below)
        {
            playerStateMachine.playerReuseData.jumping = false;
            verticalVelocity = 0;
            playerStateMachine.playerReuseData.lastGroundedTime = Time.time;
        }

        //����Ҫ���ľ��ǽ������Ծ���߼�д������,Ȼ��������Ӧ�Ĳ���������



        
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
        // �� y ��Ϊ 0 �����¹�һ������������ˮƽ�ƶ��ٶȺ㶨
        worldDir.y = 0;
        if (worldDir.magnitude > 0)
        {
            worldDir.Normalize();
        }
        return worldDir;
    }

    public Vector3 GetInputDir()//��õ�ǰ����ķ���
    {
        Vector3 inputDir = playerStateMachine.playerReuseData.MoveInputs;

        return new Vector3(inputDir.x,0,inputDir.y);
    }

    float GetInputDirection(Vector3 angle)
    {
        //�õ��������ĽǶ���
        float InputDirection = TransV2F(angle);
        //����Ҫע����ǽǶȵ���ת��ñ���������,��Ȼ���Ҳ������·��
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
        //�õ�����ĽǶȺ󽫴˽Ƕ���ӵ�����ĽǶ���ȥ
        InputDirection = InputDirection + playerStateMachine.player.cam.transform.eulerAngles.y;
        //Ҳ��Ϊ��ȷ����ת�ĽǶȹ����´���,���Ǻ�����Ե�ʱ��ûɶ����
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
        playerStateMachine.ChangeState(playerStateMachine.PlayerJumpState);//ת����jump�׶�
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
