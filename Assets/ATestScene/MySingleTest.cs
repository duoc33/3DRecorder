using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MySingleTest : MonoSingleton<MySingleTest>
{
    protected override void Init()
    {
        print("初始化了");
    }
    public void Get()
    {
        Debug.Log("执行了Get");
    }
    public void MyGet() {
        Debug.Log("第二次执行");
    }
}

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T instance;

    public static T Instance
    {
        //实现按需加载
        get
        {
            if (instance != null) return instance;
            instance = FindObjectOfType<T>();
            if (instance == null)
            {
                new GameObject("Singleton of " + typeof(T)).AddComponent<T>();
            }
            return instance;
        }
    }
    private void Awake()
    {
        instance = this as T;
        Init();
    }
    protected abstract void Init();
}
