using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public float moveSpeed = 5f; // 控制移动速度
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // 获取 Rigidbody 组件
    }

    void Update()
    {
        // 获取 WASD 输入
        float moveX = Input.GetAxis("Horizontal"); // A/D 或 左/右箭头
        float moveZ = Input.GetAxis("Vertical");   // W/S 或 上/下箭头

        // 根据输入计算移动方向
        Vector3 movement = new Vector3(moveX, 0f, moveZ).normalized * moveSpeed * Time.deltaTime;

        // 使用 Rigidbody 的 MovePosition 来更新物体的位置
        rb.MovePosition(transform.position + movement);
    }

}
