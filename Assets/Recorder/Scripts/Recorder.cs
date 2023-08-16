using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Record
{
    /// <summary>
    /// 回放器说明
    /// 1.要求回放的观看的物体一般为static物体(不强制要求)，需要回放的物体一定是会动的或者是拥有逻辑表现的不同并且又RecordObjectView组件。
    /// 2.要求回放的物体有又预制件加载而来，并且其名称加载到场景中与预制件名称一致(去掉(Clone))。
    /// 3.要求回放观看的物体的名称一定是为一的，也就是预制件名称不能又重叠。包括其预制件下的子物体。
    /// </summary>
    public class Recorder :BaseView
    {
        private static Recorder instance;
        public static Recorder Instance => instance;
        public string SavePath => mPathModel.SavePath;

        private PathModel mPathModel;
        private StateModel mStateModel;
        private RecordObjectLoadPool mRecordObjectLoadPool;
        private Timer mTimer;
        #region Mono 生命周期
        private void Awake()
        {
            if (instance != null)
            {
                return;
            }
            instance = this;
            MasterTime = 0;
            mPathModel = this.GetModel<PathModel>();    
            mStateModel = this.GetModel<StateModel>();
            mRecordObjectLoadPool = this.GetModel<RecordObjectLoadPool>();
            mTimer = this.GetSystem<Timer>();

#if false
            SetSaveOrReadPath(Application.streamingAssetsPath, "Record");
            SetRecorderMode(StateType.Recording);
#endif
        }
        #endregion
        private float MasterTime;
        private bool IsEnterWatching;
        private void Update()
        {
            if (!IsEnterWatching) return;
            if (Input.GetKey(KeyCode.RightArrow)) {
                mTimer.SetForward(true);
                MasterTime = mTimer.MasterTime;
            }
            if (Input.GetKey(KeyCode.LeftArrow)){
                mTimer.SetForward(false);
                MasterTime = mTimer.MasterTime;
            }
        }

        #region 模式选择和路径选择初始化
        public void SetSaveOrReadPath(string SavePath, string fileName) => mPathModel.SetSavePath(SavePath, fileName);
        public void SetPause(bool isPause) => mTimer.SetPause(isPause);
        public void SetForward(bool isForward) =>mTimer.SetForward(isForward);
        public void SetRecorderMode(StateType stateType) => SetMode(stateType);
        #endregion

        #region 私有方法
        private void SetMode(StateType stateType)
        {
            switch (stateType)
            {
                case StateType.None:
                    IsEnterWatching = false;
                    mStateModel.State.Value = StateType.None;
                    break;
                case StateType.Recording:
                    IsEnterWatching = false;
                    this.SendCommand<InitInRecordingCommand>();
                    break;
                case StateType.Watching:
                    IsEnterWatching = true;
                    this.SendCommand<InitInWatchingCommand>();
                    break;
                case StateType.Resume:
                    IsEnterWatching = true;
                    break;
                case StateType.Pause:
                    IsEnterWatching = false;
                    break;
                default:
                    IsEnterWatching = false;
                    break;
            }
        }
        #endregion


    }
    //public class 
}

