using UnityEngine;

//主要是用来相机的不同顺序的渲染,达到传送门的效果
public class MainCamera : MonoBehaviour
{

    Portal[] portals;

    void Awake()
    {
        portals = FindObjectsOfType<Portal>();
    }

    void OnPreCull()
    {

        //先渲染物体切片后的样子
        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].PrePortalRender();
        }

        //再渲染门中门(可能没有),确保再上一步之后保证渲染正确
        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].Render();
        }
        //渲染后得到正确的门的厚度,也必须再上一步的后面以保证最终呈现的效果正确
        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].PostPortalRender();
        }

    }

    private void Update()
    {

    }

}