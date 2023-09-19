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
    /*��һ������ָ���ļ�·��������
      �ڶ�������ģʽ,����ģʽ����ʼִ��
      Recording ¼��ģʽ����ʼ¼��
      Watching �ۿ�ģʽ����ʼ�ۿ���ʹ��StepForwardByFrame ��StepBackwardByFrame ���йۿ�
    */
    /// <summary>
    /// �ط���˵��
    /// 1.��Ҫ�طŵ�����һ���ǻᶯ�Ļ�����ӵ���߼����ֵĲ�ͬ�������ϼ̳�RecordObjectView�����
    /// </summary>
    [DisallowMultipleComponent]
    public class Recorder :BaseView
    {
        #region API_ALL
        

        /// <summary>
        /// �����ļ�д���·�����ļ��������������ȡ��·�����ļ�����
        /// </summary>
        /// <param name="SavePath"></param>
        /// <param name="fileName"></param>
        public void SetSaveOrReadPath(string SavePath, string fileName) => mPathModel.SetSavePath(SavePath, fileName);
       
        /// <summary>
        /// ����ģʽ
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
        /// �ۿ�ǰһ֡
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
        /// �ۿ���һ֡
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
        /// ������ͣ�����ط�ģʽʹ��
        /// </summary>
        /// <param name="isPause"></param>
        public void SetPause(bool isPause) => mTimer.SetPause(isPause);
        /// <summary>
        /// ��ǰ�ۿ��������ۿ�
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
        private PathModel mPathModel;//·������
        private StateModel mStateModel;//״̬����
        private Timer mTimer;//ʱ�������
        private RecorderDataCenter mRecorderCenter;//���ݴ�������
        private DataWriteUtility mDataWriteUtility;//����д�빦��
        private DataReadUtility mDataReadUtility;//���ݶ�ȡ����
        private float mMasterTime;//��ǰʱ�䣬�ۿ�ģʽ�С�
        private float mEndTime;//����ʱ��,¼��ģʽ�м�¼��ʹ�á�
        /// <summary>
        /// ��ǰ�Ƿ����ۿ�ģʽ
        /// </summary>
        [HideInInspector]
        public bool IsEnterWatching;
        #endregion

        #region Mono ��������
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
        /// �¼�ע��
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

        #region ����¼��
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
        /// ʵʱ��⣬д����
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
        /// ¼�ƽ���
        /// </summary>
        private void RecordObjectExit(RecordObjectView View)
        {
            if (!File.Exists(View.ObjectSavePath)) return;
            //���������ٵ�ʱ���¼����
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
        /// ��ǰ������
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
        /// ����
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

