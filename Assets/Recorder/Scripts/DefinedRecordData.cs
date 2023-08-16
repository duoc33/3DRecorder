using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Record
{
    /// <summary>
    /// 自定义数据，比较方式、获取方式、应用方式
    /// </summary>
    [Serializable]
    public class DefinedRecordData : AbstractRecordData
    {
        /// <summary>
        /// 增加数据比较方式
        /// </summary>
        /// <param name="other">另一个数据</param>
        /// <param name="transform">当前回放对象Trans组件</param>
        /// <returns></returns>
        protected override bool Comparer(AbstractRecordData other, Transform transform) => true;
        /// <summary>
        /// 给新增加的数据定义recordData的赋值方式
        /// </summary>
        /// <param name="recordData"></param>
        /// <param name="transform"></param>
        protected override void GetRecordData(ref AbstractRecordData recordData, Transform transform) { }
        /// <summary>
        /// 给新增加的数据定义Transform的赋值方式
        /// </summary>
        /// <param name="recordData"></param>
        /// <param name="transform"></param>
        protected override void SetRecordData(AbstractRecordData recordData, Transform transform) { }
    }
}

