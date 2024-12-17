using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interact_Door : MonoBehaviour
{
    [SerializeField] private GameObject door;//门
    [SerializeField] GameObject OpenDoorUI;//交互提示
    private Rigidbody rb;
    Keyboard keyBoard;
    public bool isOpen = false;//门是否打开
    private Vector3 DoorClosedPos;   // 门初始位置
    public float doorOpenDistance = 3f; // 门开启时移动的距离
    public float doorSpeed = 10f;        // 门移动的速度
    public Vector3 doorDirection = Vector3.back;  // 门的开关方向，默认是向左
    public PlayerInput playerInput;
    public bool PlayerIn = false;
    

    void Start()
    {
        //playerInput.Player.Interact.performed += ctx => Interact();
        DoorClosedPos = door.transform.position;
        rb= door.GetComponent<Rigidbody>();
        // 确保 Rigidbody 没有被设置为 isKinematic
        rb.isKinematic = false;

        // 确保物体没有被限制移动
        rb.constraints = RigidbodyConstraints.None; // 没有任何轴的限制

        // 设置 Rigidbody 为 Kinematic，禁用物理影响
        rb.isKinematic = true;

        // 可选：关闭重力影响
        rb.useGravity = false;
    }

    void Update()
    {
        float doorLength = 0f;

        if (doorDirection == Vector3.right || doorDirection == Vector3.left)
        {
            doorLength = door.transform.localScale.x;  // 获取门在 X 轴上的长度
        }
        else if (doorDirection == Vector3.up || doorDirection == Vector3.down)
        {
            doorLength = door.transform.localScale.y;  // 获取门在 Y 轴上的长度
        }
        else if (doorDirection == Vector3.forward || doorDirection == Vector3.back)
        {
            doorLength = door.transform.localScale.z;  // 获取门在 Z 轴上的长度
        }
        if (isOpen)
        {
            // 左右门按照 `doorDirection` 的方向移动
            Vector3 DoorTargetPos = DoorClosedPos - doorDirection * doorOpenDistance;
            // 使用 MovePosition 来平滑地移动门
            MoveDoor(rb, DoorTargetPos, doorLength);
        }
        else if (!isOpen)
        {
            MoveDoor(rb, DoorClosedPos, doorLength);
        }

        if(Input.GetKeyDown(KeyCode.E) && PlayerIn)
        {
            //改变门的状态
            isOpen = !isOpen;

            OpenDoorUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)//当玩家进入触发器范围内
    {
     
        //弹出交互提示
        if (other.CompareTag("Player"))
        {
            PlayerIn = true;
            OpenDoorUI.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)//当玩家离开触发器范围
    {
       
        //关闭交互提示
        if (other.CompareTag("Player"))
        {
            PlayerIn = false;
            OpenDoorUI.SetActive(false);
        }
    }

    // 使用 Rigidbody MovePosition 来平滑移动门
    private void MoveDoor(Rigidbody doorRb, Vector3 targetPosition, float doorLength)
    {
        // 计算移动方向
        Vector3 moveDirection = targetPosition - doorRb.position;

        // 如果门没有到达目标位置
        if (moveDirection.magnitude > doorLength * 0.1f)
        {
            Vector3 newPosition = doorRb.position + moveDirection.normalized * doorSpeed * Time.deltaTime;
            doorRb.MovePosition(newPosition);
        }
        else
        {
            // 门已经到达目标位置，停止移动
            doorRb.MovePosition(targetPosition);
        }
    }
}


