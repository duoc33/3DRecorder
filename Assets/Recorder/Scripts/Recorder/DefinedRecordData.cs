using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace Record
{
    /// <summary>
    /// 自定义数据，比较方式、获取方式、应用方式
    /// </summary>
    [Serializable]
    public class DefinedRecordData : AbstractRecordData
    {
        public string MyTest = "CC_Test";

        /// <summary>
        /// 赋值方式
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="other"></param>
        /// <exception cref="NotImplementedException"></exception>
        protected override void AssignData(AbstractRecordData origin, AbstractRecordData other)
        {
            (origin as DefinedRecordData).MyTest = (other as DefinedRecordData).MyTest;
        }
        /// <summary>
        /// 增加数据比较方式
        /// </summary>
        /// <param name="other">另一个数据</param>
        /// <param name="transform">当前回放对象Trans组件</param>
        /// <returns></returns>
        protected override bool Comparer(AbstractRecordData origin, AbstractRecordData other) {
            return (other as DefinedRecordData).MyTest== (origin as DefinedRecordData).MyTest;
        }
        /// <summary>
        /// 给新增加的数据定义recordData的赋值方式
        /// </summary>
        /// <param name="recordData"></param>
        /// <param name="transform"></param>
        protected override void GetRecordData(AbstractRecordData recordData, Transform transform) 
        {
            (recordData as DefinedRecordData).MyTest = this.MyTest;
        }
        /// <summary>
        /// 给新增加的数据定义Transform的赋值方式
        /// </summary>
        /// <param name="recordData"></param>
        /// <param name="transform"></param>
        protected override void SetRecordData(Transform transform,AbstractRecordData recordData) { }
    }
}

