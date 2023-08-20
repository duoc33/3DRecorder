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
    [DisallowMultipleComponent]
    public class Recorder :BaseView
    {
        private static Recorder instance;
        public static Recorder Instance {
            get {
                if (instance != null) return instance;
                instance = FindObjectOfType<Recorder>();
                if (instance == null)
                {
                    new GameObject("Singleton of " + typeof(Recorder)).AddComponent<Recorder>();
                }
                return instance;
            }
        }

        private PathModel mPathModel;
        private StateModel mStateModel;
        private Timer mTimer;
        private RecorderDataCenter mRecorderCenter;
        private float MasterTime;
        private bool IsEnterWatching;

        #region Mono 生命周期
        private void Awake()
        {
            if (instance != null) return;
            instance = this;
            MasterTime = 0;
            IsEnterWatching = false;
            mRecorderCenter = this.GetModel<RecorderDataCenter>();
            mPathModel = this.GetModel<PathModel>();
            mStateModel = this.GetModel<StateModel>();
            mTimer = this.GetSystem<Timer>();
            
            this.RegisterEvent<ObjectLoadedInRecordingEvent>((e) => {
                mRecorderCenter.AddBaseViewOnRecording(e.EventParamBaseView);
            }).UnRegisterWhenGameObjectDestroyed(this.gameObject);
        }
        #endregion

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
        #endregion

        #region 私有方法
        public void SetMode(StateType stateType)
        {
            switch (stateType)
            {
                case StateType.None:
                    IsEnterWatching = false;
                    mStateModel.SetState(StateType.None);
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
                    mStateModel.SetState(StateType.Resume);
                    break;
                case StateType.Pause:
                    IsEnterWatching = false;
                    mStateModel.SetState(StateType.Pause);
                    break;
                default:
                    IsEnterWatching = false;
                    mStateModel.SetState(StateType.None);
                    break;
            }
        }
        #endregion


    }
    #region 单例
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T instance;
        public static T Instance
        {
            //实现按需加载
            get
            {
                if (instance != null) return instance;
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    new GameObject("Singleton of " + typeof(T)).AddComponent<T>();
                }
                return instance;
            }
        }
        private void Awake()
        {
            instance = this as T;
            Init();
        }
        protected abstract void Init();
    }
    #endregion
}

