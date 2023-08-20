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
    /// �ط���˵��
    /// 1.Ҫ��طŵĹۿ�������һ��Ϊstatic����(��ǿ��Ҫ��)����Ҫ�طŵ�����һ���ǻᶯ�Ļ�����ӵ���߼����ֵĲ�ͬ������RecordObjectView�����
    /// 2.Ҫ��طŵ���������Ԥ�Ƽ����ض��������������Ƽ��ص���������Ԥ�Ƽ�����һ��(ȥ��(Clone))��
    /// 3.Ҫ��طŹۿ������������һ����Ϊһ�ģ�Ҳ����Ԥ�Ƽ����Ʋ������ص���������Ԥ�Ƽ��µ������塣
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

        #region Mono ��������
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

        #region ģʽѡ���·��ѡ���ʼ��
        public void SetSaveOrReadPath(string SavePath, string fileName) => mPathModel.SetSavePath(SavePath, fileName);
        public void SetPause(bool isPause) => mTimer.SetPause(isPause);
        public void SetForward(bool isForward) =>mTimer.SetForward(isForward);
        #endregion

        #region ˽�з���
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
    #region ����
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T instance;
        public static T Instance
        {
            //ʵ�ְ������
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

