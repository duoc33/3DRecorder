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
    /*第一步设置指定文件路径和名称
      第二步设置模式,设置模式即开始执行
      Recording 录制模式，开始录制
      Watching 观看模式，开始观看，使用StepForwardByFrame 和StepBackwardByFrame 进行观看
    */
    /// <summary>
    /// 回放器说明
    /// 1.需要回放的物体一定是会动的或者是拥有逻辑表现的不同。并加上继承RecordObjectView组件。
    /// </summary>
    [DisallowMultipleComponent]
    public class Recorder :BaseView
    {
        #region API_ALL
        

        /// <summary>
        /// 设置文件写入的路径和文件名，或者是其读取的路径和文件名称
        /// </summary>
        /// <param name="SavePath"></param>
        /// <param name="fileName"></param>
        public void SetSaveOrReadPath(string SavePath, string fileName) => mPathModel.SetSavePath(SavePath, fileName);
       
        /// <summary>
        /// 设置模式
        /// </summary>
        /// <param name="stateType"></param>
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

        /// <summary>
        /// 观看前一帧
        /// </summary>
        public void StepForwardByFrame()
        {
            if (!IsEnterWatching) return;
            if (!mTimer.IsForward)
            {
                mTimer.SetForward(true);
            }
            mMasterTime = mTimer.MasterTime;
        }
        /// <summary>
        /// 观看后一帧
        /// </summary>
        public void StepBackwardByFrame()
        {
            if (!IsEnterWatching) return;
            if (mTimer.IsForward)
            {
                mTimer.SetForward(false);
            }
            mMasterTime = mTimer.MasterTime;
        }


        /// <summary>
        /// 设置暂停，仅回放模式使用
        /// </summary>
        /// <param name="isPause"></param>
        public void SetPause(bool isPause) => mTimer.SetPause(isPause);
        /// <summary>
        /// 向前观看或者向后观看
        /// </summary>
        /// <param name="isForward"></param>
        public void SetForward(bool isForward) => mTimer.SetForward(isForward);
        #endregion

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

        #region Field
        private PathModel mPathModel;//路径管理
        private StateModel mStateModel;//状态数据
        private Timer mTimer;//时间管理器
        private RecorderDataCenter mRecorderCenter;//数据处理中心
        private DataWriteUtility mDataWriteUtility;//数据写入功能
        private DataReadUtility mDataReadUtility;//数据读取功能
        private float mMasterTime;//当前时间，观看模式中。
        private float mEndTime;//结束时间,录制模式中记录和使用。
        /// <summary>
        /// 当前是否进入观看模式
        /// </summary>
        [HideInInspector]
        public bool IsEnterWatching;
        #endregion

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
            if (Input.GetKey(KeyCode.RightArrow))
            {
                StepForwardByFrame();
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                StepBackwardByFrame();
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
            //将它被销毁的时间记录下来
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

