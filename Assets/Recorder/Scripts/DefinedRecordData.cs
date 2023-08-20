using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace Record
{
    /// <summary>
    /// �Զ������ݣ��ȽϷ�ʽ����ȡ��ʽ��Ӧ�÷�ʽ
    /// </summary>
    [Serializable]
    public class DefinedRecordData : AbstractRecordData
    {
        public string MyTest = "CC_Test";

        /// <summary>
        /// ��ֵ��ʽ
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="other"></param>
        /// <exception cref="NotImplementedException"></exception>
        protected override void AssignData(AbstractRecordData origin, AbstractRecordData other)
        {
            (origin as DefinedRecordData).MyTest = (other as DefinedRecordData).MyTest;
        }
        /// <summary>
        /// �������ݱȽϷ�ʽ
        /// </summary>
        /// <param name="other">��һ������</param>
        /// <param name="transform">��ǰ�طŶ���Trans���</param>
        /// <returns></returns>
        protected override bool Comparer(AbstractRecordData origin, AbstractRecordData other) {
            return (other as DefinedRecordData).MyTest== (origin as DefinedRecordData).MyTest;
        }
        /// <summary>
        /// �������ӵ����ݶ���recordData�ĸ�ֵ��ʽ
        /// </summary>
        /// <param name="recordData"></param>
        /// <param name="transform"></param>
        protected override void GetRecordData(AbstractRecordData recordData, Transform transform) 
        {
            (recordData as DefinedRecordData).MyTest = this.MyTest;
        }
        /// <summary>
        /// �������ӵ����ݶ���Transform�ĸ�ֵ��ʽ
        /// </summary>
        /// <param name="recordData"></param>
        /// <param name="transform"></param>
        protected override void SetRecordData(Transform transform,AbstractRecordData recordData) { }
    }
}

