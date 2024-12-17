using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : PortalTraveller
{

    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //这里的逻辑需要修改,后面的关卡可能需要保持之类的
        Destroy(gameObject, 5f);

    }

    //传送的方法
    public override void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        //转化位置
        transform.position = pos;
        //速度转化方法
        rb.velocity = toPortal.TransformVector(fromPortal.InverseTransformVector(rb.velocity));
        //防止在强制的物理转化后的出现的物理错误
        Physics.SyncTransforms();
    }
}