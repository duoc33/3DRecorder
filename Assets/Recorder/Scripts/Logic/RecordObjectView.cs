using QFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace Record
{
    public class RecordObjectView : BaseView
    {
        #region 字段
        //读取时使用的字段，当前帧在数据文件流中的位置
        //private long CurrentIndexInStream = 0;
        public long FileLength { get; set; }
        private DataWriteUtility mDataWriteUtility;
        private DataReadUtility mDataReadUtility;

        /// <summary>
        /// 检测间隔的设置
        /// </summary>
        protected float DetectInterval = 0;
        //回放状态 
        private StateModel mStateModel;
        private Timer mTimer;

        protected AbstractRecordData mRecordData;

        //当前对象的数据存储路径
        public string ObjectSavePath { get; set; }
        public string SavePath => this.GetModel<PathModel>().SavePath+"/"+ViewID;
        #endregion

        #region MonoBehaviour相关的初始化
        private void Start()
        {
            mRecordData = InitRecordDataType();
            if (mRecordData == null)
            {
                Debug.LogError("没有指定数据类型");
                return;
            }
            mTimer = this.GetSystem<Timer>();
            mStateModel = this.GetModel<StateModel>();
            mStateModel.State.RegisterWithInitValue(UpdateState).UnRegisterWhenGameObjectDestroyed(gameObject);
        }
        #endregion

        #region 记录数据更新 功能
        private void UpdateState(StateType state)
        {
            switch (state)
            {
                case StateType.Recording:
                    RecordObjectEnterRecording();
                    StartCoroutine(WriteUpdate());
                    break;
                case StateType.Watching:
                    RecordObjectEnterWatching();
                    break;
                case StateType.Resume:
                    RecordObjectEnterResume();
                    break;
                case StateType.Pause:
                    RecordObjectEnterPause();
                    break;
                case StateType.None:
                    break;
            }
        }
        //private IEnumerator WaitForRecorderStart() { }
        public IEnumerator WriteUpdate()
        {
            AbstractRecordData temp = InitRecordDataType();
            yield return new WaitUntil(()=> File.Exists(ObjectSavePath));
            while (mStateModel.State.Value==StateType.Recording)
            {
                mRecordData.GetCurrentRecordData(ref temp,transform,mTimer.CurrentTime);
                if (!mRecordData.CompareTo(temp, transform))
                {
                    mDataWriteUtility.WriteData(ObjectSavePath, temp);
                    mRecordData = temp;
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
        }
        private IEnumerator ReadUpdate()
        {
            yield return new WaitForEndOfFrame();
        }
        private IEnumerator ResumeUpadte()
        {
            yield return new WaitForEndOfFrame();
        }
        #endregion

        #region 重写功能 和 可用的工具
        protected virtual AbstractRecordData InitRecordDataType() 
        {
            return new DefinedRecordData();
        }
        /// <summary>
        /// 进入回放模式需要做的
        /// </summary>
        protected virtual void RecordObjectEnterRecording() 
        {
            this.SendCommand<ObjectLoadedInRecordCommand>(new ObjectLoadedInRecordCommand(this));
        }
        protected virtual void RecordObjectEnterWatching() { }
        protected virtual void RecordObjectEnterResume() { }
        protected virtual void RecordObjectEnterPause() { }

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

