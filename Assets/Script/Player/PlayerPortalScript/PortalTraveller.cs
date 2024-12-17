using System.Collections.Generic;
using UnityEngine;

public class PortalTraveller : MonoBehaviour
{

    public GameObject graphicsObject;
    public GameObject graphicsClone { get; set; }
    public Vector3 previousOffsetFromPortal { get; set; }

    public Material[] originalMaterials { get; set; }
    public Material[] cloneMaterials { get; set; }


    //虚函数,基本的方法是让物体传送到指定的物体
    public virtual void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
    }

    // 第一次进入传送门的时候要生成本物体的一个克隆体,这样就可以看到对应的物体
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

    // 退出传送门的时候就会将克隆物体设置成为不激活,然后将sliceNormal设置成zero这样就会直接不渲染
    public virtual void ExitPortalThreshold()
    {
        graphicsClone.SetActive(false);
        // Disable slicing
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            originalMaterials[i].SetVector("sliceNormal", Vector3.zero);
        }
    }

    public void SetSliceOffsetDst(float dst, bool clone)
    {
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            if (clone)
            {
                cloneMaterials[i].SetFloat("sliceOffsetDst", dst);
            }
            else
            {
                originalMaterials[i].SetFloat("sliceOffsetDst", dst);
            }

        }
    }

    //得到该子物体下的所有的材质(Materials)
    //材质包含了:shader,texture,color,propeties反正就是映射在物体表面的物体
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
}