using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
namespace RuntimeLogic
{
    public class RotateCam : MonoBehaviour
    {
        private Transform CamMoveTarget;
        private Transform ThisTarget;
        private float wheelSpeed = 2;
        public float camDisMultiple = 0.5f;//�����Ĭ�Ͼ��뱶��
        public Vector2 FovLimit;
        private void Awake()
        {
            if (CamMoveTarget == null)
            {
                CamMoveTarget = new GameObject("MoveToTarget").transform;
            }
        }
        void Update()
        {
            if (ThisTarget != null)
            {
                camerarotate();
                camerazoom();
            }
        }
        /// <summary>
        /// ָ�����λ�úͽǶ�
        /// </summary>
        /// <param name="camV3">�����λ�úͽǶ�</param>
        /// <param name="camTaget">�۲��λ��</param>
        public void SetCamPos(Transform camV3, Transform camTaget)
        {
            ThisTarget = camTaget;
            this.transform.DORotate(camV3.eulerAngles, 1);
            this.transform.DOMove(camV3.position, 1);
            this.transform.GetComponent<Camera>().DOFieldOfView(60, 1);
        }
        /// <summary>
        /// ����Ӧ��ѡ��۲��
        /// </summary>
        /// <param name="camTaget">�۲������</param>
        public void SetCamPos(Transform camTarget)
        {
            ThisTarget = camTarget;
            float dis = BestSize(camTarget);
            CamMoveTarget.position = new Vector3(camTarget.position.x + dis, camTarget.position.y + dis, camTarget.position.z + dis);
            CamMoveTarget.LookAt(camTarget.position);
            this.transform.DORotate(CamMoveTarget.eulerAngles, 1);
            this.transform.DOMove(CamMoveTarget.position, 1);
            this.transform.GetComponent<Camera>().DOFieldOfView(60, 1);
        }
        /// <summary>
        /// ���ݴ��������ģ��size�������������ĺ��ʾ���
        /// </summary>
        /// <param name="tran"></param>
        /// <returns></returns>
        private float BestSize(Transform tran)
        {
            if (tran.GetComponent<MeshFilter>() == null)
            {
                return 0;
            }
            float Sx = tran.GetComponent<MeshFilter>().mesh.bounds.size.x * Mathf.Abs(tran.localScale.x);
            float Sy = tran.GetComponent<MeshFilter>().mesh.bounds.size.y * Mathf.Abs(tran.localScale.y);
            float Sz = tran.GetComponent<MeshFilter>().mesh.bounds.size.z * Mathf.Abs(tran.localScale.z);
            float Bb = Sx;
            if (Sy > Bb) { Bb = Sy; }
            if (Sz > Bb) { Bb = Sz; }
            wheelSpeed = 2 + Bb * 0.12f;
            return Bb;
        }

        //�����Χ��Ŀ����ת����
        private void camerarotate()
        {
            var mouse_x = Input.GetAxis("Mouse X");//��ȡ���X���ƶ�
            var mouse_y = -Input.GetAxis("Mouse Y");//��ȡ���Y���ƶ�
            if (Input.GetKey(KeyCode.Mouse1))
            {
                transform.Translate(Vector3.left * (mouse_x * 15f) * Time.deltaTime);
                transform.Translate(Vector3.up * (mouse_y * 15f) * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.Mouse0))
            {
                transform.RotateAround(ThisTarget.transform.position, Vector3.up, mouse_x * 5);
                transform.RotateAround(ThisTarget.transform.position, transform.right, mouse_y * 5);
            }
        }

        //�������������
        private void camerazoom()
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                transform.GetComponent<Camera>().fieldOfView = Mathf.Clamp(Camera.main.fieldOfView - 5, 30, 90);
            }
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                transform.GetComponent<Camera>().fieldOfView = Mathf.Clamp(Camera.main.fieldOfView + 5, 30, 90);
            }
        }

        #region ��ʱ����

        /// <summary>
        /// ����Ӧ�ӽ�
        /// </summary>
        /// <param name="v3">��Ҫ�ۿ��ĵ�</param>
        /// <param name="visualAngle">�ӽ���ά����</param>
        public void SetFocues(Transform v3, Vector3 visualAngle)
        {
            float dis = BestSize(v3);
            ThisTarget = v3;
            CamMoveTarget.position = v3.position + new Vector3(dis * camDisMultiple * visualAngle.x, dis * camDisMultiple * 0.6f * visualAngle.y, dis * camDisMultiple * visualAngle.z);
            CamMoveTarget.LookAt(v3.position);
            this.transform.DORotate(CamMoveTarget.eulerAngles, 1);
            this.transform.DOMove(CamMoveTarget.position, 1);
        }

        #endregion
    }

}

