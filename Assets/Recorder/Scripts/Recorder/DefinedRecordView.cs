using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Record
{
    /// <summary>
    /// �Զ���طż������View���
    /// </summary>
    public class DefinedRecordView : RecordObjectView
    {
        /// <summary>
        /// �����
        /// </summary>
        [HideInInspector]
        public float mDetectInterval = 0;
        protected override void Start() => base.Start();
        /// <summary>
        /// ��ʼ��¼
        /// </summary>
        protected override void EnterRecording() => DetectInterval = mDetectInterval;
        /// <summary>
        /// ��ʼ�ۿ�
        /// </summary>
        protected override void EnterWatching() { }
        /// <summary>
        /// �˳���¼
        /// </summary>
        protected override void ExitRecording() { }
        /// <summary>
        /// ָ����¼��������Ϣ����
        /// </summary>
        /// <returns></returns>
        protected override AbstractRecordData DefineDataType()=> new DefinedRecordData();
        /// <summary>
        /// ָ�������л���ֵ��ʽ
        /// </summary>
        /// <param name="Json"></param>
        /// <returns></returns>
        protected override AbstractRecordData DeserializeType(string Json) => JsonUtility.FromJson<DefinedRecordData>(Json);
    }
}

