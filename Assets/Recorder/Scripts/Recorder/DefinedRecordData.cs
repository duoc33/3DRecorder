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
        /// <summary>
        /// ��ֵ��ʽ
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="other"></param>
        /// <exception cref="NotImplementedException"></exception>
        protected override void AssignData(AbstractRecordData origin, AbstractRecordData other) { }
        /// <summary>
        /// �������ݱȽϷ�ʽ
        /// </summary>
        /// <param name="other">��һ������</param>
        /// <param name="transform">��ǰ�طŶ���Trans���</param>
        /// <returns></returns>
        protected override bool Comparer(AbstractRecordData origin, AbstractRecordData other)=>true;
        /// <summary>
        /// �������ӵ����ݶ���recordData�ĸ�ֵ��ʽ
        /// </summary>
        /// <param name="recordData"></param>
        /// <param name="transform"></param>
        protected override void GetRecordData(AbstractRecordData recordData, Transform transform) { }
        /// <summary>
        /// �������ӵ����ݶ���Transform�ĸ�ֵ��ʽ
        /// </summary>
        /// <param name="recordData"></param>
        /// <param name="transform"></param>
        protected override void SetRecordData(Transform transform,AbstractRecordData recordData) { }
    }
}

