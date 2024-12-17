using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Data", menuName = "Charactors/Player")]
public class PlayerSO : ScriptableObject
{
    //物理效果的参数设置
    public float walkSpeed = 3;
    public float runSpeed = 6;
    public float smoothMoveTime = 0.1f;
    public float jumpForce = 8;
    public float gravity = 18;
    public float rotationSpeed = 10f;
    public float InputRotationMax = 6;//最大能扭动的数值

    //鼠标的设置,pitch俯仰角(X),Yaw偏航角(Y),Roll滚动角(Z)
    public bool lockCursor;
    public float mouseSensitivity = 10;
    public Vector2 pitchMinMax = new Vector2(-40, 85);
    public float rotationSmoothTime = 0.1f;
    
}
