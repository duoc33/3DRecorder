using QFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Record
{
    public abstract class RecordObjectView : BaseView
    {
        #region 基础属性和字段
        /// <summary>
        /// ViewID = -3 表示未设置ViewID
        /// </summary>
        public int ViewID = -3;
        /// <summary>
        /// Clone出来的第几个的ID
        /// </summary>
        [HideInInspector]
        public int InstantiatedID = 0;
        /// <summary>
        /// 文件流长度
        /// </summary>
        public long FileLength { get; set; }
        /// <summary>
        /// 当前对象的数据存储路径
        /// </summary>
        public string ObjectSavePath { get; set; }
        /// <summary>
        /// 检测间隔的设置
        /// </summary>
        protected float DetectInterval = 0;
        /// <summary>
        /// 数据类型
        /// </summary>
        private AbstractRecordData mRecordData;
        #endregion

        #region 字段
        //读取时使用的字段，当前帧在数据文件流中的位置
        private long CurrentIndexInStream = 0;
        private DataWriteUtility mDataWriteUtility;
        private DataReadUtility mDataReadUtility;
        //回放状态 
        private StateModel mStateModel;
        private Timer mTimer;
        private RecorderDataCenter mDataCenter;
        //中间拼接路径的一环
        public string SavePath => Path.Combine(this.GetModel<PathModel>().SavePath, ViewID.ToString()).Replace("\\", "/");
        #endregion

        #region MonoBehaviour相关的初始化
        protected virtual void Start()
        {
            mRecordData = InitRecordDataType();
            if (mRecordData == null)
            {
                Debug.LogError("没有指定数据类型");
                return;
            }
            mDataWriteUtility = this.GetUtility<DataWriteUtility>();
            mDataReadUtility = this.GetUtility<DataReadUtility>();
            mTimer = this.GetSystem<Timer>();
            mStateModel = this.GetModel<StateModel>();
            mDataCenter = this.GetModel<RecorderDataCenter>();
            mStateModel.State.RegisterWithInitValue(UpdateState).UnRegisterWhenGameObjectDestroyed(gameObject);
        }
        private void Init() { }
        private void OnDestroy()
        {
            //if (mStateModel.State.Value == StateType.Recording) 
            //{
            //    RecordObjectExit();
            //}
        }
        private void OnApplicationQuit()
        {
            if (mStateModel.State.Value == StateType.Recording) 
            {
                RecordObjectExit();
            }
        }
        #endregion

        #region 记录数据更新 功能
        private void UpdateState(StateType state)
        {
            switch (state)
            {
                case StateType.Recording:
                    RecordObjectEnterRecording();
                    break;
                case StateType.Watching:
                    RecordObjectEnterWatching();
                    break;
                case StateType.None:
                case StateType.Resume:
                case StateType.Pause:
                    break;
            }
        }
        /// <summary>
        /// 实时检测，写数据
        /// </summary>
        /// <returns></returns>
        private IEnumerator WriteUpdate()
        {
            yield return new WaitUntil(()=> File.Exists(ObjectSavePath));
            AbstractRecordData temp = InitRecordDataType();
            while (mStateModel.State.Value==StateType.Recording)
            {
                mRecordData.GetCurrentRecordData(temp,transform,mTimer.CurrentTime);
                if (!mRecordData.CompareTo(temp))
                {
                    mRecordData.AssignmentParams(mRecordData, temp);
                    mDataWriteUtility.WriteData(ObjectSavePath, mRecordData);
                }
                if (DetectInterval != 0)
                {
                    yield return new WaitForSeconds(DetectInterval);
                }
                else
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }
        /// <summary>
        /// 实时检测读数据
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReadUpdate()
        {
            yield return new WaitUntil(()=>ObjectSavePath!=null&&FileLength!=0&&InstantiatedID!=0&&mDataCenter.RcorderReader!=null);
            mRecordData = mDataReadUtility.ReadFirstData(ObjectSavePath, ref CurrentIndexInStream);
            mRecordData.ApplyRecordData(mRecordData, transform, mDataCenter.RcorderReader);
            while (mStateModel.State.Value==StateType.Watching)
            {
                if (mTimer.IsForward)
                {
                    if (mRecordData.TheDataBeAddedTime >= mTimer.CurrentTimeInWatching) {
                        yield return new WaitForEndOfFrame();
                        continue;
                    }
                    mRecordData = mDataReadUtility.ReadNextData(ObjectSavePath, ref CurrentIndexInStream);
                    mRecordData.ApplyRecordData(mRecordData, transform, mDataCenter.RcorderReader);
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    if (mRecordData.TheDataBeAddedTime <= mTimer.CurrentTimeInWatching) {
                        yield return new WaitForEndOfFrame();
                        continue;
                    }
                    mRecordData = mDataReadUtility.ReadLastData(ObjectSavePath, ref CurrentIndexInStream);
                    mRecordData.ApplyRecordData(mRecordData, transform, mDataCenter.RcorderReader);
                    yield return new WaitForEndOfFrame();
                }
            }
        }
        #endregion

        #region 私有方法
        private AbstractRecordData InitRecordDataType() 
        {
            return DefineDataType();
        }
        private void RecordObjectEnterRecording()
        {
            EnterRecording();
            this.SendCommand<ObjectLoadedInRecordCommand>(new ObjectLoadedInRecordCommand(this));
            StartCoroutine(WriteUpdate());
        }
        /// <summary>
        /// 录制结束
        /// </summary>
        private void RecordObjectExit() 
        {
            this.StopAllCoroutines();
            if (!File.Exists(ObjectSavePath)) return;
            AbstractRecordData temp = InitRecordDataType();
            mRecordData.GetCurrentRecordData(temp, transform, mTimer.CurrentTime);
            mDataWriteUtility.WriteData(ObjectSavePath, temp);
            ExitRecording();
        }
        /// <summary>
        /// 观看回放初始化
        /// </summary>
        private void RecordObjectEnterWatching()
        {
            EnterWatching();
            StartCoroutine(ReadUpdate());
        }


        #region 重写功能
        protected abstract void EnterRecording();
        protected abstract void ExitRecording();
        protected abstract AbstractRecordData DefineDataType();
        protected abstract void EnterWatching();
        #endregion


        /// <summary>
        /// 移除指定对象的组件
        /// </summary>
        /// <param name="go">GameObject</param>
        /// <param name="definedType">组件类型</param>
        protected void RemoveComponent(GameObject go, Type definedType)
        {
            foreach (var component in go.GetComponents<Component>())
            {
                if (component.GetType() == definedType)
                {
                    Destroy(component);
                }
            }
        }
        #endregion

    }
}

