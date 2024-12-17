using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeCaught : MonoBehaviour
{
    //����������õ���������Ľű�
    public GameObject graphicsObject;//�������屾��
    public GameObject graphicsClone { get; set; }

    public Vector3 previousOffsetFromPortal;

    public Material[] originalMaterials { get; set; }
    public Material[] cloneMaterials { get; set; }

    public bool throughPortal { get; set; }

    public bool sideOfportal = true;


    //�ж��Լ��Ƿ�ץס,��Ϊ�ڳ����ӻ��кܶ�Cube��ͬһ��������,Ȼ����Щ���������Ҫ�ж�����Ƿ�ץס�˶��������Ƿ����������
    public bool getCaught { get; set; }

    //���봫���ŵ�ʱ������һ������Ʒ
    public void Start()
    {
        throughPortal = false;
    }
    public void EnterPortal()
    {
        if (graphicsClone == null)
        {
            graphicsClone = Instantiate(graphicsObject);
            graphicsClone.transform.parent = graphicsObject.transform.parent;
            graphicsClone.transform.localScale = graphicsObject.transform.localScale;
        }
        else
        {
            graphicsClone.SetActive(true);
        }
    }

    public virtual void EnterPortalThreshold()
    {
        if (graphicsClone == null)
        {
            graphicsClone = Instantiate(graphicsObject);
            graphicsClone.transform.parent = graphicsObject.transform.parent;
            graphicsClone.transform.localScale = graphicsObject.transform.localScale;
            originalMaterials = GetMaterials(graphicsObject);
            cloneMaterials = GetMaterials(graphicsClone);
        }
        else
        {
            graphicsClone.SetActive(true);
        }
    }



    //�����Ӧ��ֵ
    Material[] GetMaterials(GameObject g)
    {
        var renderers = g.GetComponentsInChildren<MeshRenderer>();
        var matList = new List<Material>();
        foreach (var renderer in renderers)
        {
            foreach (var mat in renderer.materials)
            {
                matList.Add(mat);
            }
        }
        return matList.ToArray();
    }

    public virtual void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
    }

    //����λ��,�ڴ��������ŵ�ʱ��Ҫ�õ�
    public void TransPosition()
    {
        //���������ֱ�ӺͿ�¡�彻��
        transform.position = graphicsClone.transform.position;
        transform.rotation = graphicsClone.transform.rotation;
    }

    public void TransVelocity(Transform fromPortal, Transform toPortal)
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.velocity = toPortal.TransformVector(fromPortal.InverseTransformVector(rb.velocity));
        //��ֹ��ǿ�Ƶ�����ת����ĳ��ֵ��������
        Physics.SyncTransforms();
    }


    public virtual void ExitPortalThreshold()
    {
        graphicsClone.SetActive(false);
        // Disable slicing
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            originalMaterials[i].SetVector("sliceNormal", Vector3.zero);
        }
    }
}
