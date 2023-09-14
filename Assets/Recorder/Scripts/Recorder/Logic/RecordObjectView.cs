using QFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        [HideInInspector]
        public float DetectInterval = 0;
        #endregion

        #region 字段
        //回放状态 
        private StateModel mStateModel;
        public string SavePath => Path.Combine(this.GetModel<PathModel>().SavePath, ViewID.ToString()).Replace("\\", "/");
        /// <summary>
        /// 结束时间记录
        /// </summary>
        [HideInInspector]
        public float EndTime;
        [HideInInspector]
        public AbstractRecordData mRecordData;
        public AbstractRecordData GetDataType() => DefineDataType();
        public AbstractRecordData GetDeserializeType(string Json) => DeserializeType(Json);
        //public T GetDeserializeType(string Json) => JsonUtility.FromJson<T>(Json);
        #endregion

        #region MonoBehaviour相关的初始化
        protected virtual void Start()
        {
            //mRecordData = mRecordData.GetDataIns() as AbstractRecordData;
            mRecordData = DefineDataType();
            if (mRecordData == null) return;
            mStateModel = this.GetModel<StateModel>();
            mStateModel.State.RegisterWithInitValue(UpdateState).UnRegisterWhenGameObjectDestroyed(gameObject);
        }
        private void OnDestroy()
        {
            if (mStateModel.State.Value == StateType.Recording)
            {
                this.SendCommand<ObjectEndRecordCommand>(new ObjectEndRecordCommand(this));
                ExitRecording();
            }
        }
        #endregion

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
      
        #region 私有方法
        
        private void RecordObjectEnterRecording()
        {
            EnterRecording();
            this.SendCommand<ObjectLoadedInRecordCommand>(new ObjectLoadedInRecordCommand(this));
        }
        /// <summary>
        /// 观看回放初始化
        /// </summary>
        private void RecordObjectEnterWatching()
        {
            EnterWatching();
            this.SendCommand<ObjectLoadedInWatchingCommand>(new ObjectLoadedInWatchingCommand(this));
        }
        #endregion

        #region 重写功能
        protected abstract void EnterRecording();
        protected abstract void ExitRecording();
        protected abstract void EnterWatching();
        protected abstract AbstractRecordData DefineDataType();
        protected abstract AbstractRecordData DeserializeType(string Json);
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


    }
}

