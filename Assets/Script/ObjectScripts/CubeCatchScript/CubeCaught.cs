using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeCaught : MonoBehaviour
{
    //这个是用来得到传送物体的脚本
    public GameObject graphicsObject;//传送物体本体
    public GameObject graphicsClone { get; set; }

    public Vector3 previousOffsetFromPortal;

    public Material[] originalMaterials { get; set; }
    public Material[] cloneMaterials { get; set; }

    public bool throughPortal { get; set; }

    public bool sideOfportal = true;


    //判断自己是否被抓住,因为在场景钟会有很多Cube在同一个场景中,然而有些情况可能需要判断玩家是否抓住了东西并且是否是这个方块
    public bool getCaught { get; set; }

    //进入传送门的时候制造一个复制品
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



    //获得相应的值
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

    //交换位置,在穿过传送门的时候要用到
    public void TransPosition()
    {
        //这个函数是直接和克隆体交换
        transform.position = graphicsClone.transform.position;
        transform.rotation = graphicsClone.transform.rotation;
    }

    public void TransVelocity(Transform fromPortal, Transform toPortal)
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.velocity = toPortal.TransformVector(fromPortal.InverseTransformVector(rb.velocity));
        //防止在强制的物理转化后的出现的物理错误
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
