using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MySingleTest : MonoSingleton<MySingleTest>
{
    protected override void Init()
    {
        print("��ʼ����");
    }
    public void Get()
    {
        Debug.Log("ִ����Get");
    }
    public void MyGet() {
        Debug.Log("�ڶ���ִ��");
    }
}

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T instance;

    public static T Instance
    {
        //ʵ�ְ������
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
