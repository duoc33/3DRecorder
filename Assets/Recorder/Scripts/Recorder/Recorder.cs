using QFramework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        private DataWriteUtility mDataWriteUtility;
        private DataReadUtility mDataReadUtility;
        private float mMasterTime;
        private float mEndTime;

        public bool IsEnterWatching;

        #region Mono 生命周期
        private void Awake()
        {
            if (instance != null) return;
            instance = this;
            mMasterTime = 0;
            IsEnterWatching = false;
            mRecorderCenter = this.GetModel<RecorderDataCenter>();
            mPathModel = this.GetModel<PathModel>();
            mStateModel = this.GetModel<StateModel>();
            mTimer = this.GetSystem<Timer>();
            mDataWriteUtility = this.GetUtility<DataWriteUtility>();
            mDataReadUtility = this.GetUtility<DataReadUtility>();
            EventRegister();
        }

        private void OnDestroy()
        {
            if (mStateModel.State.Value == StateType.Recording) {
                WriteRecordInfo();
            }
        }
        /// <summary>
        /// 事件注册
        /// </summary>
        private void EventRegister() 
        {
            #region Write Event
            this.RegisterEvent<ObjectLoadedInRecordingEvent>((e) => {
                ViewInfoInRecording VIR = mRecorderCenter.AddBaseViewOnRecording(e.View);
                VIR.WriteCoroutine = StartCoroutine(WriteUpdate(e.View));
            }).UnRegisterWhenGameObjectDestroyed(this.gameObject);

            this.RegisterEvent<ObjectEndRecordingEvent>((e) => {
                RecordObjectExit(e.View);
            }).UnRegisterWhenGameObjectDestroyed(this.gameObject);
            #endregion

            #region Read Event
            this.RegisterEvent<ObjectLoadedInWatchingEvent>((e) => {
                SingleObjectInfo singleObjectInfo = mRecorderCenter.RecorderReader.FindSoleSingleObjectInfoByView(e.View);
                if (singleObjectInfo == null) return;
                singleObjectInfo.ReadCoroutine = StartCoroutine(StartWatching(singleObjectInfo));
            }).UnRegisterWhenGameObjectDestroyed(this.gameObject);

            #endregion
        }

        private void Update()
        {
            //float x = mTimer.MasterTime;
            if (!IsEnterWatching) return;
            if (Input.GetKey(KeyCode.RightArrow))
            {
                if (!mTimer.IsForward)
                {
                    mTimer.SetForward(true);
                }
                mMasterTime = mTimer.MasterTime;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (mTimer.IsForward)
                {
                    mTimer.SetForward(false);
                }
                mMasterTime = mTimer.MasterTime;
            }
        }
        
        #endregion

        #region 模式选择和路径选择初始化
        public void SetSaveOrReadPath(string SavePath, string fileName) => mPathModel.SetSavePath(SavePath, fileName);
        public void SetPause(bool isPause) => mTimer.SetPause(isPause);
        public void SetForward(bool isForward) =>mTimer.SetForward(isForward);
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

        #region 结束录制
        public void RemoveAll()
        {
            foreach (var item in mRecorderCenter.RecordModeDic.Values)
            {
                foreach (var viewInfoInRecording in item)
                {
                    if (viewInfoInRecording.View != null)
                    {
                        Object.Destroy(viewInfoInRecording.View);
                    }
                }
            }
            WriteRecordInfo();
        }
        private void WriteRecordInfo()
        {
            mDataWriteUtility.InitFile(mPathModel.RecordInfoPath);
            mDataWriteUtility.WriteHead(mPathModel.RecordInfoPath, Mathf.CeilToInt(mEndTime));
        }
        #endregion

        #region Write
        /// <summary>
        /// 实时检测，写数据
        /// </summary>
        /// <returns></returns>
        private IEnumerator WriteUpdate(RecordObjectView View)
        {
            if(View == null) yield break;
            AbstractRecordData temp = View.GetDataType();
            AbstractRecordData mRecordData = View.GetDataType();
            
            mDataWriteUtility.WriteHead(View.ObjectSavePath, Mathf.FloorToInt(mTimer.CurrentTime));
            while (mStateModel.State.Value == StateType.Recording)
            {
                if (mTimer.CurrentTime != 0) {
                    View.EndTime = mTimer.CurrentTime;
                }
                if(View == null) yield break;
                mRecordData.GetCurrentRecordData(temp, View.transform, mTimer.CurrentTime);
                if (!mRecordData.CompareTo(temp))
                {
                    mRecordData.AssignmentParams(mRecordData, temp);
                    byte[] tempBytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(mRecordData));
                    mDataWriteUtility.WriteData(View.ObjectSavePath, tempBytes);
                }
                if (View.DetectInterval != 0)
                {
                    yield return new WaitForSeconds(View.DetectInterval);
                }
                else
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }
        /// <summary>
        /// 录制结束
        /// </summary>
        private void RecordObjectExit(RecordObjectView View)
        {
            if (!File.Exists(View.ObjectSavePath)) return;
            mDataWriteUtility.WriteHead(View.ObjectSavePath, Mathf.FloorToInt(View.EndTime));
            if (mEndTime < View.EndTime) 
            {
                mEndTime = View.EndTime;
            }
        }
        #endregion

        #region Read
        private IEnumerator StartWatching(SingleObjectInfo View) 
        {
            if (View.CurrentIndexInStream == 4) {
                string data = mDataReadUtility.ReadNextData(View.SingleView.ObjectSavePath, ref View.CurrentIndexInStream);
                View.SingleView.mRecordData = View.SingleView.GetDeserializeType(data);
                View.SingleView.mRecordData.ApplyRecordData(View.SingleView.mRecordData, View.SingleView.transform, mRecorderCenter.RecorderReader);
            }
            if (View.CurrentIndexInStream == View.StreamLength - 4) {
                string data = mDataReadUtility.ReadPreData(View.SingleView.ObjectSavePath, ref View.CurrentIndexInStream);
                View.SingleView.mRecordData = View.SingleView.GetDeserializeType(data);
                View.SingleView.mRecordData.ApplyRecordData(View.SingleView.mRecordData, View.SingleView.transform, mRecorderCenter.RecorderReader);
            }
            while (mStateModel.State.Value == StateType.Watching) {
                if (View.SingleView == null) yield break;
                if (mTimer.IsForward) 
                {
                    ReadForward(View);
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    ReadBackward(View);
                    yield return new WaitForEndOfFrame();
                }
            }
        }
        /// <summary>
        /// 先前读数据
        /// </summary>
        /// <param name="singleObjectInfo"></param>
        private void ReadForward(SingleObjectInfo singleObjectInfo)
        {
            if (singleObjectInfo.SingleView.mRecordData.TheDataBeAddedTime >= mTimer.CurrentTimeInWatching) return;
            string data = mDataReadUtility.ReadNextData(singleObjectInfo.SingleView.ObjectSavePath, ref singleObjectInfo.CurrentIndexInStream);
            if (data == null) return;
            singleObjectInfo.SingleView.mRecordData = singleObjectInfo.SingleView.GetDeserializeType(data);
            singleObjectInfo.SingleView.mRecordData.ApplyRecordData(singleObjectInfo.SingleView.mRecordData, singleObjectInfo.SingleView.transform, mRecorderCenter.RecorderReader);
        }
        /// <summary>
        /// 向后读
        /// </summary>
        private void ReadBackward(SingleObjectInfo View)
        {
            if (View.SingleView.mRecordData.TheDataBeAddedTime <= mTimer.CurrentTimeInWatching) return;
            string data = mDataReadUtility.ReadPreData(View.SingleView.ObjectSavePath,ref View.CurrentIndexInStream);
            if (data == null) return;
            View.SingleView.mRecordData = View.SingleView.GetDeserializeType(data);
            View.SingleView.mRecordData.ApplyRecordData(View.SingleView.mRecordData, View.SingleView.transform, mRecorderCenter.RecorderReader);
        }
        #endregion

    }
}

