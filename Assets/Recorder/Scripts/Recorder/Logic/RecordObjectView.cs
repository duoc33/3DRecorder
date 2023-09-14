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
        [HideInInspector]
        public float DetectInterval = 0;
        #endregion

        #region �ֶ�
        //�ط�״̬ 
        private StateModel mStateModel;
        public string SavePath => Path.Combine(this.GetModel<PathModel>().SavePath, ViewID.ToString()).Replace("\\", "/");
        /// <summary>
        /// ����ʱ���¼
        /// </summary>
        [HideInInspector]
        public float EndTime;
        [HideInInspector]
        public AbstractRecordData mRecordData;
        public AbstractRecordData GetDataType() => DefineDataType();
        public AbstractRecordData GetDeserializeType(string Json) => DeserializeType(Json);
        //public T GetDeserializeType(string Json) => JsonUtility.FromJson<T>(Json);
        #endregion

        #region MonoBehaviour��صĳ�ʼ��
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
      
        #region ˽�з���
        
        private void RecordObjectEnterRecording()
        {
            EnterRecording();
            this.SendCommand<ObjectLoadedInRecordCommand>(new ObjectLoadedInRecordCommand(this));
        }
        /// <summary>
        /// �ۿ��طų�ʼ��
        /// </summary>
        private void RecordObjectEnterWatching()
        {
            EnterWatching();
            this.SendCommand<ObjectLoadedInWatchingCommand>(new ObjectLoadedInWatchingCommand(this));
        }
        #endregion

        #region ��д����
        protected abstract void EnterRecording();
        protected abstract void ExitRecording();
        protected abstract void EnterWatching();
        protected abstract AbstractRecordData DefineDataType();
        protected abstract AbstractRecordData DeserializeType(string Json);
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


    }
}

