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
        //������߼���Ҫ�޸�,����Ĺؿ�������Ҫ����֮���
        Destroy(gameObject, 5f);

    }

    //���͵ķ���
    public override void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        //ת��λ��
        transform.position = pos;
        //�ٶ�ת������
        rb.velocity = toPortal.TransformVector(fromPortal.InverseTransformVector(rb.velocity));
        //��ֹ��ǿ�Ƶ�����ת����ĳ��ֵ��������
        Physics.SyncTransforms();
    }
}