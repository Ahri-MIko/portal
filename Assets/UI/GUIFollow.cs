using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIFollow : MonoBehaviour
{
    public bool isWorldUI;
    //�۲������
    public Camera camera;
    //�����Ŀ��
    public Transform target;
    //�����ƫ����
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
