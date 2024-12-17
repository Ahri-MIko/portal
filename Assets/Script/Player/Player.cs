using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : PortalTraveller
{
    [field: Header("引用")]

    public CharacterController characterController;
    private PlayerStateMachine playerStateMachine;
    public MoveInputs playerInputs;
    public PlayerSO playerso;
    public Camera cam;
    public Transform playerModel;
    public Animator animator { get; private set; }
    [SerializeField]public Transform Point;
    [field: SerializeField] public AnimationData animationData { get; private set; }
    


    private void Awake()
    {
        characterController = GetComponent<CharacterController>();//这次用的是Charactor来控制玩家的移动
        playerStateMachine = new PlayerStateMachine(this);
        playerInputs = GetComponent<MoveInputs>();
        cam = Camera.main;
        animator = GetComponentInChildren<Animator>();
        animationData.initialize();//初始化所有的哈希编号

        FocusCursor();
    }

    private void Start()
    {
        PlayerInitialize();
        playerStateMachine.ChangeState(playerStateMachine.PlayerIdleState);//一开始转入idle状态
    }

    private void Update()
    {
        playerStateMachine.HandleInput();
        playerStateMachine.Update();
    }

    #region Player Init
    void PlayerInitialize()
    {
        PlayerReuseData tempReuseData = playerStateMachine.playerReuseData;
        tempReuseData.yaw = cam.transform.eulerAngles.y;
        tempReuseData.pitch = cam.transform.eulerAngles.x;
        tempReuseData.smoothYaw = tempReuseData.yaw;
        tempReuseData.smoothPitch = tempReuseData.pitch;
    }

    void FocusCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ReleaseCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    #endregion

    #region Portal Methods
    public override void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        Vector3 eulerRot = rot.eulerAngles;
        float delta = Mathf.DeltaAngle(playerStateMachine.playerReuseData.smoothYaw, eulerRot.y);
        playerStateMachine.playerReuseData.yaw += delta;
        playerStateMachine.playerReuseData.smoothYaw += delta;
        playerStateMachine.player.Point.eulerAngles = Vector3.up * playerStateMachine.playerReuseData.smoothYaw;
        //速度转化方法
        playerStateMachine.playerReuseData.velocity = toPortal.TransformVector(fromPortal.InverseTransformVector(playerStateMachine.playerReuseData.velocity));
        //防止在强制的物理转化后的出现的物理错误
        Physics.SyncTransforms();
    }

    #endregion
}
