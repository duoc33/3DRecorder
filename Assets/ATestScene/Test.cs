using Record;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Test : MonoBehaviour
{
    public GameObject target1;
    public GameObject target2;
    public LinkedList<GameObject> gameObjects = new LinkedList<GameObject>();
    void Start()
    {
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) 
        {
            InitGameObject();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Destroy(gameObjects.First.Value);
            gameObjects.RemoveFirst();

        }
        if (Input.GetKey(KeyCode.RightArrow)) 
        {
            if(target1!=null)
                target1.transform.Translate(Vector3.right*Time.deltaTime*3);
            if (target2 != null)
                target2.transform.Translate(Vector3.right * Time.deltaTime * 3);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (target1 != null)
                target1.transform.Translate(Vector3.left * Time.deltaTime * 3);
            if (target2 != null)
                target2.transform.Translate(Vector3.left * Time.deltaTime * 3);
        }

    }
    private void InitGameObject() 
    {
        GameObject target =Instantiate(Resources.Load<GameObject>("MyGameLogic/Object/Sphere (2)"));
        RecordObjectView[] recordObjectViews = target.GetComponentsInChildren<RecordObjectView>(true);
        target1 = recordObjectViews[0].gameObject;
        target2 = recordObjectViews[1].gameObject;
        gameObjects.AddLast(target1);
        gameObjects.AddLast(target2);
    }
}
