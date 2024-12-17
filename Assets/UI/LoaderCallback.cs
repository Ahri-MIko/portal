using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LoaderCallback : MonoBehaviour
{
    private bool isFirstUpdate = true;

    private void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;
            // 延迟2秒调用Loader.LoaderCallback方法
            Invoke("LoadTargetScene", 2f);
        }
    }

    private void LoadTargetScene()
    {
        Loader.LoaderCallback();
    }
}