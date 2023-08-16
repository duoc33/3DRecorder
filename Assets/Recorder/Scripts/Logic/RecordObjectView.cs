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
        #region �ֶ�
        //��ȡʱʹ�õ��ֶΣ���ǰ֡�������ļ����е�λ��
        //private long CurrentIndexInStream = 0;
        public long FileLength { get; set; }
        private DataWriteUtility mDataWriteUtility;
        private DataReadUtility mDataReadUtility;

        /// <summary>
        /// �����������
        /// </summary>
        protected float DetectInterval = 0;
        //�ط�״̬ 
        private StateModel mStateModel;
        private Timer mTimer;

        protected AbstractRecordData mRecordData;

        //��ǰ��������ݴ洢·��
        public string ObjectSavePath { get; set; }
        public string SavePath => this.GetModel<PathModel>().SavePath+"/"+ViewID;
        #endregion

        #region MonoBehaviour��صĳ�ʼ��
        private void Start()
        {
            mRecordData = InitRecordDataType();
            if (mRecordData == null)
            {
                Debug.LogError("û��ָ����������");
                return;
            }
            mTimer = this.GetSystem<Timer>();
            mStateModel = this.GetModel<StateModel>();
            mStateModel.State.RegisterWithInitValue(UpdateState).UnRegisterWhenGameObjectDestroyed(gameObject);
        }
        #endregion

        #region ��¼���ݸ��� ����
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

        #region ��д���� �� ���õĹ���
        protected virtual AbstractRecordData InitRecordDataType() 
        {
            return new DefinedRecordData();
        }
        /// <summary>
        /// ����ط�ģʽ��Ҫ����
        /// </summary>
        protected virtual void RecordObjectEnterRecording() 
        {
            this.SendCommand<ObjectLoadedInRecordCommand>(new ObjectLoadedInRecordCommand(this));
        }
        protected virtual void RecordObjectEnterWatching() { }
        protected virtual void RecordObjectEnterResume() { }
        protected virtual void RecordObjectEnterPause() { }

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

