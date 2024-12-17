using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIFollow : MonoBehaviour
{
    public bool isWorldUI;
    //观察摄像机
    public Camera camera;
    //跟随的目标
    public Transform target;
    //跟随的偏移量
    public Vector3 offset;

    void Update()
    {
        if (target == null) 
        { 
            return;
        }
        if (camera == null)
        { 
            camera = Camera.main;
        }
        if (!isWorldUI) 
        transform.position = camera.WorldToScreenPoint(target.position+offset);
        else
            transform.position = target.position+offset;
       
    }
}
