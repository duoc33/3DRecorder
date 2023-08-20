using Record;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
[DisallowMultipleComponent]
public class Test : MonoBehaviour
{
    public GameObject[] gameObjects;
    void Start()
    {
        Recorder.Instance.SetSaveOrReadPath(Application.streamingAssetsPath, "Record");
        Recorder.Instance.SetMode(StateType.Recording);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            gameObjects[2].transform.SetParent(gameObjects[0].transform);
        }
        if (Input.GetKey(KeyCode.RightArrow)) 
        {
            gameObjects[0].transform.Translate(Vector3.right*Time.deltaTime*3);
           
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            gameObjects[0].transform.Translate(Vector3.left * Time.deltaTime * 3);
            
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {

        }
        if (Input.GetKey(KeyCode.DownArrow))
        {

        }
        if (Input.GetKey(KeyCode.D)) {
            gameObjects[1].transform.Translate(Vector3.right * Time.deltaTime * 3);
        }
        if (Input.GetKey(KeyCode.A))
        {
            gameObjects[1].transform.Translate(Vector3.left * Time.deltaTime * 3);
        }
        if (Input.GetKey(KeyCode.W))
        {
            
        }
        if (Input.GetKey(KeyCode.S))
        {
            
        }

    }
    private void RemoveComponent(GameObject go, Type definedType)
    {
        foreach (var component in go.GetComponents<Component>())
        {
            if (component.GetType() == definedType)
            {
                Destroy(component);
            }
        }
    }
}



