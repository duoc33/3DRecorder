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
    public class Recorder :BaseView
    {
        private static Recorder instance;
        public static Recorder Instance => instance;
        public string SavePath => mPathModel.SavePath;

        private PathModel mPathModel;
        private StateModel mStateModel;
        private RecordObjectLoadPool mRecordObjectLoadPool;
        private Timer mTimer;
        #region Mono ��������
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

        #region ģʽѡ���·��ѡ���ʼ��
        public void SetSaveOrReadPath(string SavePath, string fileName) => mPathModel.SetSavePath(SavePath, fileName);
        public void SetPause(bool isPause) => mTimer.SetPause(isPause);
        public void SetForward(bool isForward) =>mTimer.SetForward(isForward);
        public void SetRecorderMode(StateType stateType) => SetMode(stateType);
        #endregion

        #region ˽�з���
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

