using QFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Record
{
    #region BaseData
    public interface IData
    {
        bool CompareTo(AbstractRecordData other, Transform transform = null);
        void GetCurrentRecordData(ref AbstractRecordData recordData, Transform transform, float NowTime);
        void ApplyRecordData(AbstractRecordData recordData, double masterTime, bool IsForward, Transform transform, Dictionary<int, List<RecordObjectView>> WatchingModeDic);
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
        protected abstract bool Comparer(AbstractRecordData other, Transform transform);
        protected abstract void GetRecordData(ref AbstractRecordData recordData, Transform transform);
        protected abstract void SetRecordData(AbstractRecordData recordData, Transform transform);
        /// <summary>
        /// �Ƚ�����
        /// </summary>
        /// <param name="other"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public bool CompareTo(AbstractRecordData other, Transform transform)
        {
            if (this.ParentViewID == other.ParentViewID &&
            this.IsActive == other.IsActive &&
            this.Position == other.Position &&
            this.Rotation == other.Rotation &&
            this.Scale == other.Scale && 
            this.InstantiatedID == other.InstantiatedID &&
            Comparer(other, transform))
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
        public void GetCurrentRecordData(ref AbstractRecordData recordData, Transform transform, float NowTime)
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
                    recordData.ParentViewID = -2;
                    recordData.InstantiatedID = 0;
                    Debug.LogError("��ǰ������û�м�BaseView���");
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
            GetRecordData(ref recordData, transform);
        }
        /// <summary>
        /// Ӧ������
        /// </summary>
        /// <param name="recordData"></param>
        /// <param name="masterTime"></param>
        /// <param name="IsForward"></param>
        /// <param name="transform"></param>
        /// <param name="WatchingModeDic"></param>
        public void ApplyRecordData(AbstractRecordData recordData,double masterTime,bool IsForward,Transform transform,Dictionary<int,List<RecordObjectView>> WatchingModeDic)
        {
            if (IsForward) 
                if (recordData.TheDataBeAddedTime > masterTime) return;
            else 
                if (recordData.TheDataBeAddedTime < masterTime) return;
            if (recordData.ParentViewID == -1) { 
                transform.SetParent(null); 
            }
            else {
                if (WatchingModeDic.ContainsKey(recordData.ParentViewID)) {
                    WatchingModeDic[recordData.ParentViewID].ForEach((x) => {
                        if (x.InstantiatedID == recordData.InstantiatedID) {
                            transform.SetParent(x.transform);
                            return;
                        }
                    });
                }
                else {
                    Debug.Log(recordData.ParentViewID);
                    Debug.LogError(transform.name+ " �ĸ�����û�б���¼��");
                }
            }
            transform.gameObject.SetActive(recordData.IsActive);
            transform.localPosition = recordData.Position;
            transform.localEulerAngles = recordData.Rotation;
            transform.localScale = recordData.Scale;
            SetRecordData(recordData,transform);
        }
    }
    #endregion

}
