using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Record
{
    /// <summary>
    /// �Զ������ݣ��ȽϷ�ʽ����ȡ��ʽ��Ӧ�÷�ʽ
    /// </summary>
    [Serializable]
    public class DefinedRecordData : AbstractRecordData
    {
        /// <summary>
        /// �������ݱȽϷ�ʽ
        /// </summary>
        /// <param name="other">��һ������</param>
        /// <param name="transform">��ǰ�طŶ���Trans���</param>
        /// <returns></returns>
        protected override bool Comparer(AbstractRecordData other, Transform transform) => true;
        /// <summary>
        /// �������ӵ����ݶ���recordData�ĸ�ֵ��ʽ
        /// </summary>
        /// <param name="recordData"></param>
        /// <param name="transform"></param>
        protected override void GetRecordData(ref AbstractRecordData recordData, Transform transform) { }
        /// <summary>
        /// �������ӵ����ݶ���Transform�ĸ�ֵ��ʽ
        /// </summary>
        /// <param name="recordData"></param>
        /// <param name="transform"></param>
        protected override void SetRecordData(AbstractRecordData recordData, Transform transform) { }
    }
}

