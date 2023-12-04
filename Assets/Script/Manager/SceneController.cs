using System;
using System.Collections;
using UnityEngine.SceneManagement;
using Tools;
using UnityEngine;
using UnityEngine.Pool;

public class SceneController : Singleton<SceneController>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    //场景加载
    public void LoadSceneWithIndex<T>(int sceneIndex, T someEvent) where T : Enum
    {
        StartCoroutine(LoadScene(sceneIndex, someEvent));
    }
    
    IEnumerator LoadScene<T>(int sceneIndex, T someEvent) where T : Enum
    {
        //异步加载加载场景
        GameManager.Instance?.RemoveAllEnemy();
        ObjectPool.Instance?.ClearObjectPool();
        yield return SceneManager.LoadSceneAsync(sceneIndex);
        //场景加载完毕后，调用获取目标点的函数获得目标点，将Player生成在该点
        yield return EventManager.Instance?.InvokeEvent(someEvent);
        
    }
}
