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
        #region �������Ժ��ֶ�
        /// <summary>
        /// ViewID = -3 ��ʾδ����ViewID
        /// </summary>
        public int ViewID = -3;
        /// <summary>
        /// Clone�����ĵڼ�����ID
        /// </summary>
        [HideInInspector]
        public int InstantiatedID = 0;
        /// <summary>
        /// �ļ�������
        /// </summary>
        public long FileLength { get; set; }
        /// <summary>
        /// ��ǰ��������ݴ洢·��
        /// </summary>
        public string ObjectSavePath { get; set; }
        /// <summary>
        /// �����������
        /// </summary>
        protected float DetectInterval = 0;
        /// <summary>
        /// ��������
        /// </summary>
        private AbstractRecordData mRecordData;
        #endregion

        #region �ֶ�
        //��ȡʱʹ�õ��ֶΣ���ǰ֡�������ļ����е�λ��
        private long CurrentIndexInStream = 0;
        private DataWriteUtility mDataWriteUtility;
        private DataReadUtility mDataReadUtility;
        //�ط�״̬ 
        private StateModel mStateModel;
        private Timer mTimer;
        private RecorderDataCenter mDataCenter;
        //�м�ƴ��·����һ��
        public string SavePath => Path.Combine(this.GetModel<PathModel>().SavePath, ViewID.ToString()).Replace("\\", "/");
        #endregion

        #region MonoBehaviour��صĳ�ʼ��
        protected virtual void Start()
        {
            mRecordData = InitRecordDataType();
            if (mRecordData == null)
            {
                Debug.LogError("û��ָ����������");
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

        #region ��¼���ݸ��� ����
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
        /// ʵʱ��⣬д����
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
        /// ʵʱ��������
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

        #region ˽�з���
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
        /// ¼�ƽ���
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
        /// �ۿ��طų�ʼ��
        /// </summary>
        private void RecordObjectEnterWatching()
        {
            EnterWatching();
            StartCoroutine(ReadUpdate());
        }


        #region ��д����
        protected abstract void EnterRecording();
        protected abstract void ExitRecording();
        protected abstract AbstractRecordData DefineDataType();
        protected abstract void EnterWatching();
        #endregion


        /// <summary>
        /// �Ƴ�ָ����������
        /// </summary>
        /// <param name="go">GameObject</param>
        /// <param name="definedType">�������</param>
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

