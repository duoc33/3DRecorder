using QFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.Progress;

namespace Record
{
    #region BaseData
    /// <summary>
    /// 回放数据类型 拥有的基本接口功能: 
    /// 1.比较方式
    /// 2.防止引用拷贝用，数据需要值赋值
    /// 3.获取数据的方法。
    /// 4.应用数据的方法。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IData<T>
    {
        bool CompareTo(T other);
        void AssignmentParams(T origin, T other);
        void GetCurrentRecordData(T recordData, Transform transform, float NowTime);
        void ApplyRecordData(T recordData, Transform transform, RecorderDataReader mDataReader);
    }
    [Serializable]
    public abstract class AbstractRecordData : IData<AbstractRecordData>
    {
        /// <summary>
        /// 父物体唯一ID
        /// </summary>
        public int ParentViewID;
        /// <summary>
        /// 父物体实例ID
        /// </summary>
        public int InstantiatedID;
        /// <summary>
        /// 当前数据被添加时间
        /// </summary>
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
        /// 比较数据
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
        /// 获取数据
        /// ParentViewID 为 -1 时，当前没有父物体
        /// ParentViewID 为 -2 时，表示当前父物体未设置ViewID，并没有加BaseView组件
        /// ParentViewID 为 -3 时，表示当前父物体没有设置ViewID
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
                        Debug.LogError(" 当前父物体: "+ transform.parent.name +"没有设置ViewID,但有BaseView");
                    }
                }
                else
                {
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
        /// 应用数据
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
                if (recordData.ParentViewID != -2&&recordData.ParentViewID!=-3) {
                    foreach (SingleObjectInfo item in mDataReader.ViewID2SOI.Get(recordData.ParentViewID))
                    {
                        if (item.InstantiatedID == recordData.InstantiatedID)
                        {
                            if (item.SingleView == null)
                            {
                                Debug.LogError("逻辑错误，不存在已经被销毁的物体去实例新的物体");
                            }
                            else
                            {
                                transform.SetParent(item.SingleView.transform);
                            }
                            break;
                        }
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
        /// 值拷贝
        /// </summary>
        /// <param name="origin">本身</param>
        /// <param name="other">新获得的</param>
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
    }
    #endregion

}
