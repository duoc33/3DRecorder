using QFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Record
{
    #region BaseData
    public interface IData
    {
        bool CompareTo(AbstractRecordData other);
        void GetCurrentRecordData(AbstractRecordData recordData, Transform transform, float NowTime);
        void ApplyRecordData(AbstractRecordData recordData, Transform transform, RecorderDataReader mDataReader);
    }
    [Serializable]
    public abstract class AbstractRecordData : IData
    {
        public int ParentViewID;
        public int InstantiatedID;
        public float TheDataBeAddedTime;
        public bool IsActive;
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;
        protected abstract bool Comparer(AbstractRecordData origin, AbstractRecordData other);
        protected abstract void GetRecordData(AbstractRecordData recordData, Transform transform);
        protected abstract void SetRecordData(Transform transform,AbstractRecordData recordData);
        protected abstract void AssignData(AbstractRecordData origin, AbstractRecordData other);
        /// <summary>
        /// �Ƚ�����
        /// </summary>
        /// <param name="other"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public bool CompareTo(AbstractRecordData other)
        {
            if (this.ParentViewID == other.ParentViewID &&
            this.IsActive == other.IsActive &&
            this.Position == other.Position &&
            this.Rotation == other.Rotation &&
            this.Scale == other.Scale && 
            this.InstantiatedID == other.InstantiatedID &&
            Comparer(this,other))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// ��ȡ����
        /// ParentViewID Ϊ -1 ʱ����ǰû�и�����
        /// ParentViewID Ϊ -2 ʱ����ʾ��ǰ������δ����ViewID����û�м�BaseView���
        /// ParentViewID Ϊ -3 ʱ����ʾ��ǰ������û������ViewID
        /// </summary>
        /// <param name="recordData"></param>
        /// <param name="transform"></param>
        /// <param name="NowTime"></param>
        public void GetCurrentRecordData(AbstractRecordData recordData, Transform transform, float NowTime)
        {
            if (transform.parent != null)
            {
                if (transform.parent.GetComponent<RecordObjectView>() != null)
                {
                    recordData.ParentViewID = transform.parent.GetComponent<RecordObjectView>().ViewID;
                    recordData.InstantiatedID = transform.parent.GetComponent<RecordObjectView>().InstantiatedID;
                    if (recordData.ParentViewID == -3) {
                        Debug.LogError(" ��ǰ������: "+ transform.parent.name +"û������ViewID,����BaseView");
                    }
                }
                else
                {
                    #region Debug Part
                    string debugPath = DebugLog + "/" + transform.name + ".rd";
                    if (!File.Exists(debugPath))
                    {
                        using (FileStream fs = File.Create(debugPath))
                        {
                            fs.Flush();
                            fs.Close();
                        }
                        using (FileStream fs = new FileStream(debugPath, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            fs.Write(Encoding.UTF8.GetBytes(transform.name + " �����ĸ�����û������ RecordObjectView�����ǲ����ڸ�����任��"));
                            fs.Flush();
                            fs.Close();
                        }
                    }
                    #endregion

                    recordData.ParentViewID = -2;
                    recordData.InstantiatedID = 0;
                }
            }
            else
            {
                recordData.ParentViewID = -1;
                recordData.InstantiatedID = 0;
            }
            recordData.TheDataBeAddedTime = NowTime;
            recordData.IsActive = transform.gameObject.activeSelf;
            recordData.Position = transform.localPosition;
            recordData.Rotation = transform.localEulerAngles;
            recordData.Scale = transform.localScale;
            GetRecordData(recordData, transform);
        }
        /// <summary>
        /// Ӧ������
        /// </summary>
        /// <param name="recordData"></param>
        /// <param name="transform"></param>
        /// <param name="mDataReader"></param>
        public void ApplyRecordData(AbstractRecordData recordData,Transform transform,RecorderDataReader mDataReader)
        {
            if (recordData.ParentViewID == -1) {
                transform.SetParent(null); 
            }
            else {
                foreach (SingleObjectInfo item in mDataReader.ViewID2SOI.Get(recordData.ParentViewID))
                {
                    if (item.InstantiatedID == recordData.InstantiatedID) {
                        if (item.SingleView == null) {
                            Debug.LogError("�߼����󣬲������Ѿ������ٵ�����ȥʵ���µ�����");
                        }
                        else
                        {
                            transform.SetParent(item.SingleView.transform);
                        }
                        break;
                    }
                }
            }
            transform.gameObject.SetActive(recordData.IsActive);
            transform.localPosition = recordData.Position;
            transform.localEulerAngles = recordData.Rotation;
            transform.localScale = recordData.Scale;
            SetRecordData(transform,recordData);
        }
        /// <summary>
        /// ֵ����
        /// </summary>
        /// <param name="origin">����</param>
        /// <param name="other">�»�õ�</param>
        public void AssignmentParams(AbstractRecordData origin, AbstractRecordData other)
        {
            origin.ParentViewID = other.ParentViewID;
            origin.InstantiatedID = other.InstantiatedID;
            origin.Position = other.Position;
            origin.Rotation = other.Rotation;
            origin.Scale = other.Scale;
            origin.TheDataBeAddedTime = other.TheDataBeAddedTime;
            origin.IsActive = other.IsActive;
            AssignData(origin, other);
        }

        string DebugLog = Application.streamingAssetsPath + "/DegLog";
    }
    #endregion

}
