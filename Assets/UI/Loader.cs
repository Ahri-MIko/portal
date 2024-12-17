using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene
    {
        MainMenuScene,
        GameScene,
        LoadingScene
    }

    public static Scene targetScene;

    public static void load(Scene targetScene)
    {
        Loader.targetScene = targetScene;
        SceneManager.LoadScene(Scene.LoadingScene.ToString()); // 这里使用字符串，因为LoadScene方法需要字符串参数
    }

    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString()); // 将枚举转换为字符串
    }
}