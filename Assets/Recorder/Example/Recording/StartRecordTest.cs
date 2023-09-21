using Record;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Record
{
    public class StartRecordTest : MonoBehaviour
    {
        void Start()
        {
            //选择存储路径 和 文件名
            Recorder.Instance.SetSaveOrReadPath(Application.streamingAssetsPath, "Record");
            //选择模式Recording ，自动开始录制
            Recorder.Instance.SetMode(StateType.Recording);
        }

        #region 测试操作
        private GameObject target1;
        private GameObject target2;
        private LinkedList<GameObject> gameObjects = new LinkedList<GameObject>();
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                Recorder.Instance.DestoryInstance();
            }
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
                if (target1 != null)
                    target1.transform.Translate(Vector3.right * Time.deltaTime * 3);
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
            GameObject target = Instantiate(Resources.Load<GameObject>("MyGameLogic/Object/Sphere (2)"));
            RecordObjectView[] recordObjectViews = target.GetComponentsInChildren<RecordObjectView>(true);
            target1 = recordObjectViews[0].gameObject;
            target2 = recordObjectViews[1].gameObject;
            gameObjects.AddLast(target1);
            gameObjects.AddLast(target2);
        }
        #endregion

    }
}

